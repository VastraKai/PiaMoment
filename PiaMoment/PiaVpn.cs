using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PiaMoment;

public static class PiaVpn
{
    private static string _piaPath = "C:\\Program Files\\Private Internet Access\\piactl.exe";
    static PiaVpn()
    {
        // C:\Program Files\Private Internet Access\piactl.exe
        if (!File.Exists("C:\\Program Files\\Private Internet Access\\piactl.exe"))
        {
            Console.Log.WriteLine("PIA", "piactl.exe not found, please install PIA");
            Console.WaitForEnter("Press enter to exit...");
            Environment.Exit(1);
        }
    }
    
    public static string? ExecuteCtlCommand(string command)
    {
        // C:\Program Files\Private Internet Access\piactl.exe
        var psi = new ProcessStartInfo(_piaPath, command);
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;
        psi.RedirectStandardOutput = true;
        Process? proc = Process.Start(psi);
        string? output = proc?.StandardOutput.ReadToEnd();
        proc?.WaitForExit();
        return output;
    }
    
    public static void Connect(bool silent = false)
    {
        // C:\Program Files\Private Internet Access\piactl.exe
        ExecuteCtlCommand("connect");
        
        if (!silent) Console.Log.WriteLine("PIA", "Connecting to VPN...");
        
        while (GetStatus() != "Connected")
        {
            Thread.Sleep(200);
        }
        
        if (!silent) Console.Log.WriteLine("PIA", "Connected to VPN");
    }
    
    public static string[] GetRegions()
    {
        // C:\Program Files\Private Internet Access\piactl.exe
        string? output = ExecuteCtlCommand("get regions");
        string[]? regions = output?.Split("\n");
        // Remove the auto region
        regions = regions?.Where(region => !region.Contains("Auto")).ToArray();
        // remove any non alphanumeric characters excluding - and \n
        regions = regions?.Select(region => Regex.Replace(region, "[^a-zA-Z0-9-\\n]", "")).ToArray();
        // Remove any empty strings
        regions = regions?.Where(region => region != "").ToArray();
        return regions ?? [];
    }
    
    public static void Disconnect(bool silent = false)
    {
        // C:\Program Files\Private Internet Access\piactl.exe
        ExecuteCtlCommand("disconnect");
        if (!silent) Console.Log.WriteLine("PIA", "Disconnecting from VPN...");
        
        while (GetStatus() != "Disconnected")
        {
            Thread.Sleep(200);
        }
        
        if (!silent) Console.Log.WriteLine("PIA", "Disconnected from VPN");
    }
    
    public static string GetStatus()
    {
        // C:\Program Files\Private Internet Access\piactl.exe
        /*var psi = new ProcessStartInfo(_piaPath, "get connectionstate");
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;
        psi.RedirectStandardOutput = true;
        Process? proc = Process.Start(psi);
        string? output = proc?.StandardOutput.ReadToEnd();
        proc?.WaitForExit();*/
        string? output = ExecuteCtlCommand("get connectionstate");
        return output?.ReplaceLineEndings("") ?? "error";
    }
}