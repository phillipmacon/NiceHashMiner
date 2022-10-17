using log4net;
using log4net.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace NHM.Common
{
    public enum LogType
    {
        Info,
        Warn,
        Error
    }
    public record NHMEvent
    {
        private string Event = string.Empty;
        public NHMEvent(string text, LogType logType = LogType.Info)
        {
            var time = DateTime.Now.ToString(new CultureInfo("en-GB"));
            Event = $"[{time}]:[{logType}] {text}";
        }
        public NHMEvent(string oldEvent)
        {
            Event = oldEvent;
        }
        public override string ToString()
        {
            return Event;
        }
    }
    public class EventLogger : NotifyChangedBase
    {
        private EventLogger() { }
        public static EventLogger Instance { get; } = new EventLogger();
        static object _lock = new object();
        public static bool Enabled { get; set; } = true;
        private static string _logsRootPath => Paths.RootPath("logs");
        private List<NHMEvent> _events = new List<NHMEvent>();
        private string TAG = "EventLogger";
        public List<NHMEvent> Events
        {
            get
            {
                lock (_lock)
                {
                    return _events;
                }
            }
            set
            {
                lock (_lock)
                {
                    _events = value;
                }
                OnPropertyChanged(nameof(Events));
            }
        }
        public void AddEvent(NHMEvent ev)
        {
            lock (_lock)
            {
                _events.Add(ev);
            }
            OnPropertyChanged(nameof(Events));
        }
        public void ClearEvents()
        {
            lock (_lock)
            {
                _events.Clear();
            }
        }
        public static void Warning(string text)
        {
            Instance.CreateEvent(text, LogType.Warn);
        }
        public static void Error(string text)
        {
            Instance.CreateEvent(text, LogType.Error);
        }
        public static void Info(string text)
        {
            Instance.CreateEvent(text, LogType.Info);
        }
        private void CreateEvent(string text, LogType type)
        {
            if (!Enabled) return;
            var newEvent = new NHMEvent(text, type);
            Instance.AddEvent(newEvent);
            WriteToFile(newEvent);
        }
        public void ReadLogsFromFile()
        {
            var logFilePath = Path.Combine(_logsRootPath, "events.txt");
            using StreamReader reader = new StreamReader(logFilePath);
            List<NHMEvent> readEvents = new();
            try
            {
                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine();
                    readEvents.Add(new NHMEvent(line));
                }
                lock (_lock)
                {
                    Events = readEvents;
                }
            }
            catch(Exception e)
            {
                Logger.Error(TAG, e.Message);
            }

        }
        private void WriteToFile(NHMEvent ev)
        {
            var eventFilePath = Path.Combine(_logsRootPath, "events.txt");
            using StreamWriter writer = new(eventFilePath, append: true);
            try
            {
                writer.Write(ev.ToString());
            }
            catch (Exception e)
            {
                Logger.Warn(TAG, e.Message);
            }
        }

    }
}
