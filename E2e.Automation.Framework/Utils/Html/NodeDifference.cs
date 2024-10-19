using System.Text;
using HtmlAgilityPack;

namespace E2e.Automation.Framework.Utils.Html
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>12.0</version>
  /// ***********************************************************
  public class NodeDifference
  {
    /*-----------------------------------------------------------*/
    #region Instance Fields and Properties
    public HtmlNode Node1 { get; }
    public HtmlNode Node2 { get; }
    public string Reason { get; }
    public List<AttributeDifference> AttributeDifferences { get; }
    #endregion
    /*-----------------------------------------------------------*/

    /*-----------------------------------------------------------*/
    #region Constructors
    /// ***********************************************************
    public NodeDifference(HtmlNode node1, HtmlNode node2, string reason, List<AttributeDifference> attributeDifferences = null)
    {
      Node1 = node1;
      Node2 = node2;
      Reason = reason;
      AttributeDifferences = attributeDifferences ?? new List<AttributeDifference>();
    }
    #endregion
    /*-----------------------------------------------------------*/
    #region Public Methods
    /// ***********************************************************
    /// <author>JVU</author>
    /// <version>12.0</version>
    /// ***********************************************************
    public override string ToString()
    {
      string node1Desc = Node1 != null ? $"<{Node1.Name}> ({Node1.NodeType})" : "null";
      string node2Desc = Node2 != null ? $"<{Node2.Name}> ({Node2.NodeType})" : "null";

      var nodeDifferenceMessage = Node1 == null ?
        $"difference at {Node2.XPath}:\n{node1Desc} vs {node2Desc} - {Reason}" :
        $"difference at {Node1.XPath}:\n{node1Desc} vs {node2Desc} - {Reason}";

      if (AttributeDifferences.Count == 0)
      {
        return nodeDifferenceMessage;
      }
      else
      {
        var sb = new StringBuilder();
        sb.AppendLine(nodeDifferenceMessage);
        foreach (var diff in AttributeDifferences)
        {
          sb.AppendLine(diff.ToString());
        }
        return sb.ToString();
      }
    }
    #endregion
    /*-----------------------------------------------------------*/
  }
}