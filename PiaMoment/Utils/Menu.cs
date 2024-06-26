using System.Diagnostics;
using System.Text.Json;

namespace ConsoleMenu;

/// <summary>
/// A Console Menu class.
/// </summary>
public class Menu
{
    public string Title { get; set; }
    
    public string ConfigFile { get; set; }
    public List<MenuItem> MenuItems { get; set; }
    
    private bool _exitMenu = false;
    
    public Menu(string title = "Menu", string configFile = "config.json")
    {
        Title = title;
        MenuItems = new List<MenuItem>();
        ConfigFile = configFile;
    }
    
    public void AddMenuItem(MenuItem item)
    {
        MenuItems.Add(item);
    }

    public void AddMenuItem(string name, string friendlyName, Action action)
    {
        MenuItems.Add(new ActionMenuItem(name, friendlyName, action));
    }
    public void AddMenuItem(string name, string friendlyName, bool value)
    {
        MenuItems.Add(new BoolMenuItem(name, friendlyName, value));
    }
    
    public void AddMenuItem(string name, string friendlyName, int value)
    {
        MenuItems.Add(new IntMenuItem(name, friendlyName, value));
    }
    
    public void AddMenuItem(string name, string friendlyName, float value)
    {
        MenuItems.Add(new FloatMenuItem(name, friendlyName, value));
    }
    
    public void AddLabel(string text)
    {
        MenuItems.Add(new LabelMenuItem("", text));
    }
    
    public MenuItem GetMenuItem(int index)
    {
        return MenuItems[index];
    }
    
    public MenuItem? GetMenuItem(string name)
    {
        return MenuItems.Find(item => item.Name == name);
    }

    public void ExitMenu()
    {
        // close the menu
        Console.SwitchToMainBuffer();
        _exitMenu = true;
    }

    public void ShowMenu()
    {
        int selection = 0;
        
        while (MenuItems.Count > selection && MenuItems[selection] is LabelMenuItem)
        {
            selection++;
        }
        
        this.ShowMenuInternal(selection);

        while (true)
        {
            try
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.UpArrow)
                {
                    selection--;
                    
                    if (selection < 0)
                    {
                        selection = this.MenuItems.Count - 1;
                    }
                    
                    while (MenuItems.Count > selection && selection > -1 && MenuItems[selection] is LabelMenuItem)
                    {
                        // i know this is kinda crude and might cause an infinite loop 
                        // but who the balls uses all labelmenuitems
                        selection--;
                    }
                    
                    if (selection < 0)
                    {
                        selection = this.MenuItems.Count - 1;
                    }
                    

                    this.ShowMenuInternal(selection);
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    selection++;
                    if (selection >= this.MenuItems.Count)
                    {
                        selection = 0;
                    }
                    
                    // If the selection is a labelmenuitem, skip it
                    while (MenuItems.Count > selection && MenuItems[selection] is LabelMenuItem)
                    {
                        selection++;
                    }
                    
                    if (selection >= this.MenuItems.Count)
                    {
                        selection = 0;
                    }

                    this.ShowMenuInternal(selection);
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    this.MenuItems[selection].Action();
                    this.ShowMenuInternal(selection);
                }
                else if (key.Key == ConsoleKey.X)
                {
                    if (!IsJsonUpdated())
                    {
                        Console.WriteLine("Save changes? (Y/N) ");
                        ConsoleKeyInfo key2 = Console.ReadKey(true);
                        if (key2.Key == ConsoleKey.Y)
                        {
                            SaveJson(ConfigFile);
                        }
                    }
                    Console.SwitchToMainBuffer();
                    break;
                }
            } catch (Exception e)
            {
                Console.WriteLine("selection: " + selection + " menuitems count: " + MenuItems.Count + " " + e);
            }
            
