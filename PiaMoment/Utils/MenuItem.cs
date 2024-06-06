using System.Text.Json.Serialization;

namespace ConsoleMenu;

public class MenuItem
{
    [JsonInclude]
    public string Name { get; set; }
    
    [JsonIgnore]
    public string FriendlyName { get; set; }

    public virtual void Action()
    {
        Console.WriteLine("Action not implemented.");
    }
    
    public MenuItem(string name, string friendlyName)
    {
        Name = name;
        FriendlyName = friendlyName;
    }
}