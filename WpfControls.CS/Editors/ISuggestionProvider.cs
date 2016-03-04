

namespace WpfControls.Editors
{
    using System.Collections;

    /// <summary>
    /// Represents the suggestion provider.
    /// </summary>
    public interface ISuggestionProvider
    {

        #region Public Methods

        /// <summary>
        /// Gets the suggestions.
        /// </summary>
        /// <param name="filter">The filter to apply on the suggestion list.</param>
        /// <returns></returns>
        IEnumerable GetSuggestions(string filter);

        #endregion Public Methods

    }
}
