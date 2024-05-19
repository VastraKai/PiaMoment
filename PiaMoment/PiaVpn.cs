using System.Diagnostics;

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
    
    public static void Connect()
    {
        // C:\Program Files\Private Internet Access\piactl.exe
        ExecuteCtlCommand("connect");
        
        Console.Log.WriteLine("PIA", "Connecting to VPN...");
        
        while (GetStatus() != "Connected")
        {
            Thread.Sleep(200);
        }
        
        Console.Log.WriteLine("PIA", "Connected to VPN");
    }
    
    public static void Disconnect()
    {
        // C:\Program Files\Private Internet Access\piactl.exe
        ExecuteCtlCommand("disconnect");
        Console.Log.WriteLine("PIA", "Disconnected from VPN");
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