using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Common.Web
{
    public static class Extensions
    {
        public static MvcHtmlString ToJson(this HtmlHelper html, object obj)
        {
            return ToJson(obj);
        }
        public static MvcHtmlString ToJson(object obj)
        {
            var s = new JsonSerializer();
            var sb = new StringBuilder();
            using (var stringwriter = new StringWriter(sb))
            {
                s.Serialize(stringwriter, obj);
            }

            return MvcHtmlString.Create(sb.ToString());
        }
        public static MvcHtmlString JsonKnockoutValidationFor<TModel>()
        {
            var model = typeof(TModel);
            var propertyDictionary = new Dictionary<string, List<object>>();
            foreach (var property in model.GetProperties())
            {
                var customAttributes = property.GetCustomAttributes(true);
                if (customAttributes.Any())
                {
                    var attributesList = new List<object>();

                    foreach (var attribute in customAttributes)
                    {
                        if (attribute is RegularExpressionAttribute)
                        {
                            var regex = attribute as RegularExpressionAttribute;
                            attributesList.Add(new { pattern = new { message = regex.ErrorMessage, @params = regex.Pattern } });
                        }
                        if (attribute is RequiredAttribute)
                        {
                            var required = attribute as RequiredAttribute;
                            attributesList.Add(new { required = new { message = required.ErrorMessage, @params = true } });
                            attributesList.Add(new { notify = "always" });
                        }
                        if (attribute is StringLengthAttribute)
                        {
                            var stringLength = attribute as StringLengthAttribute;
                            attributesList.Add(new { maxLength = new { message = stringLength.ErrorMessage, @params = stringLength.MaximumLength } });
                            if (stringLength.MinimumLength != default(int))
                            {
                                attributesList.Add(new { minLength = new { message = stringLength.ErrorMessage, @params = stringLength.MinimumLength } });
                            }
                        }
                        if (attribute is RemoteAttribute)
                        {
                            var remote = attribute as RemoteAttribute;
                            attributesList.Add(new { remote = new { message = remote.ErrorMessage, @params = new { url = remote.HttpMethod, property = property.Name } } });
                        }
                    }
                    propertyDictionary.Add(property.Name, attributesList);
                }
            }
            return ToJson(propertyDictionary);
        }
        public static MvcHtmlString JsonKnockoutValidationFor<TModel>(this HtmlHelper<TModel> helper)
        {
            return JsonKnockoutValidationFor<TModel>();
        }
        public static MvcHtmlString ModelStateToDictionary(this ModelStateDictionary state)
        {
            var collection = new Dictionary<string, string>();
            foreach (var entry in state)
            {
                foreach (var error in entry.Value.Errors) {
                    if (!collection.ContainsKey(entry.Key))
                        collection.Add(entry.Key, error.ErrorMessage);
                    else
                        collection[entry.Key] = string.Join(";", collection[entry.Key], error.ErrorMessage);
                }
            }

            return ToJson(collection);
        }
        public static IDictionary<string,string> ToDictionary(this ModelStateDictionary state)
        {
            var collection = new Dictionary<string, string>();
            foreach (var entry in state)
            {
                foreach (var error in entry.Value.Errors)
                {
                    if (!collection.ContainsKey(entry.Key))
                        collection.Add(entry.Key, error.ErrorMessage);
                    else
                        collection[entry.Key] = string.Join(";", collection[entry.Key], error.ErrorMessage);
                }
            }

            return collection;
        }
    }
}