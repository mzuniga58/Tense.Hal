using System;
using System.Collections.Generic;

namespace Tense.Hal
{
    /// <summary>
    /// Type links
    /// </summary>
    public class TypeLinks
    {
        private readonly Dictionary<Type, Dictionary<string, ILinkOptions>> _links;
        internal IEnumerable<ILinkOptions>? GetLinks(Type type) => _links.ContainsKey(type) ? _links[type].Values : null;
        internal bool HasLinks(Type type) => _links.ContainsKey(type);

        /// <summary>
        /// Instantiates a TypeLinks object
        /// </summary>
        public TypeLinks()
        {
            _links = new Dictionary<Type, Dictionary<string, ILinkOptions>>();
        }

        /// <summary>
        /// Adds a templated link to the list of links
        /// </summary>
        /// <typeparam name="T">The type of objet the link refers to.</typeparam>
        /// <param name="relation">The relation name of the link, i.e., "self"</param>
        /// <param name="routeName">The name of the controller and endpoint function the link refers to, i.e., nameof(controller.function)</param>
        /// <param name="name">The name of the link</param>
        /// <param name="template">The template for the link that contains replaceable arguments, i.e., /route/{rel}</param>
        /// <param name="templateGenerator">A function that generates the link using the template</param>
        /// <param name="canCreate">A function that returns <see langword="true"/> if the link can be created; <see langword="false"/> otherwise.</param>
        public void AddLinkTemplate<T>(string relation, string routeName, string name, string template, Func<T, string, string> templateGenerator, Func<T, bool> canCreate)
        {
            var type = typeof(T);

            if (!_links.ContainsKey(type))
            {
                _links[type] = new Dictionary<string, ILinkOptions>();
            }

            _links[type][relation] = new LinkOptions<T>(relation, routeName, name, template, templateGenerator, canCreate);
        }

        /// <summary>
        /// Adds a link to the list of links
        /// </summary>
        /// <typeparam name="T">The type of objet the link refers to.</typeparam>
        /// <param name="relation">The relation name of the link, i.e., "self"</param>
        /// <param name="routeName">The name of the controller and endpoint function the link refers to, i.e., nameof(controller.function)</param>
        /// <param name="routeValues">A function that returns the link based upon values in teh object, i.e., (o,l) => $"/route/{o.id}"</param>
        /// <param name="canCreate">A function that returns <see langword="true"/> if the link can be created; <see langword="false"/> otherwise.</param>
        public void AddLink<T>(string relation, string routeName, Func<T, object> routeValues, Func<T, bool> canCreate)
        {
            var type = typeof(T);
            if (!_links.ContainsKey(type))
            {
                _links[type] = new Dictionary<string, ILinkOptions>();
            }

            _links[type][relation] = new LinkOptions<T>(relation, routeName, routeValues, canCreate);
        }
    }
}
