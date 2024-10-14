using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvu.TestAutomation.Web.Framework.Logging
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// <remarks>ILogger interace</remarks>
  /// ***********************************************************
  public interface ILogger
  {
    /*-----------------------------------------------------------*/
    #region Public Methods
    /// ***********************************************************
    public void WriteLine(string message);
    /// ***********************************************************
    public void DetailLine(string message);
    /// ***********************************************************
    public void DebugLine(string message);
    /// ***********************************************************
    public void WriteException(Exception ex);
    #endregion
    /*-----------------------------------------------------------*/
  }
}
