using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Ramp.Services.Helpers
{
    public static class StringExtensions
    {
        public static string TrimAllCastToLowerInvariant(this string str)
        {
            return TrimAll(str)?.ToLowerInvariant();
        }

        public static string TrimAll(this string str)
        {
            if (str == null)
                return null;
            return Regex.Replace(str, @"\s+", string.Empty).TrimEnd();
        }

        public static string RemoveSpecialCharacters(this string str)
        {
            
            var lookup = new char[]
            {
                '\\','/',':','*','?','"','<','>','|','+','_','@','&','$','#','%',';',','
            };
            var io = System.IO.Path.GetInvalidFileNameChars();

            var sb = new StringBuilder(str.Length);
            foreach (var c in str)
            {
                if (!lookup.Contains(c) && !io.Contains(c))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static string GetFirstName(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return string.Empty;
            return str.Split(' ')[0];
        }

        public static string GetLastName(this string str)
        {
            var lastName = new StringBuilder();
            var names = str.Split(' ');
            if (names.Length > 1)
            {
                for (var i = 1; i < names.Length; i++)
                {
                    if (i == 1)
                        lastName.Append(names[i]);
                    else
                    {
                        lastName.AppendFormat(" {0}", names[i]);
                    }
                }
            }
            return lastName.ToString();
        }
        public static IEnumerable<string> FindHTMLTags(this string instance, string tag,params string[] classes)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(instance) || string.IsNullOrEmpty(tag))
                return result;
            var stringIndex = 0;
            var temp = instance;
            do
            {
                stringIndex = temp.IndexOf("<" + tag);
                if (stringIndex > -1)
                    if (stringIndex > -1)
                    {
                        temp = temp.Substring(stringIndex);
                        var closingTagIndex = temp.IndexOf(">");
                        if (closingTagIndex > -1)
                        {
                            var t = temp.Substring(0, closingTagIndex + 1);
                            if (classes.Any() && classes.Where(x => x.WhereHtmlAttrContains("class", x)).RemoveEmpty().Any())
                                result.Add(t);
                            else
                                result.Add(t);
                        }
                        temp = temp.Substring(closingTagIndex);
                    }
                stringIndex = temp.IndexOf("<" + tag);
            } while (stringIndex < temp.Length && stringIndex > -1);
            return result;
        }
        public static IEnumerable<string> Apply(this IEnumerable<string> instances, Func<string, string> func)
        {
            return instances.Select(x => func(x)).ToList();
        }
        public static string FindHtmlAttr(this string tag,string attribute)
        {
            var result = string.Empty;
            if (!string.IsNullOrWhiteSpace(tag) && !string.IsNullOrWhiteSpace(attribute))
                result = tag.SubstringAfter(attribute+"=\"").SubstringBefore("\"");
            return result;
        }
        public static string FindInlineCss(this string tag,string cssRule)
        {
            var result = string.Empty;
            if (!string.IsNullOrWhiteSpace(tag) && !string.IsNullOrWhiteSpace(cssRule))
            {
                result = tag.FindHtmlAttr("style").SubstringAfter(cssRule + ":");
                result = result.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
            }
            return result;
        }
        public static bool WhereHtmlAttrContains(this string tag, string attribute, string value)
        {
            return tag.FindHtmlAttr(attribute).IndexOf(value) > -1;
        }
        public static string SubstringAfter(this string instance, string pattern)
        {
            var result = string.Empty;
            if (!string.IsNullOrWhiteSpace(instance))
            {
                var index = instance.IndexOf(pattern);
                if (index > -1)
                    return instance.Substring(index +  pattern.Length);
            }
            return string.Empty;
        }
        public static string SubstringBefore(this string instance, string pattern)
        {
            var result = string.Empty;
            if (!string.IsNullOrWhiteSpace(instance))
            {
                var index = instance.IndexOf(pattern);
                if (index > -1)
                    return instance.Substring(0, index);
            }
            return string.Empty;
        }
        public static IEnumerable<string> RemoveEmpty(this IEnumerable<string> enumerable)
        {
            return enumerable.Where(x => !string.IsNullOrEmpty(x)).ToList();
        }
    }
}