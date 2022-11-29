﻿using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using NHM.Common;
using NHM.Common.Enums;

namespace Excavator
{
    internal static class CmdConfig
    {
        class Command
        {
            [JsonProperty("id")]
            public int Id { get; set; }
            [JsonProperty("method")]
            public string Method { get; set; }
            [JsonProperty("params")]
            public List<string> Params { get; set; }
        }

        class CommandList
        {
            [JsonProperty("time", NullValueHandling = NullValueHandling.Ignore)]
            public uint? Time { get; set; } = null;
            [JsonProperty("loop", NullValueHandling = NullValueHandling.Ignore)]
            public uint? Loop { get; set; } = null;
            [JsonProperty("event", NullValueHandling = NullValueHandling.Ignore)]
            public string Event { get; set; } = null;
            [JsonProperty("commands")]
            public List<Command> Commands { get; set; } = new List<Command>();
        }

        private static string _extraLaunchParameters = "";

        private static List<string> _mappedDeviceIDs;

        private static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            Culture = CultureInfo.InvariantCulture,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        public static string CreateTemplate(IEnumerable<string> gpuUuids, string algorithmName)
        {
            return CreateDefaultTemplateAndCreateCMD("__SUBSCRIBE_PARAM_LOCATION__", "__SUBSCRIBE_PARAM_USERNAME__", gpuUuids, algorithmName);
        }

        public static string CommandFileTemplatePath(string pluginUUID)
        {
            return Paths.MinerPluginsPath(pluginUUID, "internals", "CommandLineTemplate.json");
        }

        private static List<Command> CreateInitialCommands(string subscribeLocation, string subscribeUsername, IEnumerable<string> gpuUuids, string algorithmName)
        {
            var deviceUuids = gpuUuids.ToList();
            var initialCommands = new List<Command>
                {
                    new Command { Id = 1, Method = "subscribe", Params = new List<string>{ subscribeLocation, subscribeUsername } },
                    new Command { Id = 2, Method = "algorithm.add", Params = new List<string>{ algorithmName.ToLower() } },
                };
            initialCommands.AddRange(gpuUuids.Select((gpu, index) => new Command { Id = index + 3, Method = "worker.add", Params = new List<string> { algorithmName.ToLower(), gpu } }));


            if (_extraLaunchParameters != "")
            {
                var elps = _extraLaunchParameters.Split(' ');
                for (var i = 0; i < elps.Length; i++)
                {
                    if (i < elps.Length - 1 && elps[i].Contains('.') && elps[i+1].Contains(','))
                    {
                        var separatedElps = elps[i+1].Split(',');
                        for (var j = 0; j < deviceUuids.Count; j++)
                        {
                            initialCommands.Add(new Command
                            {
                                Id = initialCommands.Count + 1,
                                Method = elps[i],
                                Params = new List<string>()
                                {
                                   _mappedDeviceIDs[j].ToString(),
                                   separatedElps[j].ToString()
                                }
                            });
                        }
                        i++;
                    }
                    else if ((elps[i].Contains('.') && i == elps.Length - 1) || (elps[i].Contains('.') && elps[i + 1].Contains('.')))
                    {
                        for (var j = 0; j < deviceUuids.Count; j++)
                        {
                            initialCommands.Add(new Command
                            {
                                Id = initialCommands.Count + 1,
                                Method = elps[i],
                                Params = new List<string>()
                                {
                                   _mappedDeviceIDs[j].ToString()
                                }
                            });
                        }
                    }
                    else
                    {
                        initialCommands.Add(new Command
                        {
                            Id = initialCommands.Count + 1,
                            Method = elps[i],
                            Params = new List<string>()
                            {
                                deviceUuids.FirstOrDefault(),
                                elps[i+1]
                            }
                        });
                        i++;
                    }
                }
            }
            return initialCommands;
        }

