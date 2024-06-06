global using Console = ExtendedConsole.Console;
global using ExtendedConsole;
using System;
using System.Diagnostics;

namespace PiaMoment; // Private internet access moment

public static class Program
{
    public static List<string> ValidAsns = new()
    {
        "Cogent",
        "Cyber Assets Fzco",
    };
    
    public static bool IsAsnValid(string asn)
    {
        return ValidAsns.Any(asn.Contains);
    }

    public static void Main(string[] args)
    {
        Console.Config.SetupConsole();
        PiaMenu.MainMenu();
    }
    
    public static void RerollUntilValidAsn()
    {
        string lastAsn = "";
        reroll:
        while (!IsAsnValid(lastAsn))
        {
            PiaVpn.Disconnect();
            PiaVpn.Connect();
            lol:
            var asnOrg = IpApi.GetCurrentAsnOrg();
            if (asnOrg == null)
            {
                Console.Log.WriteLine("Main", "Failed to get ASN, retrying...", LogLevel.Error);
                Thread.Sleep(500);
                goto lol;
            }

            Console.Log.WriteLine("Main", $"Current ASN: &v{asnOrg}", LogLevel.Debug);
            if (!IsAsnValid(asnOrg))
            {
                Console.Log.WriteLine("Main", "&cInvalid ASN, rerolling...", LogLevel.Warning);
                continue;
            }
            
            lastAsn = asnOrg ?? "";
        }
        
        Console.Log.WriteLine("Main", "&aSuccessfully found a valid ASN!");
    }
}