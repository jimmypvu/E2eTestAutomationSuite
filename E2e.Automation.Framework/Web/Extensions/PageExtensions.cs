using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using E2e.Automation.Framework.Utils.Html;
using E2e.Automation.Framework.Utils.Images;

namespace E2e.Automation.Framework.Web.Extensions
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// ***********************************************************
  public static class PageExtensions
  {
    /// ***********************************************************
    public static async Task<string> TakeScreenshotAsync(this IPage page, string screenshotTitle, bool shouldWaitForNetworkIdle = false)
    {
      await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

      if (!shouldWaitForNetworkIdle)
        await Task.Delay(42); // small artificial wait for browser to finish rendering dom content, loaded != rendered
      else
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);  // not recommended but if necessary can wait for all network activity to stop before continuing test

      var timestamp = $"{DateTime.Now:hh-mm-ss-tt_MM-dd-yy}";
      var fullClassName = TestContext.CurrentContext.Test.ClassName;
      var classNameParts = fullClassName.Split('.');
      var className = classNameParts[classNameParts.Length - 1];
      var screenshotPath = $"{Directory.GetCurrentDirectory()}\\screenshots\\{className}\\{TestContext.CurrentContext.Test.MethodName}\\{timestamp}_{screenshotTitle}_{page.ViewportSize.Width}x{page.ViewportSize.Height}.png";
      await page.ScreenshotAsync(new()
      {
        Path = screenshotPath
      });

      return screenshotPath;
    }

    /// ***********************************************************
    /// <remarks>Naming convention: baseline image file names should end with "_Baseline.png"
    /// BitmapCompareOptions: options for bitmap comparison, can set sensitivity threshold from 0 to 1 
    /// (lower values more sensitive to pixel differences) and anti-aliasing mode (true will exclude anti-aliased pixels from 
    /// image comparison). Uses default pixelmatch compare options if not provided
    /// IgnoreRegions: image mask regions to exclude from comparison, defined by X,Y origin point and Width/Height/Radius values
    /// Enable Metadata tagging to capture element HTML and record as image metadata for comparison </remarks>
    /// ***********************************************************
    public static async Task ToHaveExpectedScreenshotAsync(this IPage page, string baselineImageRelativePath, List<IgnoreRegion> ignoreRegions = null, BitmapCompareOptions? options = null, bool enableMetaDataTagging = false)
    {
      // ensure page is fully loaded, all images and dom content are loaded and rendered
      await page.WaitForDomContentToLoadAndRenderAsync(100);

      var log = new TestContextLogger();
      var dimensionsPrefix = $"{TestContext.Parameters["viewportWidth"]}x{TestContext.Parameters["viewportHeight"]}_";

      var testFixtureParts = TestContext.CurrentContext.Test.ClassName.Split('.');
      var testFixture = testFixtureParts[testFixtureParts.Length - 1];
      var testName = TestContext.CurrentContext.Test.MethodName;

      // check if baseline image already exists, if it does take a screenshot of the page and compare it to the baseline, otherwise save screenshot as the new baseline
      var baselineImagePath = $"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent}\\Resources\\Baselines\\{testFixture}\\{testName}\\{TestContext.Parameters["browser"]}\\pages\\{dimensionsPrefix}{baselineImageRelativePath}";

      if (ignoreRegions != null)
        baselineImagePath = baselineImagePath.Replace("_Baseline", "_Unmasked_Baseline");

      var wasBaseLineAlreadyPresent = File.Exists(baselineImagePath);

      // determine path for the raw screenshot image without html metadata
      var screenshotFileRelativePath = baselineImageRelativePath.Replace("Baseline", "PageComparison_NoMetaData");
      var screenshotPath = wasBaseLineAlreadyPresent ? $"{Directory.GetCurrentDirectory()}\\screenshots\\{testFixture}\\{testName}\\{DateTime.Now:yyMMdd_HHmmss}_{screenshotFileRelativePath}" : $"{baselineImagePath.Replace("Baseline.", "Baseline_NoMetaData.")}";

      var baselineDisplayPath = baselineImagePath.Replace($"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent}", "");

      log.WriteLine("");
      log.WriteLine($"comparing Page screenshot to baseline image...\n\tpage: {page.Url}");

      if (options != null)
      {
        log.DebugLine("compare options: ");
        log.DebugLine($"SensitivityThreshold = {options.SensitivityThreshold}");
        log.DebugLine($"IgnoreAntiAliasedPixels = {options.IgnoreAntiAliasedPixels}");
      }
      else
      {
        log.DebugLine("compare options: ");
        log.DebugLine($"SensitivityThreshold = 0.1f");
        log.DebugLine($"IgnoreAntiAliasedPixels = True");
      }

      if (!wasBaseLineAlreadyPresent)
      {
        log.DebugLine($"baseline image did not exist!");
        log.DebugLine($"- creating new baseline image at \n\t{baselineDisplayPath}");
      }

      // take screenshot of page
      await page.ScreenshotAsync(new()
      {
        Path = screenshotPath
      });
      await screenshotPath.SoftWaitUntilFileExistsAsync();

      var bitmapUtil = new BitmapUtil();

      // apply image mask if any before tagging html metadata
      if (ignoreRegions != null)
      {
        var maskedAndUntaggedScreenshotPath = await bitmapUtil.ApplyImageMaskAndOutputToPathAsync(ignoreRegions, screenshotPath, wasBaseLineAlreadyPresent);
        screenshotPath = maskedAndUntaggedScreenshotPath;
      }

      string screenshotWithMetaDataPath = screenshotPath;
      if (enableMetaDataTagging)
      {
        // add metadata to the screenshot
        var pageMetaData = (await page.Locator("body").InnerHTMLAsync()).Trim();
        screenshotWithMetaDataPath = screenshotPath.Replace("_NoMetaData", "");

        await ImageMetaDataHandler.AddMetaDataToFileDescriptionAndWriteToPathAsync(screenshotPath, screenshotWithMetaDataPath, pageMetaData);

        // delete the image without metadata
        if (ImageMetaDataHandler.ReadAndReturnMetaDataFromFileDescription(screenshotWithMetaDataPath).IsNotNullOrWhiteSpace())
          File.Delete(screenshotPath);
      }
      else
      {
        screenshotWithMetaDataPath = screenshotPath.Replace("_NoMetaData", "");
        if (File.Exists(screenshotPath) && !File.Exists(screenshotWithMetaDataPath))
        {
          File.Move(screenshotPath, screenshotWithMetaDataPath);
        }
        else
        {
          File.Delete(screenshotPath);
        }
      }

      if (File.Exists(baselineImagePath) && !wasBaseLineAlreadyPresent && screenshotWithMetaDataPath.EqualsAnyCase(baselineImagePath))
        log.WriteLine($"- baseline image created");

      if (!wasBaseLineAlreadyPresent) return;

      // compare new screenshot to baseline
      var numberOfMismatchedPixels = await bitmapUtil.CompareAndReturnNumberOfMismatchedPixelsAsync(screenshotWithMetaDataPath, baselineImagePath, options);

      log.DebugLine($"Screenshot: {screenshotWithMetaDataPath.Split("\\")[screenshotWithMetaDataPath.Split("\\").Length - 1]}");
      log.DebugLine($"path: {screenshotWithMetaDataPath.Replace(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "")}");
      log.DebugLine($"Baseline: {baselineImagePath.Split("\\")[baselineImagePath.Split("\\").Length - 1]}");
      log.DebugLine($"path: {baselineDisplayPath}");
      log.DebugLine("");
      log.WriteLine($"# of mismatched pixels: {numberOfMismatchedPixels}");

      // if anything changed visually between this screenshot and the last baseline, compare the html metadata and return the different nodes / node differences
      if (numberOfMismatchedPixels > 0 && enableMetaDataTagging)
        await baselineImagePath.CompareImageMetadataAndDisplayHtmlNodeDiffsAsync(screenshotWithMetaDataPath, log);

      // assert screenshots match / number of mismatched pixels = 0
      Assert.That(numberOfMismatchedPixels == 0, $"Page screenshot did not match the expected baseline image! # of mismatched pixels: {numberOfMismatchedPixels}\n\nss:\n{screenshotWithMetaDataPath} \nvs baseline:\n{baselineImagePath}");
    }

    /// ***********************************************************
    /// <remarks>Naming convention: baseline image file names should end with "_Baseline.png"
    /// BitmapCompareOptions: options for bitmap comparison, can set sensitivity threshold from 0 to 1 
    /// (lower values more sensitive to pixel differences) and anti-aliasing mode (true will exclude anti-aliased pixels from 
    /// image comparison). Uses default pixelmatch compare options if not provided
    /// IgnoreRegions: image mask regions to exclude from comparison, defined by X,Y origin point and Width/Height/Radius values
    /// Enable Metadata tagging to capture element HTML and record as image metadata for comparison </remarks>
    /// ***********************************************************
    public static async Task ToHaveExpectedScreenshotAsync(this ILocator locator, string baselineImageRelativePath, List<IgnoreRegion> ignoreRegions = null, BitmapCompareOptions? options = null, bool enableMetaDataTagging = false)
    {
      // ensure page is fully loaded, all images and dom content are loaded and rendered
      await locator.Page.WaitForDomContentToLoadAndRenderAsync(100);

      var log = new TestContextLogger();
      var dimensionsPrefix = $"{TestContext.Parameters["viewportWidth"]}x{TestContext.Parameters["viewportHeight"]}_";

      var testFixtureParts = TestContext.CurrentContext.Test.ClassName.Split('.');
      var testFixture = testFixtureParts[testFixtureParts.Length - 1];
      var testName = TestContext.CurrentContext.Test.MethodName;

      // check if baseline image already exists, if it does take a screenshot of the element and compare it to the baseline, otherwise save screenshot as the new baseline
      var baselineImagePath = $"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent}\\Resources\\Baselines\\{testFixture}\\{testName}\\{TestContext.Parameters["browser"]}\\elements\\{dimensionsPrefix}{baselineImageRelativePath}";

      if (ignoreRegions != null)
        baselineImagePath = baselineImagePath.Replace("_Baseline", "_Unmasked_Baseline");

      var wasBaseLineAlreadyPresent = File.Exists(baselineImagePath);

      // determine path for the raw screenshot image without html metadata
      var screenshotFileRelativePath = baselineImageRelativePath.Replace("Baseline", "ElementComparison_NoMetaData");
      var screenshotPath = wasBaseLineAlreadyPresent ? $"{Directory.GetCurrentDirectory()}\\screenshots\\{testFixture}\\{testName}\\{DateTime.Now:yyMMdd_HHmmss}_{screenshotFileRelativePath}" : baselineImagePath.Replace("Baseline.", "Baseline_NoMetaData.");

      var baselineDisplayPath = baselineImagePath.Replace($"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent}", "");

      log.WriteLine("");
      log.WriteLine($"comparing Element screenshot to baseline image...\n\tpage: {locator.Page.Url}\n\tlocator: {locator}");

      if (options != null)
      {
        log.DebugLine("compare options: ");
        log.DebugLine($"SensitivityThreshold = {options.SensitivityThreshold}");
        log.DebugLine($"IgnoreAntiAliasedPixels = {options.IgnoreAntiAliasedPixels}");
      }
      else
      {
        log.DebugLine("compare options: ");
        log.DebugLine($"SensitivityThreshold = 0.1f");
        log.DebugLine($"IgnoreAntiAliasedPixels = True");
      }

      if (!wasBaseLineAlreadyPresent)
      {
        log.WriteLine($"baseline image did not exist!");
        log.WriteLine($"- creating new baseline image at \n\t{baselineDisplayPath}");
      }

      // take screenshot of element
      await locator.ScreenshotAsync(new()
      {
        Path = screenshotPath
      });
      await screenshotPath.SoftWaitUntilFileExistsAsync();

      var bitmapUtil = new BitmapUtil();

      // apply image mask if any before tagging html metadata
      if (ignoreRegions != null)
      {
        var maskedAndUntaggedScreenshotPath = await bitmapUtil.ApplyImageMaskAndOutputToPathAsync(ignoreRegions, screenshotPath, wasBaseLineAlreadyPresent);
        screenshotPath = maskedAndUntaggedScreenshotPath;
      }

      string screenshotWithMetaDataPath = screenshotPath;
      if (enableMetaDataTagging)
      {
        // add metadata to the screenshot
        var elementMetaData = (await locator.InnerHTMLAsync()).Trim();
        screenshotWithMetaDataPath = screenshotPath.Replace("_NoMetaData", "");

        await ImageMetaDataHandler.AddMetaDataToFileDescriptionAndWriteToPathAsync(screenshotPath, screenshotWithMetaDataPath, elementMetaData);

        // delete the image without metadata
        if (ImageMetaDataHandler.ReadAndReturnMetaDataFromFileDescription(screenshotWithMetaDataPath).IsNotNullOrWhiteSpace())
          File.Delete(screenshotPath);
      }
      else
      {
        screenshotWithMetaDataPath = screenshotPath.Replace("_NoMetaData", "");
        if (File.Exists(screenshotPath) && !File.Exists(screenshotWithMetaDataPath))
        {
          File.Move(screenshotPath, screenshotWithMetaDataPath);
        }
        else
        {
          File.Delete(screenshotPath);
        }
      }

      if (File.Exists(baselineImagePath) && !wasBaseLineAlreadyPresent && screenshotWithMetaDataPath.EqualsAnyCase(baselineImagePath))
        log.WriteLine($"- baseline image created");

      if (!wasBaseLineAlreadyPresent) return;

      // compare new screenshot to baseline
      var numberOfMismatchedPixels = await bitmapUtil.CompareAndReturnNumberOfMismatchedPixelsAsync(screenshotWithMetaDataPath, baselineImagePath, options);

      log.DebugLine($"Screenshot: {screenshotWithMetaDataPath.Split("\\")[screenshotWithMetaDataPath.Split("\\").Length - 1]}");
      log.DebugLine($"path: {screenshotWithMetaDataPath.Replace(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "")}");
      log.DebugLine($"Baseline: {baselineImagePath.Split("\\")[baselineImagePath.Split("\\").Length - 1]}");
      log.DebugLine($"path: {baselineDisplayPath}");
      log.DebugLine("");
      log.WriteLine($"# of mismatched pixels: {numberOfMismatchedPixels}");

      // if anything changed visually between this screenshot and the last baseline, compare the html metadata and return the different nodes / node differences
      if (numberOfMismatchedPixels > 0 && enableMetaDataTagging)
        await baselineImagePath.CompareImageMetadataAndDisplayHtmlNodeDiffsAsync(screenshotWithMetaDataPath, log);

      // assert screenshots match / number of mismatched pixels = 0
      Assert.That(numberOfMismatchedPixels == 0, $"Element screenshot did not match the expected baseline image! # of mismatched pixels: {numberOfMismatchedPixels}\n\nss:\n{screenshotWithMetaDataPath} \nvs baseline:\n{baselineImagePath}");
    }

    /// ***********************************************************
    /// <remarks>Naming convention: baseline image file names should end with "_Baseline.png"
    /// BitmapCompareOptions: options for bitmap comparison, can set sensitivity threshold from 0 to 1 
    /// (lower values more sensitive to pixel differences) and anti-aliasing mode (true will exclude anti-aliased pixels from 
    /// image comparison). Uses default pixelmatch compare options if not provided
    /// IgnoreRegions: image mask regions to exclude from comparison, defined by X,Y origin point and Width/Height/Radius values
    /// Enable Metadata tagging to capture element HTML and record as image metadata for comparison </remarks>
    /// ***********************************************************
    public static async Task ToHaveExpectedFullScreenshotAsync(this IPage page, string baselineImageRelativePath, List<IgnoreRegion> ignoreRegions = null, BitmapCompareOptions? options = null, bool enableMetaDataTagging = false)
    {
      // ensure page is fully loaded, all images and dom content are loaded and rendered
      await page.WaitForDomContentToLoadAndRenderAsync();

      var log = new TestContextLogger();
      var dimensionsPrefix = $"{TestContext.Parameters["viewportWidth"]}x{TestContext.Parameters["viewportHeight"]}_";

      var testFixtureParts = TestContext.CurrentContext.Test.ClassName.Split('.');
      var testFixture = testFixtureParts[testFixtureParts.Length - 1];
      var testName = TestContext.CurrentContext.Test.MethodName;

      // check if baseline image already exists, if it does take a screenshot of the page and compare it to the baseline, otherwise save screenshot as the new baseline
      var baselineImagePath = $"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent}\\Resources\\Baselines\\{testFixture}\\{testName}\\{TestContext.Parameters["browser"]}\\fulls\\{dimensionsPrefix}{baselineImageRelativePath}";

      if (ignoreRegions != null)
        baselineImagePath = baselineImagePath.Replace("_Baseline", "_Unmasked_Baseline");

      var wasBaseLineAlreadyPresent = File.Exists(baselineImagePath);

      // determine path for the raw screenshot image without html metadata
      var screenshotFileRelativePath = baselineImageRelativePath.Replace("_Baseline", "_PageComparison_NoMetaData");
      var screenshotPath = wasBaseLineAlreadyPresent ? $"{Directory.GetCurrentDirectory()}\\screenshots\\{testFixture}\\{testName}\\{DateTime.Now:yyMMdd_HHmmss}_{screenshotFileRelativePath}" : baselineImagePath.Replace("_Baseline.png", "_Baseline_NoMetaData.png");

      var baselineDisplayPath = baselineImagePath.Replace($"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent}", "");

      log.WriteLine("");
      log.WriteLine($"comparing Page screenshot to baseline image...\n\tpage: {page.Url}");

      if (options != null)
      {
        log.DebugLine("compare options: ");
        log.DebugLine($"SensitivityThreshold = {options.SensitivityThreshold}");
        log.DebugLine($"IgnoreAntiAliasedPixels = {options.IgnoreAntiAliasedPixels}");
      }
      else
      {
        log.DebugLine("compare options: ");
        log.DebugLine($"SensitivityThreshold = 0.1f");
        log.DebugLine($"IgnoreAntiAliasedPixels = True");
      }

      if (!wasBaseLineAlreadyPresent)
      {
        log.WriteLine($"baseline image did not exist!");
        log.WriteLine($"- creating new baseline image at \n\t{baselineDisplayPath}");
      }

      // take full page screenshot
      await page.ScreenshotAsync(new()
      {
        Path = screenshotPath,
        FullPage = true
      });
      await screenshotPath.SoftWaitUntilFileExistsAsync();

      var bitmapUtil = new BitmapUtil();

      // apply image mask if any before tagging html metadata
      if (ignoreRegions != null)
      {
        var maskedAndUntaggedScreenshotPath = await bitmapUtil.ApplyImageMaskAndOutputToPathAsync(ignoreRegions, screenshotPath, wasBaseLineAlreadyPresent);
        screenshotPath = maskedAndUntaggedScreenshotPath;
      }

      string screenshotWithMetaDataPath = screenshotPath;
      if (enableMetaDataTagging)
      {
        // add metadata to the screenshot
        var pageMetaData = (await page.Locator("body").InnerHTMLAsync()).Trim();
        screenshotWithMetaDataPath = screenshotPath.Replace("_NoMetaData", "");

        await ImageMetaDataHandler.AddMetaDataToFileDescriptionAndWriteToPathAsync(screenshotPath, screenshotWithMetaDataPath, pageMetaData);

        // delete the image without metadata
        if (ImageMetaDataHandler.ReadAndReturnMetaDataFromFileDescription(screenshotWithMetaDataPath).IsNotNullOrWhiteSpace())
          File.Delete(screenshotPath);
      }
      else
      {
        screenshotWithMetaDataPath = screenshotPath.Replace("_NoMetaData", "");
        if (File.Exists(screenshotPath) && !File.Exists(screenshotWithMetaDataPath))
        {
          File.Move(screenshotPath, screenshotWithMetaDataPath);
        }
        else
        {
          File.Delete(screenshotPath);
        }
      }

      if (File.Exists(baselineImagePath) && !wasBaseLineAlreadyPresent && screenshotWithMetaDataPath.EqualsAnyCase(baselineImagePath))
        log.WriteLine($"- baseline image created");

      if (!wasBaseLineAlreadyPresent) return;

      // compare new screenshot to baseline
      var numberOfMismatchedPixels = await bitmapUtil.CompareAndReturnNumberOfMismatchedPixelsAsync(screenshotWithMetaDataPath, baselineImagePath, options);

      log.DebugLine($"Screenshot: {screenshotWithMetaDataPath.Split("\\")[screenshotWithMetaDataPath.Split("\\").Length - 1]}");
      log.DebugLine($"path: {screenshotWithMetaDataPath.Replace(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "")}");
      log.DebugLine($"Baseline: {baselineImagePath.Split("\\")[baselineImagePath.Split("\\").Length - 1]}");
      log.DebugLine($"path: {baselineDisplayPath}");
      log.DebugLine("");
      log.WriteLine($"# of mismatched pixels: {numberOfMismatchedPixels}");

      // if anything changed visually between this screenshot and the last baseline, compare the html metadata and return the different nodes / node differences
      if (numberOfMismatchedPixels > 0 && enableMetaDataTagging)
        await baselineImagePath.CompareImageMetadataAndDisplayHtmlNodeDiffsAsync(screenshotWithMetaDataPath, log);

      // assert screenshots match / number of mismatched pixels = 0
      Assert.That(numberOfMismatchedPixels == 0, $"Page screenshot did not match the expected baseline image! # of mismatched pixels: {numberOfMismatchedPixels}\n\nss:\n{screenshotWithMetaDataPath} \nvs baseline:\n{baselineImagePath}");
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

      log.WriteLine($"crawling site starting from {initialUrl} for {crawlTimeoutSeconds} seconds...");
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

          await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

          // execute logic on page
          log.WriteLine($"executing delegate logic on page...");
          await logicBlock.Invoke(page); // pass the current page to the logic block, execute whatever logic you need to on page, ie. page.RunAxeScanAsync page.ScreenshotAsync, etc

          // add to visitedUrls after navigating to
          visitedUrls.Add(currentUrl);

          // scroll to bottom & scrape all links on current page
          var links = await page.EvaluateAsync<string[]>(@"async () => {
    const scrollToBottom = async () => {
        await new Promise((resolve) => {
            let totalHeight = 0;
            const distance = 200;
            const timer = setInterval(() => {
                const scrollHeight = document.body.scrollHeight;
                window.scrollBy(0, distance);
                totalHeight += distance;

                if (totalHeight >= scrollHeight) {
                    clearInterval(timer);
                    resolve();
                }
            }, 100);
        });
    };
    
    // scroll to end
    await scrollToBottom();
    
    // wait for dynamic content to load
    await new Promise(resolve => setTimeout(resolve, 333));

    // scrape all links founds
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
        if (crawlTimeoutSeconds > 0)
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

    /// ***********************************************************
    public static async Task WaitForDomContentToLoadAndRenderAsync(this IPage page, int waitForContentToBeRenderedMs = 0)
    {
      await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
      if (waitForContentToBeRenderedMs > 0)
        await Task.Delay(waitForContentToBeRenderedMs);
      else
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
    /*-----------------------------------------------------------*/

    /*-----------------------------------------------------------*/
    /// ***********************************************************
    private static async Task<string> ApplyImageMaskAndOutputToPathAsync(this BitmapUtil bitmapUtil, List<IgnoreRegion> ignoreRegions, string outputPath, bool wasBaseLineAlreadyPresent)
    {
      // draw mask onto screenshot before adding metadata
      var maskedAndUntaggedScreenshotPath = outputPath.Replace("_Unmasked", "");
      using (var bitmap = bitmapUtil.CreateColorCorrectedBitmapFromImage(outputPath, ignoreRegions))
      {
        bitmap.Save(maskedAndUntaggedScreenshotPath);
        await maskedAndUntaggedScreenshotPath.SoftWaitUntilFileExistsAsync();
      }

      // delete raw baseline images after processing to keep TestData Baselines folder clean, keep raw comparison screenshots though for troubleshooting pages if anything changed
      if (File.Exists(maskedAndUntaggedScreenshotPath) && !wasBaseLineAlreadyPresent)
        File.Delete(outputPath);

      return maskedAndUntaggedScreenshotPath;
    }

    /// ***********************************************************
    private static async Task CompareImageMetadataAndDisplayHtmlNodeDiffsAsync(this string taggedBaselineImagePath, string taggedComparisonScreenshotPath, ILogger log)
    {
      // compare baseline and screenshot metadata to see what nodes changed
      var baselinePageHtml = ImageMetaDataHandler.ReadAndReturnMetaDataFromFileDescription(taggedBaselineImagePath);
      var screenshotPageHtml = ImageMetaDataHandler.ReadAndReturnMetaDataFromFileDescription(taggedComparisonScreenshotPath);
      var doElementHtmlsMatch = baselinePageHtml.Equals(screenshotPageHtml);
      log.DebugLine($"do page htmls match? {doElementHtmlsMatch}");

      var differentNodes = HtmlDocUtil.CompareHtmlStringsAndReturnDifferentNodes(screenshotPageHtml, baselinePageHtml);

      log.DebugLine($"# of node differences: {differentNodes.Count}");

      // display node differences
      foreach (var differentNode in differentNodes)
      {
        log.WriteLine(differentNode.ToString());
      }
    }
  }
}