        private static string CreateDefaultTemplateAndCreateCMD(string subscribeLocation, string subscribeUsername, IEnumerable<string> gpuUuids, string algorithmName)
        {
            try
            {
                var commandListTemplate = new List<CommandList>
                {
                    new CommandList
                    {
                        Time = 0,
                        Commands = CreateInitialCommands(subscribeLocation, subscribeUsername, gpuUuids, algorithmName),
                    },
                    new CommandList
                    {
                        Event = "on_quit",
                        Commands = new List<Command>{ },
                    }
                };
                return JsonConvert.SerializeObject(commandListTemplate, Formatting.Indented);
            }
            catch (Exception e)
            {
                Logger.Error("Excavator.CmdConfig", $"CreateCommandFile error {e.Message}");
                return null;
            }
        }
        private static string[] _invalidTemplateMethods = new string[] { "subscribe", "algorithm.add", "worker.add" };
        private static string ParseTemplateFileAndCreateCMD(string templateFilePath, IEnumerable<string> gpuUuids, string subscribeLocation, string subscribeUsername, string algorithmName)
        {
            if (!File.Exists(templateFilePath)) return null;
            try
            {
                var template = JsonConvert.DeserializeObject<List<CommandList>>(File.ReadAllText(templateFilePath), _jsonSettings);
                var validCmds = template
                    .Where(cmd => cmd.Commands.All(c => !_invalidTemplateMethods.Contains(c.Method)))
                    .Select(cmd => (cmd, commands: cmd.Commands.Where(c => IsValidSessionCommand(c, gpuUuids)).ToList()))
                    .Where(p => p.commands.Any())
                    .ToArray();
                foreach (var (cmd, commands) in validCmds)
                {
                    cmd.Commands = commands;
                }
                var commandListTemplate = new List<CommandList>
                {
                    new CommandList
                    {
                        Time = 0,
                        Commands = CreateInitialCommands(subscribeLocation, subscribeUsername, gpuUuids, algorithmName),
                    },
                };
                if (validCmds.Any()) commandListTemplate.AddRange(validCmds.Select(p => p.cmd));
                return JsonConvert.SerializeObject(commandListTemplate, Formatting.Indented, _jsonSettings);
            }
            catch (Exception e)
            {
                Logger.Error("Excavator.CmdConfig", $"ParseTemplateFile error {e.Message}");
                return null;
            }
        }

        private static bool IsValidSessionCommand(Command command, IEnumerable<string> gpuUuids)
        {
            var anyMissingGpuUuidParams = command.Params
                .Where(p => p.StartsWith("GPU"))
                .Any(pGpu => !gpuUuids.Contains(pGpu));
            return !anyMissingGpuUuidParams;
        }

        private static string CreateCommandWithTemplate(string subscribeLocation, string subscribeUsername, IEnumerable<string> gpuUuids, string templateFilePath, string algorithmName)
        {
            var template = ParseTemplateFileAndCreateCMD(templateFilePath, gpuUuids, subscribeLocation, subscribeUsername, algorithmName);
            if (template == null)
            {
                Logger.Warn("Excavator.CmdConfig", "Template file not found, using default!");
                template = CreateDefaultTemplateAndCreateCMD(subscribeLocation, subscribeUsername, gpuUuids, algorithmName);
            }
            return template;
        }
        private static string GetServiceLocation(string miningLocation)
        {
            if (BuildOptions.BUILD_TAG == BuildTag.TESTNET) return $"nhmp-test.auto.nicehash.com:443";
            if (BuildOptions.BUILD_TAG == BuildTag.TESTNETDEV) return $"nhmp-dev.auto.nicehash.com:443";
            //BuildTag.PRODUCTION
            return $"nhmp.auto.nicehash.com:443";
        }

        public static string CmdJSONString(string pluginUUID, string _miningLocation, string username, string algorithmName, string elps, List<string> IDs, params string[] uuids) {
            _extraLaunchParameters = elps;
            _mappedDeviceIDs = IDs;
            var miningLocation = GetMiningLocation(_miningLocation);
            var templatePath = CommandFileTemplatePath(pluginUUID);
            var miningServiceLocation = GetServiceLocation(miningLocation);
            var command = CreateCommandWithTemplate(miningServiceLocation, username, uuids, templatePath, algorithmName);
            if (command == null) Logger.Error("Excavator.CmdConfig", "command is NULL");
            return command;
        }

        private static string GetMiningLocation(string location)
        {
            // new mining locations new clients
            if (location.StartsWith("eu") || location.StartsWith("usa")) return location;
            // old mining locations old clients with obsolete locations fallback to usa
            return "usa";
        }
    }
}
