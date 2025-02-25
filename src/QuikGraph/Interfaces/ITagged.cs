using System;
using JetBrains.Annotations;

namespace QuikGraph
{
    /// <summary>
    /// Represents an object that is able to be tagged.
    /// </summary>>
    public interface ITagged<TTag>
    {
        /// <summary>
        /// Fired when the tag is changed.
        /// </summary>
        event EventHandler TagChanged;

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        [CanBeNull]
        TTag Tag { get; set; }
    }
}