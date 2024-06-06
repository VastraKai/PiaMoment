using System.Diagnostics;
using ConsoleMenu;

namespace PiaMoment;

public static class PiaMenu
{
    public static void MainMenu()
    {
        var menu = new Menu($"{Console.Log.ColorB}[Main Menu]{Console.Log.R}", "");
        menu.AddLabel("Select an option...");
        menu.AddMenuItem("showCurrentAsn", "Show information", () =>
        {
            if (!PiaVpn.CheckClientRunning()) return;
            Console.Log.WriteLine("Main", $"Current ASN: &v{IpApi.GetCurrentAsnOrg()}");
            Console.Log.WriteLine("Main", $"VPN Status: &v{PiaVpn.GetStatus()}");
            Console.Log.WriteLine("Main", $"VPN Region: &v{PiaVpn.GetRegion()}");

            Console.WaitForEnter("Press enter to continue...");
        });
        menu.AddMenuItem("rerollIp", "Reroll IP", () =>
        {
            if (!PiaVpn.CheckClientRunning()) return;
            Program.RerollUntilValidAsn();
            Console.WaitForEnter("Press enter to continue...");
        });
        menu.AddMenuItem("connectVpn", "Connect to VPN", () =>
        {
            if (!PiaVpn.CheckClientRunning()) return;
            PiaVpn.Connect();
            Console.WaitForEnter("Press enter to continue...");
        });
        menu.AddMenuItem("disconnectVpn", "Disconnect from VPN", () =>
        {
            if (!PiaVpn.CheckClientRunning()) return;
            PiaVpn.Disconnect();
            Console.WaitForEnter("Press enter to continue...");
        });
        menu.AddMenuItem("setRegion", "Set VPN Region...", RegionMenu);
        menu.AddMenuItem("troubleshootingOptions", "Troubleshooting Options...", TroubleshootingMenu);
        menu.AddMenuItem("exit", "Exit", () => menu.ExitMenu());
        
        menu.ShowMenu();
    }
    
    public static List<string> bypassingRegions = new List<string>
    {
        "us-east",
        "us-washington-dc",
        "us-new-york",
        "us-chicago",
        "us-atlanta",
        "us-denver",
        "us-texas",
        "us-florida",
        "us-seattle",
        "us-west-streaming-optimized",
        "us-silicon-valley",
        "us-california",
        "us-las-vegas",
        "ca-toronto",
        "ca-vancouver",
        "uk-london",
        "uk-manchester",
        "france",
        "belgium",
        "de-germany-streaming-optimized",
        "de-berlin"
    };

    public static void RegionMenu()
    {
        var menu = new Menu($"{Console.Log.ColorB}[Select a Region]{Console.Log.R}", "");
        
        menu.AddLabel("Select a region to use...");
        menu.AddLabel("Most of the regions here should work with Minecraft servers.");
        
        foreach (var region in bypassingRegions)
        {
            menu.AddMenuItem(region, region, new Action(() =>
            {
                if (!PiaVpn.CheckClientRunning()) return;
                if (PiaVpn.GetStatus() == "Connected")
                {
                    Console.Log.WriteLine("Main", "This action will disconnect you from VPN!");
                    if (!Console.Log.Confirm("Main", "Are you sure you want to continue?")) return;
                }

                PiaVpn.Disconnect(true);
                PiaVpn.ExecuteCtlCommand($"set region {region}");
                Console.Log.WriteLine("Main", $"&aRegion has been set to &v{region}&a.");
                Console.WaitForEnter("Press enter to continue...");
                menu.ExitMenu();
            }));
        }
        menu.AddMenuItem("goBack", "Go back...", menu.ExitMenu);
        
        menu.ShowMenu();
    }
    
    public static void TroubleshootingMenu()
    {
        var menu = new Menu($"{Console.Log.ColorB}[Troubleshooting Options]{Console.Log.R}", "");
        menu.AddLabel("Select an option...");
        menu.AddMenuItem("checkClientRunning", "Check if PIA client is running", new Action(() =>
        {
            Console.Log.WriteLine("Main", $"&bThe PIA client is currently {(PiaVpn.IsClientRunning() ? "&arunning" : "&cnot running")}&b.");
            Console.WaitForEnter("Press enter to continue...");
        }));
        menu.AddMenuItem("openClient", "Open PIA client", new Action(() =>
        {
            PiaVpn.OpenClient();
            Console.WaitForEnter("Press enter to continue...");
        }));
        menu.AddMenuItem("restartService", "Restart PIA service", new Action(() =>
        {
            Process.Start("powershell", "Start-Process -Verb RunAs -FilePath powershell.exe -ArgumentList 'Restart-Service PrivateInternetAccessService'").WaitForExit();
            Console.WaitForEnter("Press enter to continue...");
        }));
        menu.AddMenuItem("terminatePia", "Terminate PIA processes", new Action(() =>
        {
            PiaVpn.TerminateClient();
            Console.WaitForEnter("Press enter to continue...");
        }));
        menu.AddMenuItem("goBack", "Go back...", menu.ExitMenu);
        
        menu.ShowMenu();
    }
}