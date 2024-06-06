using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PiaMoment;

public static class PiaVpn
{
    private static string _piaPath = "C:\\Program Files\\Private Internet Access\\piactl.exe";
    private static string _piaClientPath = "C:\\Program Files\\Private Internet Access\\pia-client.exe";
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
        string? output = ExecuteCtlCommand("connect");
        if (!silent) Console.Log.WriteLine("PIA", "Connecting to VPN...");
        
        while (GetStatus() != "Connected")
        {
            Thread.Sleep(200);
        }
        
        if (!silent) Console.Log.WriteLine("PIA", "Connected to VPN");
    }
    
    public static bool IsClientRunning()
    {
        return Process.GetProcessesByName("pia-client").Length != 0;
    }
    
    public static bool CheckClientRunning()
    {
        if (IsClientRunning()) return true;
        Console.Log.WriteLine("PIA", "&cPIA client is not running, please start it to perform this action"!);
        Console.WaitForEnter("Press enter to continue...");
        return false;
    }
    
    public static string GetRegion()
    {
        string? output = ExecuteCtlCommand("get region");
        return output?.ReplaceLineEndings("") ?? "error";
    }
    
    public static string[] GetRegions()
    {
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
        ExecuteCtlCommand("disconnect");
        if (!silent) Console.Log.WriteLine("PIA", "Disconnecting from VPN...");
        
        while (GetStatus() != "Disconnected")
        {
            Thread.Sleep(200);
        }
        
        if (!silent) Console.Log.WriteLine("PIA", "Disconnected from VPN");
    }
    
    // Possible statuses: Connected, Disconnected, Connecting, Disconnecting
    public static string GetStatus()
    {
        string? output = ExecuteCtlCommand("get connectionstate");
        return output?.ReplaceLineEndings("") ?? "error";
    }

    public static void OpenClient()
    {
        Process.Start(_piaClientPath);
    }

    public static void TerminateClient()
    {
        // Combine the stop-service and start-service commands into one command
        string argumentList = "Stop-Service PrivateInternetAccessService; Stop-Process -Name pia-client -Force";
        Process.Start("powershell", "Start-Process -Verb RunAs -FilePath powershell.exe -ArgumentList '" + argumentList + "'").WaitForExit();
        Console.Log.WriteLine("PIA", "&cTerminated PIA client and service.");
    }   
}