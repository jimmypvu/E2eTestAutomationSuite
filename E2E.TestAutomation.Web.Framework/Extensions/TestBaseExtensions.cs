using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jvu.TestAutomation.Web.Framework.Pages;
using Jvu.TestAutomation.Web.Framework.Logging;
using Jvu.TestAutomation.Web.Framework.Testing;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.Runtime.CompilerServices;


namespace Jvu.TestAutomation.Web.Framework.Extensions
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
      logger.WriteLine($"doesParamExist {parameterName}: {TestContext.Parameters.Exists(parameterName)}");
      if (TestContext.Parameters.Exists(parameterName))
        return TestContext.Parameters.Get(parameterName);

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
