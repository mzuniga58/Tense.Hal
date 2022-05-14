using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Tense.Hal
{
    /// <summary>
    /// Hateoas Result Filter
    /// </summary>
    public class HalFilter : IResultFilter
    {
        /// <summary>
        /// Instantiates the filter
        /// </summary>
        public HalFilter()
        {
        }

        /// <summary>
        /// OnResultExecuting
        /// </summary>
        /// <param name="context"></param>
        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is not ObjectResult objectResult) return;

            if (objectResult.StatusCode != null && objectResult.StatusCode.Value >= 300)
                return;

            var style = GetOutputStyle(context);

            if (style.Equals("hal", StringComparison.OrdinalIgnoreCase))
            {
                IHalConfiguration halConfiguration = context.HttpContext.RequestServices.GetService<IHalConfiguration>();
                objectResult.Value = GenerateHateoasResponse(objectResult.Value, halConfiguration, context);
            }
        }

        /// <summary>
        /// Generates the Hateoas response
        /// </summary>
        /// <param name="value">The value of the object</param>
        /// <param name="halConfiguration">The <see cref="IHalConfiguration"/> interfae</param>
        /// <param name="context">The context</param>
        /// <returns></returns>
        public object? GenerateHateoasResponse(object? value, IHalConfiguration halConfiguration, ResultExecutingContext context)
        {
            if (value == null)
            {
                return null;
            }

            IDictionary<string, object?> result = AddPropertiesAndEmbedded(value, halConfiguration, context);
            IDictionary<string, object?>? links = AddResourceLinks(value, halConfiguration, context);

            if (links is not null)
            {
                result["_links"] = links;
            }

            return result;
        }

        private IDictionary<string, object?> AddPropertiesAndEmbedded(object obj, IHalConfiguration halConfiguration, ResultExecutingContext context)
        {
            IDictionary<string, object?> properties = new ExpandoObject();
            IDictionary<string, object?> embedded = new ExpandoObject();

            if (obj.GetType().IsGenericType)
            {
                var genericType = obj.GetType().GetGenericTypeDefinition();
                var genArgs = obj.GetType().GetGenericArguments();
                var typedVariant = genericType.MakeGenericType(genArgs);
                FieldInfo[] thisFieldInfo = typedVariant.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var field in thisFieldInfo)
                {
                    var propertyValue = field.GetValue(obj);
                    var propertyName = $"{field.Name[..1].ToLower()}{field.Name[1..]}";

                    if (propertyValue is null)
                    {
                        continue;
                    }

                    if (!halConfiguration.Links.HasLinks(propertyValue.GetType()) && propertyValue is not IEnumerable<object>)
                    {
                        properties.Add(propertyName, propertyValue);
                        continue;
                    }

                    var content = propertyValue is IEnumerable<object> list
                        ? list.ToList().Select(x => GenerateHateoasResponse(x, halConfiguration, context)).ToList()
                        : GenerateHateoasResponse(propertyValue, halConfiguration, context);

                    embedded.Add(propertyName, content);
                }

                if (embedded.Count > 0)
                {
                    properties["_embedded"] = embedded;
                }
            }
            if (obj.GetType() == typeof(ExpandoObject))
            {
                foreach (var property in (IDictionary<string, Object>)obj)
                {
                    var propertyValue = property.Value;
                    var propertyName = $"{property.Key[..1].ToLower()}{property.Key[1..]}";

                    if (propertyValue is null)
                    {
                        continue;
                    }

                    if (!halConfiguration.Links.HasLinks(propertyValue.GetType()) && propertyValue is not IEnumerable<object>)
                    {
                        properties.Add(propertyName, propertyValue);
                        continue;
                    }

                    var content = propertyValue is IEnumerable<object> list
                        ? list.ToList().Select(x => GenerateHateoasResponse(x, halConfiguration, context)).ToList()
                        : GenerateHateoasResponse(propertyValue, halConfiguration, context);

                    embedded.Add(propertyName, content);
                }

                if (embedded.Count > 0)
                {
                    properties["_embedded"] = embedded;
                }
            }
            else
            {
                foreach (PropertyInfo property in obj.GetType().GetProperties())
                {
                    var propertyValue = property.GetValue(obj);
                    var propertyName = $"{property.Name[..1].ToLower()}{property.Name[1..]}";
                    var propertyType = property.PropertyType;

                    if (propertyValue is null)
                    {
                        continue;
                    }

                    if (!halConfiguration.Links.HasLinks(propertyType) && propertyValue is not IEnumerable<object>)
                    {
                        properties.Add(propertyName, propertyValue);
                        continue;
                    }

                    var content = propertyValue is IEnumerable<object> list
                        ? list.ToList().Select(x => GenerateHateoasResponse(x, halConfiguration, context)).ToList()
                        : GenerateHateoasResponse(propertyValue, halConfiguration, context);

                    embedded.Add(propertyName, content);
                }

                if (embedded.Count > 0)
                {
                    properties["_embedded"] = embedded;
                }
            }

            return properties;
        }

        private IDictionary<string, object?>? AddResourceLinks(object obj, IHalConfiguration halConfiguration, ActionContext context)
        {
            IEnumerable<ILinkOptions>? linkOptions;

            if (obj.GetType().IsGenericType)
            {
                var genericType = obj.GetType().GetGenericTypeDefinition();
                var genArgs = obj.GetType().GetGenericArguments();
                var typedVariant = genericType.MakeGenericType(genArgs);

                linkOptions = halConfiguration.Links.GetLinks(typedVariant); // getting all configured links for the specified type
            }
            else
            {
                linkOptions = halConfiguration.Links.GetLinks(obj.GetType()); // getting all configured links for the specified type
            }

            if (linkOptions != null)
            {
                var result = new Dictionary<string, object?>();
                var createableOptions = linkOptions.Where(x => x.CanCreateLink(obj));

                foreach (var option in createableOptions)
                {
                    var key = option.Relation;

                    if (option.IsTemplate)
                    {
                        if (option.Relation.Equals("Curies", StringComparison.OrdinalIgnoreCase))
                        {
                            var value = new
                            {
                                href = GetUriByRouteValues(context.HttpContext, option.Relation, option.GetLinkTemplate(obj)),
                                name = option.Name,
                                templated = true
                            } as object;

                            result.Add(key, value);
                        }
                        else
                        {
                            var value = new
                            {
                                href = GetUriByRouteValues(context.HttpContext, option.Relation, option.GetLinkTemplate(obj)),
                                templated = true
                            } as object;

                            result.Add(key, value);
                        }
                    }
                    else
                    {
                        string url;

                        if (obj.GetType().IsGenericType)
                            url = GetPagedUriByRouteValues(context.HttpContext, option.GetRouteValues(obj));
                        else
                            url = GetUriByRouteValues(context.HttpContext, option.Relation, option.GetRouteValues(obj));

                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            var value = new
                            {
                                href = url
                            } as object;

                            result.Add(key, value);
                        }
                    }
                }

                return result;
            }

            return null;
        }

        private string GetOutputStyle(ResultExecutingContext context)
        {
            string style = string.Empty;
            string version = string.Empty;
            string media = string.Empty;

            var acceptHeader = context.HttpContext.Request.Headers.FirstOrDefault(h => h.Key.Equals("Accept", StringComparison.OrdinalIgnoreCase));

            if (acceptHeader.Key != null)
            {
                foreach (var headerValue in acceptHeader.Value)
                {
                    var match = Regex.Match(headerValue, "application\\/(?<style>[a-z-A-Z0-9]+)\\.v(?<version>[0-9]+)\\+(?<media>.*)", RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        style = match.Groups["style"].Value;
                        version = match.Groups["version"].Value;
                        media = match.Groups["media"].Value;
                        break;
                    }

                    if (!string.IsNullOrWhiteSpace(style) &&
                         !string.IsNullOrWhiteSpace(version) &&
                         !string.IsNullOrWhiteSpace(media))
                    {
                        context.HttpContext.Response.ContentType = $"application/{style}.v{version}+{media}";
                    }

                    else if (!string.IsNullOrWhiteSpace(style) &&
                         !string.IsNullOrWhiteSpace(media))
                    {
                        context.HttpContext.Response.ContentType = $"application/{style}+{media}";
                    }
                }
            }

            return style;
        }

        /// <summary>
        /// Returns the link
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/></param>
        /// <param name="relation">The relation for the link</param>
        /// <param name="routeValues">The route values</param>
        /// <returns></returns>
        public string GetUriByRouteValues(HttpContext context, string relation, object routeValues)
        {
            if (relation.IndexOf(':') == -1)
            {
                return $"{context.Request.Scheme}://{context.Request.Host}{routeValues}";
            }
            else
                return $"{routeValues}";
        }

        /// <summary>
        /// Returns links for a pagedSet
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/></param>
        /// <param name="routeValues">The route values</param>
        /// <returns></returns>
        public string GetPagedUriByRouteValues(HttpContext context, object routeValues)
        {
            if (!string.IsNullOrWhiteSpace(context.Request.QueryString.ToString()))
            {
                var queryString = WebUtility.UrlDecode(context.Request.QueryString.ToString())[1..];
                var match = Regex.Match(queryString, "(?<limitclause>limit\\([0-9]+(\\,[0-9]+){0,1}\\))");

                if (match.Success)
                {
                    int start = match.Groups["limitclause"].Index;
                    int length = match.Groups["limitclause"].Length;

                    var startOfString = queryString[..start];
                    var endOfString = queryString[(start + length)..];

                    if (string.IsNullOrWhiteSpace(startOfString))
                    {
                        if (string.IsNullOrWhiteSpace(endOfString))
                        {
                            queryString = string.Empty;
                        }
                        else
                        {
                            if (endOfString.StartsWith("&") || endOfString.StartsWith("|"))
                                endOfString = endOfString[1..];

                            queryString = endOfString;
                        }
                    }
                    else if (string.IsNullOrWhiteSpace(endOfString))
                    {
                        if (startOfString.EndsWith("&") || startOfString.EndsWith("|"))   
                            startOfString = startOfString[0..^1];

                        queryString = startOfString;
                    }
                    else
                    {
                        if (startOfString.EndsWith("&") && endOfString.StartsWith("&"))
                            endOfString = endOfString[1..];

                        else if (startOfString.EndsWith("&") && endOfString.StartsWith("|"))
                            startOfString = startOfString[0..^1];

                        else if (startOfString.EndsWith("|") && endOfString.StartsWith("|"))
                            endOfString = endOfString[1..];

                        else if (startOfString.EndsWith("|") && endOfString.StartsWith("&"))
                            startOfString = startOfString[0..^1];

                        queryString = $"{startOfString}{endOfString}";
                    }
                }

                if (string.IsNullOrWhiteSpace(queryString))
                    return $"{context.Request.Scheme}://{context.Request.Host}{routeValues}";
                else
                    return $"{context.Request.Scheme}://{context.Request.Host}{routeValues}&{queryString}";
            }
            else
                return $"{context.Request.Scheme}://{context.Request.Host}{routeValues}";
        }


        /// <summary>
        /// OnResultExecuted
        /// </summary>
        /// <param name="context"></param>
        public void OnResultExecuted(ResultExecutedContext context)
        {
        }
    }
}
