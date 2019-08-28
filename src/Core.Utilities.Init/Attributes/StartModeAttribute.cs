using System;
using Core.Utilities.Init.Enums;

namespace Core.Utilities.Init.Attributes
{
  [AttributeUsage(AttributeTargets.Class)]
  public class StartModeAttribute : Attribute
  {
    public RunModes Mode { get; }

    public StartModeAttribute(RunModes mode)
    {
      Mode = mode;
    }
  }
}
