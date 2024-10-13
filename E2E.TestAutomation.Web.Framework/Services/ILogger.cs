using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvu.TestAutomation.Web.Framework.Services
{
  public interface ILogger
  {
    public void WriteLine(string message);
    public void DebugLine(string message);
    public void WriteException(Exception ex);
  }
}
