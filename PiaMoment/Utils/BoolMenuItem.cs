namespace ConsoleMenu;

public class BoolMenuItem : MenuItem
{
    public bool Value { get; set; }
    
    public override void Action()
    {
        Value = !Value;
    }
    
    public BoolMenuItem(string name, string friendlyName, bool value) : base(name, friendlyName)
    {
        Value = value;
    }
}