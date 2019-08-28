using System;

namespace Core.Utilities.Init.Attributes
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
  public class OnStartRunAfterAttribute : Attribute
  {
    public Type Type { get; }

    public OnStartRunAfterAttribute(Type type)
    {
      Type = type ?? throw new ArgumentNullException(nameof(type));
    }
  }
}