namespace ConsoleMenu;

public static class Extensions
{
    public static string Humanize(this TimeSpan timeSpan)
    {
        string humanized = "";
        if (timeSpan.Days > 0)
        {
            humanized += $"{timeSpan.Days} day{(timeSpan.Days > 1 ? "s" : "")} ";
        }
        if (timeSpan.Hours > 0)
        {
            humanized += $"{timeSpan.Hours} hour{(timeSpan.Hours > 1 ? "s" : "")} ";
        }
        if (timeSpan.Minutes > 0)
        {
            humanized += $"{timeSpan.Minutes} minute{(timeSpan.Minutes > 1 ? "s" : "")} ";
        }
        if (timeSpan.Seconds > 0)
        {
            humanized += $"{timeSpan.Seconds} second{(timeSpan.Seconds > 1 ? "s" : "")} ";
        }
        return humanized.Trim();
    } 
}