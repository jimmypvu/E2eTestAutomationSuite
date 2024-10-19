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
    public static bool AsBool(this string? input)
    {
      if (bool.TryParse(input, out bool result))
      {
        return (bool)result;
      }
      throw new ArgumentException($"input string '{input}' was not in a valid boolean format");
    }

    /// ***********************************************************
    public static int AsInt(this string? input)
    {
      if (Int32.TryParse(input, out int result))
      {
        return (int)result;
      }
      throw new ArgumentException($"input string '{input}' was not in a valid Int32 format");
    }

    /// ***********************************************************
    public static long AsLong(this string? input)
    {
      if (Int64.TryParse(input, out long result))
      {
        return (long)result;
      }
      throw new ArgumentException($"input string '{input}' was not in a valid Int.64 format");
    }

    /// ***********************************************************
    public static double AsDouble(this string? input)
    {
      if (double.TryParse(input, out double result))
      {
        return (double)result;
      }
      throw new ArgumentException($"input string '{input}' was not in a valid double format");
    }

    /// ***********************************************************
    public static float AsFloat(this string? input)
    {
      if (float.TryParse(input, out float result))
      {
        return (float)result;
      }
      throw new ArgumentException($"input string '{input}' was not in a valid floating integer format");
    }

    /// ***********************************************************
    /// <remarks>for files within the project directory</remarks>
    /// ***********************************************************
    public static string ToDisplayPath(this string? path)
    {
      return path.Replace(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "");
    }

    /// ***********************************************************
    public static async Task<bool> SoftWaitUntilFileExistsAsync(this string filePath, int timeoutSeconds = 30)
    {
      var log = new ConsoleLogger();
      var timeout = new TimeSpan(0, 0, 0, timeoutSeconds);
      var startDt = DateTime.Now;
      var doesFileExist = File.Exists(filePath);

      while (!File.Exists(filePath))
      {
        await Task.Delay(250);
        doesFileExist = File.Exists(filePath);
        if (DateTime.Now - startDt > timeout)
          break;
      }

      if (TestContext.Parameters["enableDebug"].EqualsAnyCase("true"))
      {
        log.DebugLine($" -  waited {timeout.TotalSeconds} seconds for file to exist at \n{filePath}");
        log.DebugLine($" -  does file exist?\n\t\t\t{doesFileExist}");
      }

      return doesFileExist;
    }
  }
}
