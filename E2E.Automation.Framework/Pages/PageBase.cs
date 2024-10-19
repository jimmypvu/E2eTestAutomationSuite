using Jvu.TestAutomation.Web.Framework.Testing;

namespace Jvu.TestAutomation.Web.Framework.Pages
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// <remarks></remarks>
  /// ***********************************************************
  public abstract class PageBase
  {
    /*-----------------------------------------------------------*/
    private IPage? _page;

    public IPage? Page
    {
      get
      {
        return _page;
      }
    }

    public TestBase? TestBase
    {
      get; private set;
    }

    public ILogger Log
    {
      get; private set;
    }

    public PageBase()
    {
      this._page = null;
      this.Log = null;
      this.TestBase = null;
    }

    public PageBase(IPage page)
    {
      this._page = page;
      this.Log = null;
      this.TestBase = null;
    }

    public PageBase(IPage page, TestBase test)
    {
      this._page = page;
      this.Log = test;
      this.TestBase = test;
    }

    internal void SetTestBase(TestBase test)
    {
      this.TestBase = test;
      this.Log = test;
    }

    internal void SetPage(IPage page)
    {
      this._page = page;
    }
    /*-----------------------------------------------------------*/
  }
}
