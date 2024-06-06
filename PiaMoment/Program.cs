global using Console = ExtendedConsole.Console;
global using ExtendedConsole;
using System;
using System.Diagnostics;
using System.Numerics;
using ConsoleMenu;

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
        // increase the window size y by 30
        string currentTitle = Console.Title;
        var windowWidth = Console.WindowWidth;
        var windowHeight = Console.WindowHeight;
        Console.SetWindowSize(120, 40);
        Console.Title = "PiaMoment";
        PiaMenu.MainMenu();
        Console.Title = currentTitle;
        Console.SetWindowSize(windowWidth, windowHeight);
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
    
    public static int i = 0;
    public static bool isRunning = false;
    public static List<TimeSpan> timesPerCycle = new();

    public static string[] GetValidRegions()
    {
        i = 0;
        timesPerCycle.Clear();
        isRunning = false;
        
        List<string> regions = PiaVpn.GetRegions().ToList();
        Console.Log.WriteLine("Main", $"&v{regions.Count} &aregions found");
        
        Console.Log.WriteLine("Main", "Measuring time per cycle...");
        PiaVpn.Connect(true);
        var dateThen = DateTime.Now;
        PiaVpn.Disconnect(true);
        PiaVpn.Connect(true);
        var dateNow = DateTime.Now;
        PiaVpn.Disconnect(true);
            
        var timePerCycle = dateNow - dateThen;
        
        var totalEstimate = timePerCycle * regions.Count;
        // format: 1 hour, 2 minutes, 3 seconds
        Console.Log.WriteLine("Main", $"Estimated time: &v{totalEstimate.Humanize()}");
        if(!Console.Log.Confirm("Main", "Do you want to continue?")) return [];
        
        List<string> regionsWithAsn = new();

        Console.Log.WriteLine("Main", "&bChecking regions for valid ASN...");
        isRunning = true;
        new Thread(() =>
        {
            var startTime = DateTime.Now;
            while (isRunning)
            {
                var timeElapsed = DateTime.Now - startTime;
                var averageTimePerCycle = timesPerCycle.Count > 0 ? new TimeSpan((long)timesPerCycle.Average(x => x.Ticks)) : new TimeSpan(0);
                var estimatedRemaining = averageTimePerCycle * (regions.Count - i);
                Console.Title = "PiaMoment - " + timeElapsed.Humanize() + " elapsed, " + estimatedRemaining.Humanize() + " remaining";
                Thread.Sleep(10);
            }
            Console.Title = "";
        }).Start();
        foreach (var region in regions)
        {
            i++;
            var cycleStart = DateTime.Now;
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
                Console.Log.WriteLine("Main",
                    $"&aRegion &v{region}&a has a valid ASN: &v{asn}                           ");
                regionsWithAsn.Add(region);
            }
            else
            {
                Console.Log.WriteLine("Main",
                    $"&cRegion &v{region}&c has an invalid ASN: &v{asn}                       ");
            }
            // Display the next region if available
            string nextRegion = regions.ElementAtOrDefault(i);
            if (nextRegion != null)
            {
                Console.Log.Write("Main", $"&aChecking region &v{nextRegion} &7(&v{i+1}&7/&v{regions.Count}&7)        \r");
            }
            PiaVpn.Disconnect(true);
            var cycleEnd = DateTime.Now;
            var cycleTime = cycleEnd - cycleStart;
            timesPerCycle.Add(cycleTime);
        }
        isRunning = false;

        // show number of regions with valid ASN
        Console.Log.WriteLine("Main", $"&aFound &v{regionsWithAsn.Count} &aregions with valid ASN!");
        return regionsWithAsn.ToArray();
    }
}