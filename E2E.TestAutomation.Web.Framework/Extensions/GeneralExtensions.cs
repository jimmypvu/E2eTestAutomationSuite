using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Jvu.TestAutomation.Web.Framework.Testing;

namespace Jvu.TestAutomation.Web.Framework.Extensions
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// <remarks>general extensions</remarks>
  /// ***********************************************************
  public static partial class GeneralExtensions
  {
    /// ***********************************************************
    public static bool EqualsAnyCase(this string inputString, string targetString)
    {
      return inputString.Equals(targetString, StringComparison.OrdinalIgnoreCase);
    }

    /// ***********************************************************
    public static bool ContainsAnyCase(this string inputString, string targetString)
    {
      return inputString.Contains(targetString, StringComparison.OrdinalIgnoreCase);
    }

    /// ***********************************************************
    public static bool IsNullOrWhiteSpace(this string inputString)
    {
      return string.IsNullOrWhiteSpace(inputString);
    }

    /// ***********************************************************
    public static bool IsNotNullOrWhiteSpace(this string inputString)
    {
      return !string.IsNullOrWhiteSpace(inputString);
    }

    /// ***********************************************************
    public static bool? AsBool(this string input)
    {
      if (bool.TryParse(input, out bool result))
      {
        return (bool)result;
      }
      return null;
    }

    /// ***********************************************************
    public static int? AsInt(this string input)
    {
      if (Int32.TryParse(input, out int result))
      {
        return (int)result;
      }
      return null;
    }

    /// ***********************************************************
    public static long? AsLong(this string input)
    {
      if (Int64.TryParse(input, out long result))
      {
        return (long)result;
      }
      return null;
    }

    /// ***********************************************************
    public static double? AsDouble(this string input)
    {
      if (double.TryParse(input, out double result))
      {
        return (double)result;
      }
      return null;
    }

    /// ***********************************************************
    public static float? AsFloat(this string input)
    {
      if (float.TryParse(input, out float result))
      {
        return (float)result;
      }
      return null;
    }
  }
}
