namespace Tense.Hal
{
    /// <summary>
    /// ILinkOptions Interface
    /// </summary>
    public interface ILinkOptions
    {
        /// <summary>
        /// Relation
        /// </summary>
        string Relation { get; }

        /// <summary>
        /// RouteName
        /// </summary>
        string RouteName { get; }

        /// <summary>
        /// Name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// IsTemplate
        /// </summary>
        bool IsTemplate { get; }

        /// <summary>
        /// GetRouteValues
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        object GetRouteValues(object obj);

        /// <summary>
        /// GetLinkTemplates
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string GetLinkTemplate(object obj);

        /// <summary>
        /// CanCreateLink
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool CanCreateLink(object obj);
    }
}
