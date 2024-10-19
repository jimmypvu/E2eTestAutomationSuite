namespace E2e.Automation.Framework.Utils.Html
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>12.0</version>
  /// ***********************************************************
  public class AttributeDifference
  {
    /*-----------------------------------------------------------*/
    #region Instance Fields and Properties
    public string Name { get; }
    public string Value1 { get; }
    public string Value2 { get; }
    public string Reason { get; }
    #endregion
    /*-----------------------------------------------------------*/

    /*-----------------------------------------------------------*/
    #region Constructors
    /// ***********************************************************
    public AttributeDifference(string name, string value1, string value2, string reason)
    {
      Name = name;
      Value1 = value1;
      Value2 = value2;
      Reason = reason;
    }
    #endregion
    /*-----------------------------------------------------------*/
    #region Public Methods
    /// ***********************************************************
    public override string ToString()
    {
      return @$"- Attribute '{Name}' - {Reason}:
  '{Value1}'
  vs
  '{Value2}'";
    }
    #endregion
    /*-----------------------------------------------------------*/
  }
}