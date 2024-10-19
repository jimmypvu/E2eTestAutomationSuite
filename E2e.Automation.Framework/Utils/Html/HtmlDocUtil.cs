using HtmlAgilityPack;

namespace E2e.Automation.Framework.Utils.Html
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>12.0</version>
  /// ***********************************************************
  public class HtmlDocUtil
  {
    /*-----------------------------------------------------------*/
    #region Static Fields/Properties and Constants
    private static ILogger _log = new TestContextLogger();
    #endregion
    /*-----------------------------------------------------------*/

    /*-----------------------------------------------------------*/
    #region Static Public Methods
    /// ***********************************************************
    /// <author>JVU</author>
    /// <version>12.0</version>
    /// ***********************************************************
    public static List<NodeDifference> CompareHtmlStringsAndReturnDifferentNodes(string htmlString1, string htmlString2)
    {
      // load htmlDoc objects from html strings
      var doc1 = new HtmlDocument();
      var doc2 = new HtmlDocument();

      doc1.LoadHtml(htmlString1);
      doc2.LoadHtml(htmlString2);

      // get the first child of the html node, which should be our root element node
      var rootNode1 = doc1.DocumentNode.FirstChild;
      var rootNode2 = doc2.DocumentNode.FirstChild;

      _log.DebugLine("screenshot HTML: \n" + doc1.ParsedText);
      _log.DebugLine("baseline HTML: \n" + doc2.ParsedText);

      // compare nodes and return list of different nodes
      return CompareNodes(rootNode1, rootNode2);
    }
    #endregion
    /*-----------------------------------------------------------*/
    #region Static Private Methods
    /// ***********************************************************
    /// <author>JVU</author>
    /// <version>12.0</version>
    /// ***********************************************************
    private static List<NodeDifference> CompareNodes(HtmlNode node1, HtmlNode node2)
    {
      // list to store differences
      var differences = new List<NodeDifference>();

      if (node1 == null && node2 == null)
        return differences;

      if (node1 == null || node2 == null)
      {
        var nullNodeMessage = node1 == null ?
          $"screenshotted page / element html is missing an expected <{node2.Name}> ({node2.NodeType}) node at {node2.XPath}, node was null instead!" :
          $"screenshotted page / element html contained a new <{node1.Name}> ({node1.NodeType}) node at {node1.XPath} that was not present in the baseline!";
        differences.Add(new NodeDifference(node1, node2, nullNodeMessage));
        return differences;
      }

      // nodes can differ in type, name, text content / value, and attributes
      if (node1.NodeType != node2.NodeType)
      {
        // compare node types: element, text, comment, or document
        differences.Add(new NodeDifference(node1, node2, $"different node types (screenshot vs baseline): \n{node1.NodeType}\nvs\n{node2.NodeType}"));
      }
      else if (node1.Name != node2.Name)
      {
        // compare node names: tag or element name ie. div p label input
        differences.Add(new NodeDifference(node1, node2, $"different node names (screenshot vs baseline): \n{node1.Name}\nvs\n{node2.Name}"));
      }
      else if (node1.NodeType == HtmlNodeType.Text && node2.NodeType == HtmlNodeType.Text)
      {
        // compare node text content
        if (node1.InnerText.Trim() != node2.InnerText.Trim())
          differences.Add(new NodeDifference(node1, node2, $"different node text content (screenshot vs baseline): \n{node1.InnerText.Trim()}\nvs\n{node2.InnerText.Trim()}"));
      }
      else
      {
        // compare node attributes; attributes can differ in presence and value
        var attributeDifferences = CompareAttributes(node1.Attributes, node2.Attributes);
        if (attributeDifferences.Any())
          differences.Add(new NodeDifference(node1, node2, "different node attributes (screenshot vs baseline): ", attributeDifferences));
      }

      // recurse down doc nodes and compare child nodes
      var childNodes1 = node1.ChildNodes.ToList();
      var childNodes2 = node2.ChildNodes.ToList();
      int maxChildCount = Math.Max(childNodes1.Count, childNodes2.Count);

      for (int i = 0; i < maxChildCount; i++)
      {
        HtmlNode childNode1 = i < childNodes1.Count ?
          childNodes1[i] :
          null;
        HtmlNode childNode2 = i < childNodes2.Count ?
          childNodes2[i] :
          null;

        differences.AddRange(CompareNodes(childNode1, childNode2));
      }

      return differences;
    }

    /// ***********************************************************
    /// <author>JVU</author>
    /// <version>12.0</version>
    /// ***********************************************************
    private static List<AttributeDifference> CompareAttributes(HtmlAttributeCollection nodeAttributes1, HtmlAttributeCollection nodeAttributes2)
    {
      var differences = new List<AttributeDifference>();

      // loop through attributes in 1st node and compare if 2nd node contains same attribute name and value
      foreach (var attribute in nodeAttributes1)
      {
        if (!nodeAttributes2.Contains(attribute.Name))
        {
          differences.Add(new AttributeDifference(attribute.Name, attribute.Value, null, "missing from baseline html element"));
        }
        else if (attribute.Value != nodeAttributes2[attribute.Name].Value)
        {
          differences.Add(new AttributeDifference(attribute.Name, attribute.Value, nodeAttributes2[attribute.Name].Value, "different attribute values"));
        }
      }

      // do the same for the 2nd node / collection of attributes
      foreach (var attribute in nodeAttributes2)
      {
        if (!nodeAttributes1.Contains(attribute.Name))
        {
          differences.Add(new AttributeDifference(attribute.Name, null, attribute.Value, "missing from comparison html element"));
        }
      }

      return differences;
    }
    #endregion
    /*-----------------------------------------------------------*/
  }
}