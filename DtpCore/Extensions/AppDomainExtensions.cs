using System;
using System.Collections.Generic;
using System.Reflection;

namespace DtpCore.Extensions
{
    public static class AppDomainExtensions
    {
        public static IEnumerable<Type> FindTypes(this AppDomain app, Func<Type, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            foreach (var assembly in app.GetAssemblies())
            {
                if (!assembly.IsDynamic)
                {
                    Type[] exportedTypes = null;
                    try
                    {
                        exportedTypes = assembly.GetExportedTypes();
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        exportedTypes = e.Types;
                    }

                    if (exportedTypes != null)
                    {
                        foreach (var type in exportedTypes)
                        {
                            if (predicate(type))
                                yield return type;
                        }
                    }
                }
            }
        }
    }
}
