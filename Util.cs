using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
// using System.Threading;
using System.Collections.Generic;

class Util
{
    public static string ConvertBytesToReadableSize(double bytes, string chars = "BKMGT")
    {
        char[] chars_arr = chars.ToCharArray();
        string[] units = chars_arr.Select(c => c.ToString()).ToArray();

        double size = bytes;
        int unitIndex = 0;

        // 循环除以1024，直到值小于1024或达到最大单位
        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        // 格式化输出，保留两位小数
        return $"{size:0.##}{units[unitIndex]}";
    }
}


class SystemMonitor
{
    private PerformanceCounter cpuCounter;
    private PerformanceCounter ramCounter;

    public SystemMonitor()
    {
        // Initialize performance counters
        cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        ramCounter = new PerformanceCounter("Memory", "Available Bytes");
    }

    public float GetCpuUsage()
    {
        // Get CPU usage
        //cpuCounter.NextValue();
        //Thread.Sleep(1000); // Give time interval to get accurate reading
        return cpuCounter.NextValue();
    }

    public float GetAvailableRam()
    {
        // Get available RAM
        return ramCounter.NextValue();
    }

    public List<NetworkStatistics> GetNetworkStatistics()
    {
        // Get network statistics
        var networkStatistics = new List<NetworkStatistics>();
        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (var ni in networkInterfaces)
        {
            if (ni.OperationalStatus == OperationalStatus.Up)
            {
                var stats = ni.GetIPv4Statistics();
                networkStatistics.Add(new NetworkStatistics
                {
                    InterfaceName = ni.Name,
                    BytesSent = stats.BytesSent,
                    BytesReceived = stats.BytesReceived
                });
            }
        }

        return networkStatistics;
    }
}

class NetworkStatistics
{
    public string InterfaceName { get; set; }
    public long BytesSent { get; set; }
    public long BytesReceived { get; set; }
}
