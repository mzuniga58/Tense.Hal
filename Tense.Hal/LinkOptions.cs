using System;

namespace Tense.Hal
{
    /// <summary>
    /// Link Options 
    /// </summary>
    /// <typeparam name="T">The type of object the links are for.</typeparam>
    public class LinkOptions<T> : ILinkOptions
    {
        private readonly Func<T, object> _getRouteValues;
        private readonly Func<T, string, string> _templateLinkGenerator;
        private readonly Func<T, bool> _canCreateLink;
        private readonly string _template;

        /// <summary>
        /// Instantiates a LinkOptions object using a template link
        /// </summary>
        /// <param name="relation">The relation of the link</param>
        /// <param name="routeName">The route name to the endpoint in the controller</param>
        /// <param name="name">The name of the link</param>
        /// <param name="template">The template for the link</param>
        /// <param name="templateGenerator">A function that generates the link from the template</param>
        /// <param name="canCreate">A function that returns <see langword="true"/> if the link can be created; <see langword="false"/> otherwise.</param>
        public LinkOptions(string relation, string routeName, string name, string template, Func<T, string, string> templateGenerator, Func<T, bool> canCreate)
        {
            _getRouteValues = (o) => { return string.Empty; };
            _templateLinkGenerator = templateGenerator;
            _canCreateLink = canCreate;
            _template = template;
            Relation = relation;
            RouteName = routeName;
            Name = name;
        }

        /// <summary>
        /// Instantiates a LinkOptions objet using a simple link
        /// </summary>
        /// <param name="relation">The relation of the link</param>
        /// <param name="routeName">The route name to the endpoint in the controller</param>
        /// <param name="routeValues">The route values</param>
        /// <param name="canCreate">A function that returns <see langword="true"/> if the link can be created; <see langword="false"/> otherwise.</param>
        public LinkOptions(string relation, string routeName, Func<T, object> routeValues, Func<T, bool> canCreate)
        {
            _getRouteValues = routeValues;
            _templateLinkGenerator = (o, s) => { return s; };
            _canCreateLink = canCreate;
            _template = string.Empty;
            Relation = relation;
            RouteName = routeName;
        }

        /// <summary>
        /// Instantiates a LinkOptions objet using a full link
        /// </summary>
        /// <param name="relation">The relation of the link</param>
        /// <param name="routeName">The route name to the endpoint in the controller</param>
        /// <param name="name">The name of the link</param>
        /// <param name="template">The template used for templated links.</param>
        /// <param name="routeValues">The route values</param>
        /// <param name="templateGenerator">A function that generates the link from the template</param>
        /// <param name="canCreate">A function that returns <see langword="true"/> if the link can be created; <see langword="false"/> otherwise.</param>
        public LinkOptions(string relation, string routeName, string name, string template, Func<T, object> routeValues, Func<T, string, string> templateGenerator, Func<T, bool> canCreate)
        {
            Relation = relation;
            RouteName = routeName;
            Name = name;
            _getRouteValues = routeValues;
            _templateLinkGenerator = templateGenerator;
            _canCreateLink = canCreate;
            _template = template;
        }

        /// <summary>
        /// Returns the relation
        /// </summary>
        public string Relation { get; } = string.Empty;

        /// <summary>
        /// Returns the route name
        /// </summary>
        public string RouteName { get; } = string.Empty;

        /// <summary>
        /// Returns the route name
        /// </summary>
        public string Name { get; } = string.Empty;

        /// <summary>
        /// Returns <see langword="true"/> if this is a template; <see langword="false"/> otherwise.
        /// </summary>
        public bool IsTemplate => !string.IsNullOrEmpty(_template);

        /// <summary>
        /// Returns the route values
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public object GetRouteValues(object obj)
        {
            return _getRouteValues((T)obj);
        }

        /// <summary>
        /// Returns the link template
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string GetLinkTemplate(object obj)
        {
            return _templateLinkGenerator((T)obj, _template);
        }

        /// <summary>
        /// Returns <see langword="true"/> if this can create a link; <see langword="false"/> otherwise.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool CanCreateLink(object obj)
        {
            return _canCreateLink((T)obj);
        }
    }
}