            if (_exitMenu)
            {
                _exitMenu = false;
                break;
            }
        }
    }

    private bool IsJsonUpdated(string jsonFile = "")
    {
        if (ConfigFile == "") return true; // If there is no config file, it is always updated
        if (jsonFile == "") jsonFile = ConfigFile;
        
        string json = ToJson()!;
        string oldJson = File.Exists(ConfigFile) ? File.ReadAllText(ConfigFile) : "";
        return json == oldJson;
    }

    private void ShowMenuInternal(int selection = 0)
    {
        // Example menu:

        // Clear the console
        Console.SwitchToAlternativeBuffer();
        Console.Write($"{Console.Esc}[2J{Console.Esc}[H{Console.Esc}[3J{Console.Esc}[H");


        // Prevent the default selection from being a labelmenuitem

        // Print the title
        Console.WriteLine(Title);
        string selectionColor = Console.Log.Color9; // Color of the selected item
        string defaultColor = Console.Log.R; // Color of the default text
        string enabledColor = Console.Log.ColorA; // Color of enabled BoolMenuItems
        string disabledColor = Console.Log.ColorC; // Color of disabled BoolMenuItems
        

        // Print the menu items
        int displayedI = 0;
        for (int i = 0; i < MenuItems.Count; i++)
        {
            MenuItem item = MenuItems[i];
            if (item is BoolMenuItem boolItem)
            {
                if (i == selection)
                {
                    Console.WriteLine($"[{displayedI}] {selectionColor}{item.FriendlyName}{defaultColor}: {(boolItem.Value ? enabledColor : disabledColor)}{boolItem.Value}{defaultColor}");
                }
                else
                {
                    Console.WriteLine($"[{displayedI}] {item.FriendlyName}: {(boolItem.Value ? enabledColor : disabledColor)}{boolItem.Value}{defaultColor}");
                }
            }
            else if (item is IntMenuItem intItem)
            {
                if (i == selection)
                {
                    Console.WriteLine($"[{displayedI}] {selectionColor}{item.FriendlyName}{defaultColor}: {intItem.Value}");
                }
                else
                {
                    Console.WriteLine($"[{displayedI}] {item.FriendlyName}: {intItem.Value}");
                }
            }
            else if (item is FloatMenuItem floatItem)
            {
                if (i == selection)
                {
                    Console.WriteLine($"[{displayedI}] {selectionColor}{item.FriendlyName}{defaultColor}: {floatItem.Value}");
                }
                else
                {
                    Console.WriteLine($"[{displayedI}] {item.FriendlyName}: {floatItem.Value}");
                }
            }
            else if (item is ActionMenuItem actionItem)
            {
                if (i == selection)
                {
                    Console.WriteLine($"[{displayedI}] {selectionColor}{item.FriendlyName}{defaultColor}");
                }
                else
                {
                    Console.WriteLine($"[{displayedI}] {item.FriendlyName}");
                }
            }
            else if (item is LabelMenuItem) // LabelMenuItem is a MenuItem that only displays text
            {
                Console.WriteLine(item.FriendlyName);
                displayedI--;
            }
            else
            {
                if (i == selection)
                {
                    Console.WriteLine($"[{displayedI}] {selectionColor}{item.FriendlyName}{defaultColor}");
                }
                else
                {
                    Console.WriteLine($"[{displayedI}] {item.FriendlyName}");
                }
            }
            
            displayedI++;
        }
        
        // Go to the bottom of the console
        int oldTop = Console.CursorTop;
        Console.CursorTop = Console.WindowHeight - 3;
        Console.CursorLeft = 0;
        Console.WriteLine("Use the arrow keys to navigate the menu.");
        Console.WriteLine("Press Enter to execute the selected action.");
        Console.Write("Press X to exit the menu.");
        Console.CursorTop = oldTop;
        Console.CursorLeft = 0;
    }

    public string? ToJson()
    {
        // Construct an object to serialize to JSON
        // that for each menu item, only contains the name and value
        // (the name is used as the key, and the value is used as the value)
        Dictionary<string, object> dict = new Dictionary<string, object>();
        foreach (MenuItem item in MenuItems)
        {
            if (item is BoolMenuItem boolItem)
            {
                dict.Add(item.Name, boolItem.Value);
            }
            else if (item is IntMenuItem intItem)
            {
                dict.Add(item.Name, intItem.Value);
            }
            else if (item is FloatMenuItem floatItem)
            {
                dict.Add(item.Name, floatItem.Value);
            }
        }
        
        // Serialize the object to JSON
        return JsonSerializer.Serialize(dict, new JsonSerializerOptions()
        {
            WriteIndented = true
        });
    }
    
    public void LoadJson(string json = "")
    {
        if (ConfigFile == "")
        {
            return;
        }
        if (json == "") json = ConfigFile;
        if (File.Exists(json)) json = File.ReadAllText(json);
        
        // Deserialize the JSON to a dictionary
        Dictionary<string, object> dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;
        
        // For each menu item, if the dictionary contains the item's name,
        // set the item's value to the value in the dictionary
        foreach (var item in MenuItems.Where(item => dict.ContainsKey(item.Name)))
        {
            switch (item)
            {
                case BoolMenuItem boolItem:
                    boolItem.Value = bool.Parse(dict[item.Name].ToString()!);
                    break;
                case IntMenuItem intItem:
                    intItem.Value = int.Parse(dict[item.Name].ToString()!);
                    break;
                case FloatMenuItem floatItem:
                    floatItem.Value = float.Parse(dict[item.Name].ToString()!);
                    break;
            }
        }
    }

    public void SaveJson(string file = "")
    {
        if (ConfigFile == "")
        {
            return;
        }
        if (file == "") file = ConfigFile;
        if (File.Exists(file)) File.Delete(file);
        File.WriteAllText(file, ToJson()!);
    }
}