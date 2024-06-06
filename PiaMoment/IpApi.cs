using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace PiaMoment;

public static class IpApi
{
  // https://api.ipapi.is/
  public static string BaseUrl = "https://api.ipapi.is/";

  public static string? GetCurrentAsnOrg()
  {
    try
    {
      string response = new HttpClient().GetStringAsync(BaseUrl).Result; // dis just tweaks out if i dont use a new client here i have no fucking idea why
      var json = JsonSerializer.Deserialize<JsonElement>(response);
      // Check if the json actually has an ASN property
      if (!json.TryGetProperty("asn", out var _))
      {
        return "no asn";
      }

      return json.GetProperty("asn").GetProperty("org").GetString();
    }
    catch (Exception)
    {
      return null;
    }
  }
}