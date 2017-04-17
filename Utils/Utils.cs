using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class Util
    {
        public static IEnumerable<Type> GetTypesWithAttribute(Type attribute)
        {
            string definedIn = attribute.Assembly.GetName().Name;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                // Note that we have to call GetName().Name.  Just GetName() will not work.  The following
                // if statement never ran when I tried to compare the results of GetName().
                if ((!assembly.GlobalAssemblyCache) && ((assembly.GetName().Name == definedIn) || assembly.GetReferencedAssemblies().Any(a => a.Name == definedIn)))
                    foreach (Type type in assembly.GetTypes())
                        if (type.GetCustomAttributes(attribute, true).Length > 0)
                        {
                            yield return type;
                        }
        }

        public static bool ReturnTrue(params object[] args)
        {
            return true;
        }

        public static bool ReturnFalse(params object[] args)
        {
            return false;
        }

        // formatter for forms of
        // seconds/hours/day
        public class HMSFormatter : ICustomFormatter, IFormatProvider
        {
            // list of Formats, with a P customformat for pluralization
            static Dictionary<string, string> timeformats = new Dictionary<string, string> {
                {"L", "{0:P:Milliseconds:Millisecond}"},
                {"S", "{0:P:Seconds:Second}"},
                {"M", "{0:P:Minutes:Minute}"},
                {"H","{0:P:Hours:Hour}"},
                {"D", "{0:P:Days:Day}"}
            };

            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                return string.Format(new PluralFormatter(), timeformats[format], arg);
            }

            public object GetFormat(Type formatType)
            {
                return formatType == typeof(ICustomFormatter) ? this : null;
            }
        }

        // formats a numeric value based on a format P:Plural:Singular
        public class PluralFormatter : ICustomFormatter, IFormatProvider
        {

            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                if (arg != null)
                {
                    var parts = format.Split(':'); // ["P", "Plural", "Singular"]

                    if (parts[0] == "P") // correct format?
                    {
                        // which index postion to use
                        int partIndex = (arg.ToString() == "1") ? 2 : 1;
                        // pick string (safe guard for array bounds) and format
                        return String.Format("{0} {1}", arg, (parts.Length > partIndex ? parts[partIndex] : ""));
                    }
                }
                return String.Format(format, arg);
            }

            public object GetFormat(Type formatType)
            {
                return formatType == typeof(ICustomFormatter) ? this : null;
            }
        }
    }
}
