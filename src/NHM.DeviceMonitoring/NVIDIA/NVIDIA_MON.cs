﻿using System.Runtime.InteropServices;

namespace NHM.DeviceMonitoring.NVIDIA
{
    internal static class NVIDIA_MON
    {
        const string dll = "device_monitoring_nvidia.dll";
        public delegate void log_cb(string error);

        [DllImport(dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_reg_log_cb(log_cb cb);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int nhm_nvidia_set_debug_log_level(int level);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_init();
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_deinit();
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_restart_driver();
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern bool nhm_nvidia_is_nvapi_alive();
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern bool nhm_nvidia_is_nvml_alive();
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_load_percentage(int bus_number, ref int get_load_percentage);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_power_usage(int bus_number, ref int get_power_usage);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_temperature(int bus_number, ref ulong get_temperature);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_fan_speed_rpm(int bus_number, ref int get_fan_speed_rpm);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_fan_speed_rpm_v2(int bus_number, ref int get_fan_speed_rpm);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_fan_speed_percentage(int bus_number, ref int get_fan_speed_percentage);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_set_fan_speed_percentage(int bus_number, int set_fan_speed_percentage);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_tdp_min_max_default(int bus_number, ref uint min, ref uint max, ref uint defaultV);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_tdp(int bus_number, ref int get_tdp);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_set_tdp(int bus_number, int set_tdp);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_core_clocks(int bus_number, ref int core_clocks);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_set_core_clocks_absolute(int bus_number, int core_clocks);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_set_core_clocks_delta(int bus_number, int core_clocks);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_memory_clocks(int bus_number, ref int memory_clocks);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_set_memory_clocks_absolute(int bus_number, int memory_clocks);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_set_memory_clocks_delta(int bus_number, int memory_clocks);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_memory_info(int bus_number, ref ulong free, ref ulong total, ref ulong used);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_restore_fan_speed(int bus_number);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_set_memory_timings(int bus_number, string memory_timings);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_reset_memory_timings(int bus_number);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_special_temperatures(int bus_number, ref int hotspot_temp, ref int vram_temp);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_memory_controller_load(int bus_number, ref int mem_ctrl_load);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_print_memory_timings(int bus_number);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_core_clocks_min_max_default(int bus_number, ref int min, ref int max, ref int def);
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_nvidia_device_get_memory_clocks_min_max_default(int bus_number, ref int min, ref int max, ref int def);


    }
}
