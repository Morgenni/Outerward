using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Outerward.Extensions
{
    internal static class XElementExtensions
    {
        #nullable enable
        public static T? ValueOrNull<T>(this XElement element, XName childElement) where T : struct
        {
            if (element.Element(childElement) != null)
                return (T)Convert.ChangeType(element.Element(childElement).Value, typeof(T));
            return null;
        }
        public static string? ValueOrNull(this XElement element, XName childElement) => element.Element(childElement)?.Value;
    }
}
