using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvu.TestAutomation.Web.Framework.Extensions
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// <remarks></remarks>
  /// ***********************************************************
  public static class GeneralExtensions
  {
    /// *****************************************
    public static bool EqualsAnyCase(this string inputString, string targetString)
    {
      return inputString.Equals(targetString, StringComparison.OrdinalIgnoreCase);
    }

    /// *****************************************
    public static bool EqualsAnyCase(this string inputString, string targetString)
    {
      return inputString.Equals(targetString, StringComparison.OrdinalIgnoreCase);
    }
  }
}
