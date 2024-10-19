using System.Text;
using System.Text.RegularExpressions;
using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;

namespace E2e.Automation.Framework.Extensions
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// ***********************************************************
  public static partial class PageExtensions
  {
    /// ***********************************************************
    public static async Task TakeScreenshotAsync(this IPage page, string screenshotTitle, bool shouldWaitForNetworkIdle = false)
    {
      await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

      if (!shouldWaitForNetworkIdle)
        await Task.Delay(33); // small artificial wait for browser to finish rendering dom content, loaded != rendered
      else
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);  // not recommended but if necessary can wait for all network activity to stop before continuing test

      var timestamp = $"{DateTime.Now:hhmmssttMMddyy}";
      var fullClassName = TestContext.CurrentContext.Test.ClassName;
      var classNameParts = fullClassName.Split('.');
      var className = classNameParts[classNameParts.Length - 1];
      await page.ScreenshotAsync(new()
      {
        Path = $"{Directory.GetCurrentDirectory()}\\screenshots\\{className}\\{TestContext.CurrentContext.Test.MethodName}\\{timestamp}_{screenshotTitle}_{page.ViewportSize.Width}x{page.ViewportSize.Height}.png"
      });
    }

    /// ***********************************************************
    /// <remarks>crawls a site and runs passed lambda function logic on each page</remarks>
    /// ***********************************************************
    public static async Task CrawlWebAndRunCustomLogicOnPagesAsync(this IPage page, Func<IPage, Task> logicBlock, string baseUrl = "", int crawlTimeoutSeconds = 3600)
    {
      var log = new ConsoleLogger();

      // keep track of all unique urls visited and all urls left to visit
      // as we crawl through site, scrape links off the current page and add to the urlsToVisit queue, visit that link and add to the set if it's not already in the visitedUrls set
      var visitedUrls = new HashSet<string>();
      var urlsToVisit = new Queue<string>();

      var initialUrl = page.Url;
      urlsToVisit.Enqueue(initialUrl);

      log.WriteLine($"crawling site starting from {initialUrl} ...");
      var startDt = DateTime.Now;

      while (urlsToVisit.Count > 0)
      {
        var currentUrl = urlsToVisit.Dequeue();

        // if we already visited that link, skip this loop and continue to the next url
        if (visitedUrls.Contains(currentUrl))
          continue;

        try
        {
          // go to current url
          log.WriteLine($"navigating to: {currentUrl}");
          await page.GotoAsync(currentUrl);

          await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

          // execute logic on page
          log.WriteLine($"executing delegate logic on page...");
          await logicBlock.Invoke(page); // pass the current page to the logic block, execute whatever logic you need to on page, ie. page.RunAxeScanAsync page.ScreenshotAsync, etc

          // add to visitedUrls after navigating to
          visitedUrls.Add(currentUrl);

          // scrape all links on current page
          var links = await page.EvaluateAsync<string[]>(@"() => {
return Array.from(document.querySelectorAll('a')).map(a => a.href)
}");

          // add unique links to urlsToVisit
          foreach (var link in links)
          {
            // only add urls from the same domain if a baseUrl is specified
            if (baseUrl.IsNotNullOrWhiteSpace())
            {
              if (!visitedUrls.Contains(link) && !urlsToVisit.Contains(link) && link.ContainsAnyCase(baseUrl))
              {
                urlsToVisit.Enqueue(link);
              }
            }
            else
            {
              if (!visitedUrls.Contains(link) && !urlsToVisit.Contains(link))
              {
                // otherwise add all unique scraped links regardless of domain
                urlsToVisit.Enqueue(link);
              }
            }
          }

          log.DebugLine($"found {links.Length} link on page");
          log.DebugLine($" - queue size: {urlsToVisit.Count}");

          // small delay to keep from server overload
          await Task.Delay(42);
        }
        catch (Exception ex)
        {
          log.WriteLine(ex.Message);
        }

        // stop crawling when timeout hits; if timeout = 0 crawl indefinitely or until all links are visited
        if (crawlTimeoutSeconds != 0)
        {
          if (DateTime.Now - startDt > new TimeSpan(0, 0, 0, crawlTimeoutSeconds))
            break;
        }
      }
    }

    /// ***********************************************************
    /// <remarks>defaults to WCAG 2.2 AA level rules & serious & critical violations</remarks>
    /// ***********************************************************
    public static async Task RunAccessibilityScanAsync(this IPage page, string impactLevel = "serious", string commaSeparatedRuleTags = "wcag22aa,wcag21aa,wcag21a,wcag2aa,wcag2a")
    {
      var log = new TestContextLogger();

      // ensure report directory for test exists
      var axeScanResultsFolderPath = $"{Directory.GetCurrentDirectory()}\\AxeScanResults\\{TestContext.CurrentContext.Test.MethodName}\\{TestContext.Parameters["browser"]}\\pages";

      if (!new DirectoryInfo(axeScanResultsFolderPath).Exists)
      {
        var axeFolder = new DirectoryInfo(axeScanResultsFolderPath);
        axeFolder.Create();
      }

      // add rules and set axe scan options, run page.GetRules to see all available rule tags (default value "wcag22aa,wcag21aa,wcag21a,wcag2aa,wcag2a" for standard compliance)
      var ruleTags = new List<string>();
      foreach (var rule in commaSeparatedRuleTags.Split(","))
      {
        ruleTags.Add(rule.Trim());
      }

      var axeRunOptions = new AxeRunOptions()
      {
        RunOnly = new RunOnlyOptions { Type = "tag", Values = ruleTags },
        ResultTypes = new HashSet<ResultType>() { ResultType.Violations }, // limit result types to Violations
        Selectors = true,
        XPath = true,
        Ancestry = true
      };

      // run axe scan
      log.WriteLine("running accessibility scan on page...");
      log.WriteLine($" - {page.Url}...");
      var results = await page.RunAxe(axeRunOptions);
      var failResults = results.Violations;

      // log violations, published separately as the accessibility scan results contain many lines of text
      var sb = new StringBuilder();
      sb.AppendLine(page.Url);
      sb.AppendLine("");

      bool hasViolation = false;
      foreach (var violation in failResults)
      {
        switch (impactLevel)
        {
          case "moderate": // log moderate violations and up
            if (violation.Impact.EqualsAnyCase("moderate") || violation.Impact.EqualsAnyCase("serious") || violation.Impact.EqualsAnyCase("critical"))
            {
              sb.Append(violation);
              sb.Append(",\n");
            }
            break;
          case "serious": // log serious violations and up
            if (violation.Impact.EqualsAnyCase("serious") || violation.Impact.EqualsAnyCase("critical"))
            {
              sb.Append(violation);
              sb.Append(",\n");
              hasViolation = true;
            }
            break;
          case "critical": // log critical violations only
            if (violation.Impact.EqualsAnyCase("critical"))
            {
              sb.Append(violation);
              sb.Append(",\n");
              hasViolation = true;
            }
            break;
          default: // log all violations if Minor is specified or unknown
            sb.Append(violation);
            sb.Append(",\n");
            hasViolation = true;
            break;
        }
      }

      if (hasViolation)
      {
        var cleanedUrl = Regex.Replace(page.Url, @"[^a-zA-Z0-9]", "_").Replace("http__", "");
        var trimmedCleanedUrl = cleanedUrl.Substring(1, cleanedUrl.Length - 1);
        var filePath = $"{axeScanResultsFolderPath}\\{trimmedCleanedUrl}.txt";

        // truncate and clean file path if long URL causes path to go over the windows limit (260 char max)
        if (filePath.Length > 260)
        {
          // split full path into path and extension parts
          var lastDotIndex = filePath.LastIndexOf('.');
          var longPath = filePath.Substring(0, lastDotIndex);

          // truncate long path part down to 252 chars, replace last 3 chars with random suffix to avoid case of similar but diff longUrl pages from being visited but only getting 1 report generated (due to having the same truncated filepath)
          longPath = longPath.Substring(0, 252);
          var random = new Random();
          var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
          var randomSuffix = new string(Enumerable.Repeat(chars, 3).Select(s => s[random.Next(s.Length)]).ToArray());
          filePath = longPath + "~" + randomSuffix + ".txt";
        }

        string reportString = sb.ToString();
        var trimmedReportString = reportString.Trim();
        trimmedReportString = trimmedReportString.Substring(0, trimmedReportString.Length - 1);

        await File.WriteAllTextAsync(filePath, trimmedReportString);
      }
    }

    /// <remarks>defaults to WCAG 2.2 AA level rules & serious & critical violations</remarks>
    public static async Task RunAccessibilityScanAsync(this ILocator locator, string partIdPrefix, string impactLevel = "serious", string commaSeparatedRuleTags = "wcag22aa,wcag21aa,wcag21a,wcag2aa,wcag2a")
    {
      var log = new TestContextLogger();

      // ensure report directory for test exists
      var axeScanResultsFolderPath = $"{Directory.GetCurrentDirectory()}\\AxeScanResults\\{TestContext.CurrentContext.Test.MethodName}\\{TestContext.Parameters["browser"]}\\parts";

      if (!new DirectoryInfo(axeScanResultsFolderPath).Exists)
      {
        var axeFolder = new DirectoryInfo(axeScanResultsFolderPath);
        axeFolder.Create();
      }

      // add rules and set axe scan options, run page.GetRules to see all available rule tags (default value "wcag22aa,wcag21aa,wcag21a,wcag2aa,wcag2a" for standard compliance)
      var ruleTags = new List<string>();
      foreach (var rule in commaSeparatedRuleTags.Split(","))
      {
        ruleTags.Add(rule.Trim());
      }

      var axeRunOptions = new AxeRunOptions()
      {
        RunOnly = new RunOnlyOptions { Type = "tag", Values = ruleTags },
        ResultTypes = new HashSet<ResultType>() { ResultType.Violations }, // limit result types to Violations
        Selectors = true,
        XPath = true,
        Ancestry = true
      };

      // run axe scan
      log.WriteLine("running accessibility scan on element...");
      log.WriteLine($" - {locator.Page.Url}...");
      log.WriteLine($"");
      log.WriteLine($"{locator}");
      var results = await locator.RunAxe(axeRunOptions);
      var failResults = results.Violations;

      // log violations, published separately as the accessibility scan results contain many lines of text
      var sb = new StringBuilder();
      sb.AppendLine(locator.Page.Url);
      sb.Append(locator.ToString());
      sb.AppendLine("");

      bool hasViolation = false;
      foreach (var violation in failResults)
      {
        switch (impactLevel)
        {
          case "moderate": // log moderate violations and up
            if (violation.Impact.EqualsAnyCase("moderate") || violation.Impact.EqualsAnyCase("serious") || violation.Impact.EqualsAnyCase("critical"))
            {
              sb.Append(violation);
              sb.Append(",\n");
            }
            break;
          case "serious": // log serious violations and up
            if (violation.Impact.EqualsAnyCase("serious") || violation.Impact.EqualsAnyCase("critical"))
            {
              sb.Append(violation);
              sb.Append(",\n");
              hasViolation = true;
            }
            break;
          case "critical": // log critical violations only
            if (violation.Impact.EqualsAnyCase("critical"))
            {
              sb.Append(violation);
              sb.Append(",\n");
              hasViolation = true;
            }
            break;
          default: // log all violations if Minor is specified or unknown
            sb.Append(violation);
            sb.Append(",\n");
            hasViolation = true;
            break;
        }
      }

      if (hasViolation)
      {
        var cleanedUrl = Regex.Replace(locator.Page.Url, @"[^a-zA-Z0-9]", "_").Replace("http__", "");
        var trimmedCleanedUrl = cleanedUrl.Substring(1, cleanedUrl.Length - 1);
        var filePath = $"{axeScanResultsFolderPath}\\{partIdPrefix}_{trimmedCleanedUrl}.txt";

        // truncate and clean file path if long URL causes path to go over the windows limit (260 char max)
        if (filePath.Length > 260)
        {
          // split full path into path and extension parts
          var lastDotIndex = filePath.LastIndexOf('.');
          var longPath = filePath.Substring(0, lastDotIndex);

          // truncate long path part down to 252 chars, replace last 3 chars with random suffix to avoid case of similar but diff longUrl pages from being visited but only getting 1 report generated (due to having the same truncated filepath)
          longPath = longPath.Substring(0, 252);
          var random = new Random();
          var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
          var randomSuffix = new string(Enumerable.Repeat(chars, 3).Select(s => s[random.Next(s.Length)]).ToArray());
          filePath = longPath + "~" + randomSuffix + ".txt";
        }

        string reportString = sb.ToString();
        var trimmedReportString = reportString.Trim();
        trimmedReportString = trimmedReportString.Substring(0, trimmedReportString.Length - 1);

        await File.WriteAllTextAsync(filePath, trimmedReportString);
      }
    }
  }
}
