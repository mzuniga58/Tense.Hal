using System;
using System.Collections.Generic;

namespace Tense.Hal
{
    /// <summary>
    /// Base configuration
    /// </summary>
    public class BaseConfiguration : IHalConfiguration
    {
        /// <summary>
        /// The links defined for the configuration
        /// </summary>
        public TypeLinks Links { get; } = new TypeLinks();

        /// <summary>
        /// Returns the configuration
        /// </summary>
        public Dictionary<Type, Dictionary<string, ILinkOptions>> Configuration
        {
            get { return Links.Configuration; }
        }

        /// <summary>
        /// Register a child configuration
        /// </summary>
        /// <param name="configuration">The child configuration to register.</param>
        public void Register(IHalConfiguration configuration)
        {
            Links.Register(configuration);
        }
    }
}
