using System;
using Core.Utilities.Init.Enums;

namespace Core.Utilities.Init.Attributes
{
  [AttributeUsage(AttributeTargets.Class)]
  public class StopModeAttribute : Attribute
  {
    public RunModes Mode { get; }

    public StopModeAttribute(RunModes mode)
    {
      Mode = mode;
    }
  }
}