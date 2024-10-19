using System.ComponentModel.DataAnnotations;

namespace E2e.Automation.Web.Tests
{
  /// ***********************************************************
  public static class TestCat
  {
    public const string Stable = "Stable";
    public const string InProgress = "InProgress";
  }

  /// ***********************************************************
  public enum TestEnv
  {
    [Display(Name = "Local")]
    Local = 0,
    [Display(Name = "Azure")]
    Azure = 1
  }
}
