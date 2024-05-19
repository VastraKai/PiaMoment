using System.Net;
using System.Text.Json;

namespace PiaMoment;

public static class IpApi
{
  // https://api.ipapi.is/
  public static string BaseUrl = "https://api.ipapi.is/";

  public static HttpClient Client = new HttpClient();

  public static string? GetCurrentAsnOrg()
  {
    try
    {
      // Sending a request to the API without any parameters will return information about the IP address of the client.
      string response = Client.GetStringAsync(BaseUrl).Result;
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