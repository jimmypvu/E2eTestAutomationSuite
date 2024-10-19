using Jvu.TestAutomation.Web.Framework.Testing;


namespace Jvu.TestAutomation.Web.Framework.Extensions
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// <remarks></remarks>
  /// ***********************************************************
  public static partial class TestBaseExtensions
  {
    /// ***********************************************************
    public static string? GetTestSettingIfExists(this ILogger logger, string parameterName)
    {
      if (TestContext.Parameters.Exists(parameterName))
        return TestContext.Parameters.Get(parameterName).ToLower();

      return null;
    }

    /// ***********************************************************
    public static string? GetTestDataFolderPath(this TestBase test)
    {
      return $"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent}\\TestData";
    }

    /// ***********************************************************
    public static string? GetTestDownloadsFolderPath(this TestBase test)
    {
      return $"{Directory.GetCurrentDirectory()}\\downloads";
    }
  }
}
