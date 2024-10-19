using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Text;

namespace Jvu.TestAutomation.Web.Framework.Utils.Images
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// ***********************************************************
  public static class ImageMetaDataHandler
  {
    /// ***********************************************************
    public static async Task AddMetaDataToFileDescriptionAndWriteToPathAsync(string inputImagePath, string outputPath, string metaData)
    {
      var log = new ConsoleLogger();
      log.DebugLine($"tagging HTML metadata to source img at\n\t\t\t{inputImagePath.ToDisplayPath()}...");
      log.DebugLine($"output img path:\n\t\t\t{outputPath.ToDisplayPath()}");

      try
      {
        using (var inputStream = new FileStream(inputImagePath, FileMode.Open, FileAccess.Read))
        using (var img = Image.FromStream(inputStream, false, false))
        {
          // create new PropertyItem to store metadata
          var propItem = (PropertyItem)typeof(PropertyItem).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null).Invoke(null);
          propItem.Id = 0x010E; // arbitrary ID
          propItem.Type = 2; // ASCII string type
          propItem.Value = Encoding.ASCII.GetBytes(metaData + '\0');
          propItem.Len = propItem.Value.Length;

          // add PropertyItem to image
          img.SetPropertyItem(propItem);

          // save image as png
          using (var outputStream = new FileStream(outputPath, FileMode.Create))
          {
            img.Save(outputStream, ImageFormat.Png);
          }
        }
        await outputPath.SoftWaitUntilFileExistsAsync();

        if (File.Exists(outputPath) && !string.IsNullOrWhiteSpace(ReadAndReturnMetaDataFromFileDescription(outputPath)))
        {
          log.DebugLine($"metadata-tagged image successfully saved");
        }
        else
        {
          log.DebugLine($"failed to save metadata-tagged image or metadata is empty");
        }
      }
      catch (OutOfMemoryException ex)
      {
        log.DebugLine($"out of memory exception: {ex.Message}");
        throw;
      }
      catch (Exception ex)
      {
        log.DebugLine($"error processing image: {ex.Message}");
        throw;
      }
    }

    /// ***********************************************************
    public static string ReadAndReturnMetaDataFromFileDescription(string imagePath)
    {
      using (Image img = Image.FromFile(imagePath))
      {
        // retrieve image's PropertyItems
        foreach (var prop in img.PropertyItems)
        {
          // if propId matches the Description ID above (0x010E) return the Description metadata value
          if (prop.Id == 0x010E)
          {
            string metadataDescription = Encoding.ASCII.GetString(prop.Value).TrimEnd('\0');
            return metadataDescription;
          }
        }
      }
      return "no metadata found";
    }
  }
}