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

    public static void Main(string[] args)
    {
        Console.Config.SetupConsole();

        if (args.Length != 0)
        {
            ExecWithArgs(args);
            return;
        }

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

    private static void ExecWithArgs(string[] args)
    {
        switch (args[0])
        {
            case "location-check":
                List<string> regions = PiaVpn.GetRegions().ToList();
                Console.Log.WriteLine("Main", $"&v{regions.Count} &aregions found");
                
                
                // print regions
                Console.Log.WriteLine("Main", "Regions:");
                foreach (var region in regions)
                {
                    Console.WriteLine(region);
                }
                
                List<string> regionsWithAsn = new();
                
                Console.Log.WriteLine("Main", "&bChecking regions for valid ASN...");
                
                int i = 0;
                foreach (var region in regions)
                {
                    i++;
                    Console.Log.Write("Main", $"&aChecking region &v{region} &7(&v{i}&7/&v{regions.Count}&7)        \r");
                    PiaVpn.ExecuteCtlCommand($"set region {region}");
                    PiaVpn.Connect(true);
                    var asn = IpApi.GetCurrentAsnOrg();
                    if (asn == null)
                    {
                        Console.Log.WriteLine("Main", "Failed to get ASN, skipping...                      ", LogLevel.Error);
                        continue;
                    }
                    if (IsAsnValid(asn))
                    {
                        Console.Log.WriteLine("Main", $"&aRegion &v{region}&a has a valid ASN: &v{asn}                           ");
                        regionsWithAsn.Add(region);
                    } else
                    {
                        Console.Log.WriteLine("Main", $"&cRegion &v{region}&c has an invalid ASN: &v{asn}                       ");
                    }
                    
                    // Display the next region if available
                    string nextRegion = regions.ElementAtOrDefault(i);
                    if (nextRegion != null)
                    {
                        Console.Log.Write("Main", $"&aChecking region &v{nextRegion} &7(&v{i+1}&7/&v{regions.Count}&7)        \r");
                    }
                    PiaVpn.Disconnect(true);
                }
                
                // show number of regions with valid ASN
                Console.Log.WriteLine("Main", $"&v{regionsWithAsn.Count} &aregions out of &v{regions.Count} &ahave a valid ASN");
                Console.Log.WriteLine("Main", "Regions with valid ASN:");
                foreach (var region in regionsWithAsn)
                {
                    Console.WriteLine(region);
                }
                break;
            case "status":
                Console.Log.WriteLine("Main", $"VPN Status: &v{PiaVpn.GetStatus()}");
                break;
            case "help": // Fallthrough
            default:
                Console.Log.WriteLine("Main", "Usage: PiaMoment [status/help/location-check]");
                break;
        }
    }
}