using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace E2e.Automation.Framework.Extensions
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// <remarks>general extensions</remarks>
  /// ***********************************************************
  public static class GeneralExtensions
  {
    /// ***********************************************************
    public static bool EqualsAnyCase(this string? inputString, string targetString)
    {
      return inputString?.Equals(targetString, StringComparison.OrdinalIgnoreCase) ?? false;
    }

    /// ***********************************************************
    public static bool EqualsAnyCase(this char? input, char targetChar)
    {
      return input.HasValue && char.Equals(targetChar, StringComparison.OrdinalIgnoreCase);
    }

    /// ***********************************************************
    public static bool ContainsAnyCase(this string? inputString, string targetString)
    {
      return inputString?.Contains(targetString, StringComparison.OrdinalIgnoreCase) ?? false;
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
      throw new ArgumentException($"input string '{input}' was not in a valid long int / Int.64 format");
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
      throw new ArgumentException($"input string '{input}' was not in a valid floating point int format");
    }

    /// ***********************************************************
    public static DateTime AsDateTime(this string? input)
    {
      if (DateTime.TryParse(input, out DateTime result))
      {
        return (DateTime)result;
      }
      throw new ArgumentException($"input string '{input}' was not in a valid DateTime format");
    }

    /// ***********************************************************
    public static DateTimeOffset AsDateTimeOffset(this string? input)
    {
      if (DateTimeOffset.TryParse(input, out DateTimeOffset result))
      {
        return (DateTimeOffset)result;
      }
      throw new ArgumentException($"input string '{input}' was not in a valid DateTimeOffset format");
    }

    /// ***********************************************************
    /// <remarks>uses enum Display Name attribute to convert enum
    /// values to their string representation</remarks>
    /// ***********************************************************
    public static string ToString<T>(this T typeValue) where T : Enum
    {
      var fieldInfo = typeValue.GetType().GetField(typeValue.ToString());
      var displayAttribute = fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false).OfType<DisplayAttribute>().FirstOrDefault();

      if (displayAttribute != null)
        return displayAttribute.Name;

      throw new ArgumentException($"enum does not have a valid Display attribute / unsupported enum of type {typeof(T).Name}", nameof(typeValue));
    }

    /// ***********************************************************
    /// <remarks>PlaywrightTest.ToHaveCssClassAsync formats hex in
    /// rgb (r, g, b) format</remarks>
    /// ***********************************************************
    public static string ToRgbString(this string inputHexColorString)
    {
      // trim # char
      inputHexColorString = inputHexColorString.TrimStart('#');

      // convert hex to decimal
      int r = Convert.ToInt32(inputHexColorString.Substring(0, 2), 16);
      int g = Convert.ToInt32(inputHexColorString.Substring(2, 2), 16);
      int b = Convert.ToInt32(inputHexColorString.Substring(4, 2), 16);

      // return RGB string
      return $"rgb({r}, {g}, {b})";
    }

    /// ***********************************************************
    public static string CleanStringForPath(this string? input)
    {
      if (string.IsNullOrEmpty(input))
        return "";

      return Regex.Replace(input, @"[^a-zA-Z0-9_\s-]", string.Empty);
    }

    /// ***********************************************************
    public static string CapitalizeFirstLetter(this string? input)
    {
      if (input.IsNullOrWhiteSpace())
        throw new ArgumentNullException($"{input} string was null or empty!");

      return char.ToUpper(input[0]) + input.Substring(1).ToLower();
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
        log.DebugLine($" -  ensure file exists at \n{filePath}");
        log.DebugLine($" -  {doesFileExist}");
      }

      return doesFileExist;
    }

    /// ***********************************************************
    public static string GetDomainFromUrl(this Uri uri)
    {
      try
      {
        // get the host (e.g., "www.example.com")
        string host = uri.Host;

        // split the host by dots
        var hostParts = host.Split('.');

        // if only 2 or 3 parts (e.g., "example.com" or "www.example.com"), return as is
        if (hostParts.Length == 2 || hostParts.Length == 3)
          return host;

        // for hosts with more parts, we need to determine the domain
        // regex matches common known TLDs
        var tldRegex = new Regex(@"^(com|net|org|edu|gov|mil|int|biz|info|name|pro|aero|coop|museum|[a-z]{2})$", RegexOptions.IgnoreCase);

        // start from the end and find the first non-TLD part
        for (int i = hostParts.Length - 1; i >= 0; i--)
        {
          if (!tldRegex.IsMatch(hostParts[i]))
          {
            return string.Join(".", hostParts.Skip(i));
          }
        }

        // if we couldn't determine, return the full host
        return host;
      }
      catch (UriFormatException)
      {
        throw new Exception("invalid url format, could not extract domain!");
      }
    }
  }
}
