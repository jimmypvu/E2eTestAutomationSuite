using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using iText.Kernel.Pdf.Canvas.Parser.ClipperLib;
using StronglyTyped.PixelMatch;
using Jvu.TestAutomation.Web.Framework.Logging;

namespace Jvu.TestAutomation.Web.Framework.Utils.Images
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// ***********************************************************
  public class BitmapUtil
  {
    private ConsoleLogger _log = new();

    /// ***********************************************************
    public async Task<int> CompareAndReturnNumberOfMismatchedPixelsAsync(string imagePath1, string imagePath2, BitmapCompareOptions? options = null, List<IgnoreRegion> ignoreRegions = null)
    {
      // create bitmap images from input images, if ignoring regions create masked bitmap
      var bitmap1 = this.CreateColorCorrectedBitmapFromImage(imagePath1, ignoreRegions);
      var bitmap2 = this.CreateColorCorrectedBitmapFromImage(imagePath2, ignoreRegions);

      // images need to be the same size for comparison to work or pixelmatcher will throw; already logging baseline & comparison image paths in extension method
      _log.DebugLine($"- img1 dimensions: {bitmap1.Width} x {bitmap1.Height}");
      _log.DebugLine($"- img2 dimensions: {bitmap2.Width} x {bitmap2.Height}");
      _log.DebugLine($"comparing images...");

      var image1 = new BitmapImagePBgra32(bitmap1);
      var image2 = new BitmapImagePBgra32(bitmap2);

      // compare against baseline image
      var pixelMatcher = new PixelMatcher32
      {
        // use default image comparison options if none are provided
        Threshold = options != null ? options.SensitivityThreshold : 0.1f,
        IgnoreAntiAliasedPixels = options != null ? options.IgnoreAntiAliasedPixels : true
      };

      var numberOfMismatchedPixels = pixelMatcher.Compare(image1, image2);

      if (numberOfMismatchedPixels > 0)
        this.CreateComparisonResultDifferencesImagesAsync(imagePath2, imagePath1);

      return numberOfMismatchedPixels;
    }

    /// ***********************************************************
    public Bitmap CreateColorCorrectedBitmapFromImage(string sourceImagePath, List<IgnoreRegion> ignoreRegionsMask = null)
    {
      using (var fs = new FileStream(sourceImagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        // copy original image onto new bitmap, useIcm true to ensure color consistency across different devices and color profiles
        var bitmap = new Bitmap(fs, true);

        if (ignoreRegionsMask != null)
        {
          this._log.WriteLine($" - creating masked bitmap img from src file at\n\t\t\t{sourceImagePath.ToDisplayPath()}");
          this._log.WriteLine($" - applying regions mask...");

          // draw the ignoredRegions mask onto the bitmap -> loop through region area and color the ignored pixels green
          foreach (var region in ignoreRegionsMask)
          {
            var xMinValue = region.Radius == null ? region.X : region.X - region.Radius;
            var yMinValue = region.Radius == null ? region.Y : region.Y - region.Radius;
            var xMaxValue = region.Radius == null ? region.X + region.Width : region.X + region.Radius;
            var yMaxValue = region.Radius == null ? region.Y + region.Height : region.Y + region.Radius;

            // loop through the region starting from the origin / starting point
            for (int? x = xMinValue; x < xMaxValue; x++)
            {
              for (int? y = yMinValue; y < yMaxValue; y++)
              {
                // pixel is in the excluded area -> color / mask it with green and exclude that point from the visual check (technically still comparing those 'excluded' pixels, but since they are both masked green they are effectively not counted against any comparison differences, ie. greenMaskedPixel vs greenMaskedPixel = pixelsMatch / true)
                if (ShouldIgnorePixel((int)x, (int)y, ignoreRegionsMask))
                {
                  var maskColor = x % 2 == 0 ? System.Drawing.Color.LimeGreen : System.Drawing.Color.LawnGreen;

                  // lock image bits, LockBits instead of SetPixel for memory optimization (out of memory exceptions during image processing)
                  var bitmapBits = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                  try
                  {
                    var bytesPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                    var strideWidth = bitmapBits.Stride;
                    var ptr = bitmapBits.Scan0;
                    var rgbValues = new byte[strideWidth * bitmap.Height];
                    // copy original image data
                    Marshal.Copy(ptr, rgbValues, 0, rgbValues.Length);

                    // apply mask
                    int i = ((int)y * strideWidth) + ((int)x * bytesPerPixel);
                    rgbValues[i] = maskColor.B;
                    rgbValues[i + 1] = maskColor.G;
                    rgbValues[i + 2] = maskColor.R;
                    if (bytesPerPixel == 4)
                    {
                      rgbValues[i + 3] = maskColor.A;
                    }
                    // copy masked pixel data onto new bitmap
                    Marshal.Copy(rgbValues, 0, ptr, rgbValues.Length);
                  }
                  finally
                  {
                    // unlock image bits
                    bitmap.UnlockBits(bitmapBits);
                  }
                }
              }
            }
          }
          this._log.WriteLine($" - image masking completed");
        }
        return bitmap;
      }
    }

    /// ***********************************************************
    private void CreateComparisonResultDifferencesImagesAsync(string baselineImagePath, string comparisonImagePath)
    {
      // store the coordinates and color of each pixel
      var diffList = new List<Tuple<IntPoint, System.Drawing.Color>>();

      var comparisonBitmap = this.CreateColorCorrectedBitmapFromImage(comparisonImagePath);
      var baselineBitmap = this.CreateColorCorrectedBitmapFromImage(baselineImagePath);

      int searchWidth = baselineBitmap.Size.Width;
      int searchHeight = baselineBitmap.Size.Height;

      // loop through each pixel and compare values
      for (int i = 0; i < searchWidth; i++)
      {
        for (int j = 0; j < searchHeight; j++)
        {
          var targetColor = baselineBitmap.GetPixel(i, j);
          var pixelColor = comparisonBitmap.GetPixel(i, j);

          // if pixel colors don't match, add pixel list of differences
          if (pixelColor != targetColor)
          {
            diffList.Add(Tuple.Create(new IntPoint(i, j), pixelColor));
          }
        }
      }

      var testFixtureParts = TestContext.CurrentContext.Test.ClassName.Split('.');
      var testFixture = testFixtureParts[testFixtureParts.Length - 1];
      var testName = TestContext.CurrentContext.Test.MethodName;

      var baselineFileName = baselineImagePath.Replace($"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent}", "");
      var baselineFileNameParts = baselineFileName.Split("\\");
      baselineFileName = baselineFileNameParts[baselineFileNameParts.Length - 1];

      this.CreateDifferencesOnlyImage(diffList, new Bitmap(baselineImagePath), $"{Directory.GetCurrentDirectory()}\\screenshots\\{testFixture}\\{testName}\\{DateTime.Now:yyMMdd_HHmmss}_{baselineFileName.Replace("_Baseline.png", "")}_Diffs_Raw.png");
      this.CreateHighlightedDifferencesImage(diffList, new Bitmap(baselineImagePath), $"{Directory.GetCurrentDirectory()}\\screenshots\\{testFixture}\\{testName}\\{DateTime.Now:yyMMdd_HHmmss}_{baselineFileName.Replace("_Baseline.png", "")}_Diffs_Highlighted.png");
    }

    /// ***********************************************************
    private void CreateHighlightedDifferencesImage(List<Tuple<IntPoint, Color>> differentPixelsList, Bitmap baselineBitmap, string outputPath)
    {
      using (Bitmap result = new Bitmap(baselineBitmap.Width, baselineBitmap.Height, PixelFormat.Format32bppArgb))
      {
        // extract pixel coordinates from diffs list to a hashset for faster lookup
        var differentIntPointsLookupSet = new HashSet<IntPoint>(differentPixelsList.Select(pixel => pixel.Item1));

        // lock bits for both baseline and result bitmaps
        BitmapData baselineData = baselineBitmap.LockBits(new Rectangle(0, 0, baselineBitmap.Width, baselineBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        BitmapData resultData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        try
        {
          int bytesPerPixel = 4;
          int baselineStride = baselineData.Stride;
          int resultStride = resultData.Stride;
          int height = result.Height;
          int width = result.Width;

          byte[] baselinePixels = new byte[baselineStride * height];
          byte[] resultPixels = new byte[resultStride * height];

          // copy baseline bitmap data
          Marshal.Copy(baselineData.Scan0, baselinePixels, 0, baselinePixels.Length);

          // loop through and process pixels
          for (int y = 0; y < height; y++)
          {
            for (int x = 0; x < width; x++)
            {
              int index = y * resultStride + x * bytesPerPixel;

              if (differentIntPointsLookupSet.Contains(new IntPoint(x, y)))
              {
                // highlight if diff pixel
                var maskColor = x % 2 == 0 ? Color.DeepPink : Color.HotPink;
                resultPixels[index] = maskColor.B;
                resultPixels[index + 1] = maskColor.G;
                resultPixels[index + 2] = maskColor.R;
                resultPixels[index + 3] = 255; // Alpha
              }
              else
              {
                // otherwise copy original pixel
                resultPixels[index] = baselinePixels[index];
                resultPixels[index + 1] = baselinePixels[index + 1];
                resultPixels[index + 2] = baselinePixels[index + 2];
                resultPixels[index + 3] = baselinePixels[index + 3];
              }
            }
          }
          // copy processed pixels back onto the result bitmap
          Marshal.Copy(resultPixels, 0, resultData.Scan0, resultPixels.Length);
        }
        finally
        {
          // unlock bits
          baselineBitmap.UnlockBits(baselineData);
          result.UnlockBits(resultData);
        }
        // save to output path
        result.Save(outputPath, ImageFormat.Png);
      }
    }

    /// ***********************************************************
    private void CreateDifferencesOnlyImage(List<Tuple<IntPoint, System.Drawing.Color>> diffPixelsList, Bitmap baselineBitmap, string outputPath)
    {
      using (Bitmap result = new Bitmap(baselineBitmap.Width, baselineBitmap.Height, PixelFormat.Format32bppArgb))
      {
        // use LockBits for better memory optimization
        BitmapData bmpData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
          int bytesPerPixel = 4;
          int strideWidth = bmpData.Stride;
          byte[] pixels = new byte[strideWidth * result.Height];

          foreach (var pixel in diffPixelsList)
          {
            int x = (int)pixel.Item1.X;
            int y = (int)pixel.Item1.Y;
            Color color = pixel.Item2;

            if (x >= 0 && x < result.Width && y >= 0 && y < result.Height)
            {
              int index = y * strideWidth + x * bytesPerPixel;
              pixels[index] = color.B;
              pixels[index + 1] = color.G;
              pixels[index + 2] = color.R;
              pixels[index + 3] = color.A;
            }
          }
          Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
        }
        finally
        {
          result.UnlockBits(bmpData);
        }
        result.Save(outputPath, ImageFormat.Png);
      }
    }

    /// ***********************************************************
    private bool ShouldIgnorePixel(int x, int y, List<IgnoreRegion> ignoreRegions)
    {
      if (ignoreRegions == null) return false;

      foreach (var region in ignoreRegions)
      {
        // check the region shape to determine how to loop over the region area
        if (region.Radius != null || region.Radius > 0) // circular area
        {
          // a point at (a,b) is within a circle at point (x,y) with radius r if sqrt((a-x)^2 + (b-y)^2) <= r  ---> a^2 + b^2 = c^2
          var distanceToOriginX = Math.Abs(x - region.X);
          var distanceToOriginY = Math.Abs(y - region.Y);
          if (distanceToOriginX * distanceToOriginX + distanceToOriginY * distanceToOriginY < region.Radius * region.Radius)
          {
            return true;
          }
        }
        else
        {
          // rect area, check if pixel is within the bounds of the rect, if it is than return true and ignore the pixel
          if (x >= region.X && x < region.X + region.Width &&
              y >= region.Y && y < region.Y + region.Height)
          {
            return true;
          }
        }
      }
      return false;
    }
  }
}