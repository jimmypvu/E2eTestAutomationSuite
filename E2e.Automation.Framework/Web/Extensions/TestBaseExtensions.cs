using E2e.Automation.Framework.Web.Testing;

namespace E2e.Automation.Framework.Web.Extensions
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// <remarks></remarks>
  /// ***********************************************************
  public static class TestBaseExtensions
  {
    /// ***********************************************************
    public static string? GetTestSettingIfExists(this ILogger logger, string parameterName)
    {
      if (TestContext.Parameters.Exists(parameterName))
        return TestContext.Parameters.Get(parameterName).ToLower();

      return null;
    }

    /// ***********************************************************
    public static string GetTestFixtureName(this TestBase test)
    {
      var fullClassName = TestContext.CurrentContext.Test.ClassName;
      var classNameParts = fullClassName.Split('.');
      return classNameParts[classNameParts.Length - 1];
    }

    /// ***********************************************************
    public static string? GetTestDataFolderPath(this TestBase test)
    {
      return $"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent}\\Resources\\TestData";
    }

    /// ***********************************************************
    public static string? GetTestDownloadsFolderPath(this TestBase test)
    {
      return $"{Directory.GetCurrentDirectory()}\\downloads";
    }

    /// ***********************************************************
    public static string? GetScreenshotsFolderPath(this TestBase test)
    {
      return $"{Directory.GetCurrentDirectory()}\\screenshots";
    }

    /// ***********************************************************
    public static string? GetScreenshotsTestFolderPath(this TestBase test)
    {
      var fullClassName = TestContext.CurrentContext.Test.ClassName;
      var classNameParts = fullClassName.Split('.');
      var className = classNameParts[classNameParts.Length - 1];

      return $"{Directory.GetCurrentDirectory()}\\screenshots\\{className}\\{test.TestName}";
    }
  }
}
