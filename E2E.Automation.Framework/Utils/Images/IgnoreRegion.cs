namespace Jvu.TestAutomation.Web.Framework.Utils.Images
{
  /// ***********************************************************
  /// <remarks>defines region masks to exclude from images when doing visual comparison with BitmapUtil</remarks>
  /// ***********************************************************
  public class IgnoreRegion
  {
    /*-----------------------------------------------------------*/
    #region Instance Fields and Properties
    public required int X
    {
      get; set;
    }
    public required int Y
    {
      get; set;
    }
    public int? Width
    {
      get; set;
    }
    public int? Height
    {
      get; set;
    }
    public int? Radius
    {
      get; set;
    }

    // for excluding circle / ellipse shaped areas if necessary
    public const Double Pi = 3.141592653589793;
    #endregion
    /*-----------------------------------------------------------*/
  }
}