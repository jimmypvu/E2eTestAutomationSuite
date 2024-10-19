namespace Jvu.TestAutomation.Web.Framework.Utils.Images
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// ***********************************************************
  public class BitmapCompareOptions
  {
    public float SensitivityThreshold { get; set; } = 0.1f; // default pixelmatch sensitivity threshold level, ranges from 0 to 1 with lower values being more sensitive to differences, use 0 for exact matching
    public bool IgnoreAntiAliasedPixels { get; set; } = true;  // pixelmatch AA mode, true excludes anti-aliased pixels from image comparison
  }
}