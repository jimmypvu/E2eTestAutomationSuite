namespace Jvu.TestAutomation.Web.Framework.Models
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// ***********************************************************
  public static class MobileTestData
  {
    /// ***********************************************************
    public static async Task<IEnumerable<string>> GetDeviceNamesAsync()
    {
      using var playwright = await Playwright.CreateAsync();
      var devices = new List<string>();
      foreach (var deviceName in playwright.Devices.Keys)
      {
        devices.Add(deviceName);
      }
      return devices;
    }
  }
}
