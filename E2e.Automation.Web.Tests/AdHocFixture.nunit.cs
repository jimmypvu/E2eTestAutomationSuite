namespace E2e.Automation.Web.Tests
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// ***********************************************************
  public class AdHocFixture
  {
    /// ***********************************************************
    [Test]
    public async Task A_Test()
    {
      var raw = @"AcceptDownloads = clone.AcceptDownloads;
    BaseURL = clone.BaseURL;
    BypassCSP = clone.BypassCSP;
    ClientCertificates = clone.ClientCertificates;
    ColorScheme = clone.ColorScheme;
    DeviceScaleFactor = clone.DeviceScaleFactor;
    ExtraHTTPHeaders = clone.ExtraHTTPHeaders;
    ForcedColors = clone.ForcedColors;
    Geolocation = clone.Geolocation;
    HasTouch = clone.HasTouch;
    HttpCredentials = clone.HttpCredentials;
    IgnoreHTTPSErrors = clone.IgnoreHTTPSErrors;
    IsMobile = clone.IsMobile;
    JavaScriptEnabled = clone.JavaScriptEnabled;
    Locale = clone.Locale;
    Offline = clone.Offline;
    Permissions = clone.Permissions;
    Proxy = clone.Proxy;
    RecordHarContent = clone.RecordHarContent;
    RecordHarMode = clone.RecordHarMode;
    RecordHarOmitContent = clone.RecordHarOmitContent;
    RecordHarPath = clone.RecordHarPath;
    RecordHarUrlFilter = clone.RecordHarUrlFilter;
    RecordHarUrlFilterRegex = clone.RecordHarUrlFilterRegex;
    RecordHarUrlFilterString = clone.RecordHarUrlFilterString;
    RecordVideoDir = clone.RecordVideoDir;
    RecordVideoSize = clone.RecordVideoSize;
    ReducedMotion = clone.ReducedMotion;
    ScreenSize = clone.ScreenSize;
    ServiceWorkers = clone.ServiceWorkers;
    StorageState = clone.StorageState;
    StorageStatePath = clone.StorageStatePath;
    StrictSelectors = clone.StrictSelectors;
    TimezoneId = clone.TimezoneId;
    UserAgent = clone.UserAgent;
    ViewportSize = clone.ViewportSize;";

      var properties = raw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Split('=')[0].Trim())
                    .ToList();

      var result = string.Join(", \n", properties);

      Console.WriteLine(result);
    }
  }
}
