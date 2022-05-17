using System;
using System.Collections.Generic;

namespace Tense.Hal
{
    /// <summary>
    /// The interface that is used to define the HAL configuration
    /// </summary>
    public interface IHalConfiguration
    {
        /// <summary>
        /// A list of links 
        /// </summary>
        TypeLinks Links { get; }

        /// <summary>
        /// Gets the configuration
        /// </summary>
        Dictionary<Type, Dictionary<string, ILinkOptions>> Configuration { get; }
    }
}
