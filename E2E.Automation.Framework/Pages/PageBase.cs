using E2e.Automation.Framework.Testing;

namespace E2e.Automation.Framework.Pages
{
  /// ***********************************************************
  /// <author>JVU</author>
  /// <version>1.0.0</version>
  /// <remarks>Base class for Page models for Web tests</remarks>
  /// ***********************************************************
  public abstract class PageBase
  {
    /*-----------------------------------------------------------*/
    private IPage _page { get; set; }

    public IPage Page => _page;

    public TestBase? TestBase { get; private set; }

    public ILogger Log { get; private set; }
    /*-----------------------------------------------------------*/

    /*-----------------------------------------------------------*/
    /// ***********************************************************
    public PageBase(IPage page)
    {
      this._page = page;
      this.Log = new TestContextLogger();
      this.TestBase = null;
    }

    /// ***********************************************************
    public PageBase(IPage page, TestBase test)
    {
      this._page = page;
      this.Log = test;
      this.TestBase = test;
    }
    /*-----------------------------------------------------------*/

    /*-----------------------------------------------------------*/
    /// ***********************************************************
    public void SetTestBase(TestBase test)
    {
      this.TestBase = test;
      this.Log = test;
    }
    /// ***********************************************************
    public void SetPage(IPage page)
    {
      this._page = page;
    }
    /*-----------------------------------------------------------*/
  }
}
