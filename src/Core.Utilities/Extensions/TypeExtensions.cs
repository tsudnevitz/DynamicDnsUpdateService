using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Utilities.Extensions
{
  public static class TypeExtensions
  {
    public static T GetAttribute<T>(this object instance)
    {
      return instance.GetAttributes<T>().SingleOrDefault();
    }

    public static IEnumerable<T> GetAttributes<T>(this object instance)
    {
      if (instance == null)
        throw new ArgumentNullException(nameof(instance));

      return instance.GetType().GetCustomAttributes(typeof(T), true).Cast<T>();
    }
  }
}