using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DtpCore.Extensions
{
    public static class EqualsExtensions
    {
        public static bool JsonCompare(this object obj, object another)
        {
            if (ReferenceEquals(obj, another)) return true;
            if ((obj == null) || (another == null)) return false;
            if (obj.GetType() != another.GetType()) return false;

            var objJson = JsonConvert.SerializeObject(obj);
            var anotherJson = JsonConvert.SerializeObject(another);

            return objJson == anotherJson;
        }
    }
}
