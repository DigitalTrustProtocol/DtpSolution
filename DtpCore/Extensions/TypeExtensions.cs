using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DtpCore.Extensions
{
    public static class TypeExtensions
    {
        public static string GetDisplayName<T>(this T c) where T: Type
        {
            return GetAttribute<DisplayNameAttribute>(c)?.DisplayName ?? c.Name;
        }

        public static string GetDescription<T>(this T c) where T : Type
        {
            return GetAttribute<DescriptionAttribute>(c)?.Description ?? string.Empty;
        }

        public static TAttribute GetAttribute<TAttribute>(Type type) => (TAttribute)type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault();
        public static TAttribute GetAttribute<TClass, TAttribute>() => (TAttribute)typeof(TClass).GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault();

    }
}
