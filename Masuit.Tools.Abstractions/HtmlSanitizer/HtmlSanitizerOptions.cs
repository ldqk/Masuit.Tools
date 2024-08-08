using AngleSharp.Css.Dom;
using System;
using System.Collections.Generic;

namespace Ganss.Xss
{
    /// <summary>
    /// Provides options to be used with <see cref="HtmlSanitizer"/>.
    /// </summary>
    public class HtmlSanitizerOptions
    {
        /// <summary>
        /// Gets or sets the allowed tag names such as "a" and "div".
        /// </summary>
        public ISet<string> AllowedTags { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets the allowed HTML attributes such as "href" and "alt".
        /// </summary>
        public ISet<string> AllowedAttributes { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets the allowed CSS classes.
        /// </summary>
        public ISet<string> AllowedCssClasses { get; set; } = new HashSet<string>();

        /// <summary>
        /// Gets or sets the allowed CSS properties such as "font" and "margin".
        /// </summary>
        public ISet<string> AllowedCssProperties { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets the allowed CSS at-rules such as "@media" and "@font-face".
        /// </summary>
        public ISet<CssRuleType> AllowedAtRules { get; set; } = new HashSet<CssRuleType>();

        /// <summary>
        /// Gets or sets the allowed URI schemes such as "http" and "https".
        /// </summary>
        public ISet<string> AllowedSchemes { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets the HTML attributes that can contain a URI such as "href".
        /// </summary>
        public ISet<string> UriAttributes { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Allow all custom CSS properties (variables) prefixed with <c>--</c>.
        /// </summary>
        public bool AllowCssCustomProperties { get; set; }

        /// <summary>
        /// Allow all HTML5 data attributes; the attributes prefixed with <c>data-</c>.
        /// </summary>
        public bool AllowDataAttributes { get; set; }
    }
}
