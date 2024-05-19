global using Console = ExtendedConsole.Console;
global using ExtendedConsole;
using System;
using System.Diagnostics;

namespace PiaMoment; // Private internet access moment

class Program
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

    public static void Main()
    {
        Console.Config.SetupConsole();

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
        Console.WaitForEnter("Press enter to reroll IP...");
        lastAsn = "";
        goto reroll;
    }
}