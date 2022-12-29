using System.Net.Http;
using Common.Logging.Configuration;
using Ramp.Contracts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using NameValueCollection = System.Collections.Specialized.NameValueCollection;
using Ramp.Contracts.ViewModel;
using System.Reflection;

namespace Web.UI.Helpers
{
    public static class ExtensionMethods
    {
        public static MvcHtmlString StateDropDownListFor(this HtmlHelper helper, JsTreeModel _object)
        {
            //Dictionary<string, string> stateList = new Dictionary<string, string>();

            StringBuilder output = new StringBuilder();
            int widthInPixels = 0;

            if (_object.children.Count > 0)
            {
                
                output.Append("<ul class='dropdown-menu multi-level' role='menu' aria-labelledby='dropdownMenu'>");

                foreach (JsTreeModel subItem in _object.children)
                {
                    widthInPixels = (int)GetWidthOfString(subItem.data);

                    if (subItem.children.Count > 0)
                    {
                        output.Append($"<li class='dropdown-submenu'><a tabindex='-1' href='#' id='{subItem.attr.id}'>{subItem.data}</a>");
                        //output.Append("<li value =" + subItem.attr.id + " style='width: " + widthInPixels + "px;' " + " class='dropdown-submenu'  text='" + subItem.data + "' id='SelectedLI'><span class='ui-menu-icon ui-icon ui-icon-carat-1-e' style='position: absolute;right: 0px;'></span>");
                        //output.Append("<li value =" + subItem.attr.id + "  class='dropdown-submenu'     text='" + subItem.data + "' id='SelectedLI'><span class='ui-menu-icon ui-icon ui-icon-carat-1-e' style='position: absolute;right: 10px;'></span>");
                    }
                    else 
                    {
                        output.Append("<li style='width: " + widthInPixels + "px;' id='SelectedLI'><a href='#' id='" + subItem.attr.id + "'>" +subItem.data +"/a></li>");
                    }
                   
                    //output.Append(subItem.data);
                    //output.Append("<span class='textDemo'>" + subItem.data + "</span>");
                    output.Append(StateDropDownListFor(helper, subItem));
                    output.Append("</li>");
                }
                output.Append("</ul>");
            }
            return MvcHtmlString.Create(output.ToString());
            //{
            //    {"AL"," Alabama"},
            //    {"AK"," Alaska"},
            //    {"AZ"," Arizona"},
            //    {"AR"," Arkansas"}

            //};

            // return html.DropDownListFor(expression, new SelectList(stateList, "key", "value"));
        }


        private static float GetWidthOfString(string str)
        {
            Bitmap objBitmap = default(Bitmap);
            Graphics objGraphics = default(Graphics);

            objBitmap = new Bitmap(500, 200);
            objGraphics = Graphics.FromImage(objBitmap);

            SizeF stringSize = objGraphics.MeasureString(str, new Font("Times New Roman", 15));

            objBitmap.Dispose();
            objGraphics.Dispose();
            return stringSize.Width;
        }

        public static Uri SetQueryStringParameter(this Uri uri, string parameterName, string parameterValue)
        {
            var uriString = uri.ToString();

            if (!uriString.Contains("?"))
                return uri;

            var queryStringParameters = uri.ParseQueryString();

            queryStringParameters.Set(parameterName,parameterValue);

            var uriStringWithoutQueryString = uriString.Substring(0, uriString.IndexOf("?", System.StringComparison.Ordinal));

            return new Uri(uriStringWithoutQueryString.AddQueryStringParamters(queryStringParameters));
        }

        private static string AddQueryStringParamters(this string uriStringWithoutQueryString,
            NameValueCollection queryStringParameters)
        {
            var currentParameterIndex = 0;
            var parameters = queryStringParameters.AllKeys.SelectMany(queryStringParameters.GetValues,
                (k, v) => new KeyValuePair<string, string>(k, v));
            
            foreach (var parameter in parameters)
            {
                uriStringWithoutQueryString += parameter.GetParameterString(currentParameterIndex == 0);

                currentParameterIndex++;
            }

            return uriStringWithoutQueryString;
        }

        private static string GetParameterString(this KeyValuePair<string, string> parameter, bool isFirstParameter)
        {
            return string.Format(isFirstParameter ? "?{0}={1}" : "&{0}={1}", parameter.Key, parameter.Value);
        }

		public static string GetDescription(this Enum GenericEnum) {
			Type genericEnumType = GenericEnum.GetType();
			MemberInfo[] memberInfo = genericEnumType.GetMember(GenericEnum.ToString());
			if ((memberInfo != null && memberInfo.Length > 0)) {
				var _Attribs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
				if ((_Attribs != null && _Attribs.Count() > 0)) {
					return ((System.ComponentModel.DescriptionAttribute)_Attribs.ElementAt(0)).Description;
				}
			}
			return GenericEnum.ToString();
		}




        public static DateTime FirstDayOfMonth_AddMethod(this DateTime value)
        {
            return value.Date.AddDays(1 - value.Day);
        }

        public static DateTime FirstDayOfMonth_NewMethod(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1);
        }

        public static DateTime LastDayOfMonth_AddMethod(this DateTime value)
        {
            return value.FirstDayOfMonth_AddMethod().AddMonths(1).AddDays(-1);
        }

        public static DateTime LastDayOfMonth_AddMethodWithDaysInMonth(this DateTime value)
        {
            return value.Date.AddDays(DateTime.DaysInMonth(value.Year, value.Month) - value.Day);
        }

        public static DateTime LastDayOfMonth_SpecialCase(this DateTime value)
        {
            return value.AddDays(DateTime.DaysInMonth(value.Year, value.Month) - 1);
        }

        public static int DaysInMonth(this DateTime value)
        {
            return DateTime.DaysInMonth(value.Year, value.Month);
        }

        public static DateTime LastDayOfMonth_NewMethod(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, DateTime.DaysInMonth(value.Year, value.Month));
        }

        public static DateTime LastDayOfMonth_NewMethodWithReuseOfExtMethod(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.DaysInMonth());
        }

    }
}