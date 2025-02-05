using AngleSharp;
using AngleSharp.Css.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Ganss.Xss;

/// <summary>
/// Enables an inheriting class to implement an HtmlSanitizer class, which cleans HTML documents and fragments
/// from constructs that can lead to <a href="https://en.wikipedia.org/wiki/Cross-site_scripting">XSS attacks</a>.
/// </summary>
public interface IHtmlSanitizer
{
    /// <summary>
    /// Gets or sets a value indicating whether to keep child nodes of elements that are removed.
    /// </summary>
    bool KeepChildNodes { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Func{HtmlParser}"/> object the creates the parser used for parsing the input.
    /// </summary>
    Func<HtmlParser> HtmlParserFactory { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="IMarkupFormatter"/> object used for generating output.
    /// </summary>
    IMarkupFormatter OutputFormatter { get; set; }

    /// <summary>
    /// Gets the allowed CSS at-rules such as "@media" and "@font-face".
    /// </summary>
    /// <value>
    /// The allowed CSS at-rules.
    /// </value>
    ISet<CssRuleType> AllowedAtRules { get; }

    /// <summary>
    /// Gets the allowed URI schemes such as "http" and "https".
    /// </summary>
    /// <value>
    /// The allowed URI schemes.
    /// </value>
    ISet<string> AllowedSchemes { get; }

    /// <summary>
    /// Gets the allowed HTML tag names such as "a" and "div".
    /// </summary>
    /// <value>
    /// The allowed tag names.
    /// </value>
    ISet<string> AllowedTags { get; }

    /// <summary>
    /// Gets the allowed HTML attributes such as "href" and "alt".
    /// </summary>
    /// <value>
    /// The allowed HTML attributes.
    /// </value>
    ISet<string> AllowedAttributes { get; }

    /// <summary>
    /// Allow all HTML5 data attributes; the attributes prefixed with data-
    /// </summary>
    bool AllowDataAttributes { get; set; }

    /// <summary>
    /// Gets or sets the HTML attributes that can contain a URI such as "href".
    /// </summary>
    /// <value>
    /// The URI attributes.
    /// </value>
    ISet<string> UriAttributes { get; }

    /// <summary>
    /// Gets or sets the allowed CSS properties such as "font" and "margin".
    /// </summary>
    /// <value>
    /// The allowed CSS properties.
    /// </value>
    ISet<string> AllowedCssProperties { get; }

    /// <summary>
    /// Gets or sets a regex that must not match for legal CSS property values.
    /// </summary>
    /// <value>
    /// The regex.
    /// </value>
    Regex DisallowCssPropertyValue { get; set; }

    /// <summary>
    /// Gets or sets the allowed CSS classes. If the set is empty, all classes will be allowed.
    /// </summary>
    /// <value>
    /// The allowed CSS classes. An empty set means all classes are allowed.
    /// </value>
    ISet<string> AllowedClasses { get; }

    /// <summary>
    /// Occurs after sanitizing the document and post processing nodes.
    /// </summary>
    event EventHandler<PostProcessDomEventArgs> PostProcessDom;

    /// <summary>
    /// Occurs for every node after sanitizing.
    /// </summary>
    event EventHandler<PostProcessNodeEventArgs> PostProcessNode;

    /// <summary>
    /// Occurs before a tag is removed.
    /// </summary>
    event EventHandler<RemovingTagEventArgs> RemovingTag;

    /// <summary>
    /// Occurs before an attribute is removed.
    /// </summary>
    event EventHandler<RemovingAttributeEventArgs> RemovingAttribute;

    /// <summary>
    /// Occurs before a style is removed.
    /// </summary>
    event EventHandler<RemovingStyleEventArgs> RemovingStyle;

    /// <summary>
    /// Occurs before an at-rule is removed.
    /// </summary>
    event EventHandler<RemovingAtRuleEventArgs> RemovingAtRule;

    /// <summary>
    /// Occurs before a comment is removed.
    /// </summary>
    event EventHandler<RemovingCommentEventArgs> RemovingComment;

    /// <summary>
    /// Occurs before a CSS class is removed.
    /// </summary>
    event EventHandler<RemovingCssClassEventArgs> RemovingCssClass;

    /// <summary>
    /// Occurs when a URL is being sanitized.
    /// </summary>
    event EventHandler<FilterUrlEventArgs>? FilterUrl;

    /// <summary>
    /// Sanitizes the specified HTML.
    /// </summary>
    /// <param name="html">The HTML to sanitize.</param>
    /// <param name="baseUrl">The base URL relative URLs are resolved against. No resolution if empty.</param>
    /// <param name="outputFormatter">The formatter used to render the DOM. Using the default formatter if null.</param>
    /// <returns>The sanitized HTML.</returns>
    string Sanitize(string html, string baseUrl = "", IMarkupFormatter? outputFormatter = null);

    /// <summary>
    /// Sanitizes the specified HTML body fragment. If a document is given, only the body part will be returned.
    /// </summary>
    /// <param name="html">The HTML body fragment to sanitize.</param>
    /// <param name="baseUrl">The base URL relative URLs are resolved against. No resolution if empty.</param>
    /// <returns>The sanitized HTML document.</returns>
    IHtmlDocument SanitizeDom(string html, string baseUrl = "");

    /// <summary>
    /// Sanitizes the specified parsed HTML body fragment.
    /// If the document has not been parsed with CSS support then all styles will be removed.
    /// </summary>
    /// <param name="document">The parsed HTML document.</param>
    /// <param name="context">The node within which to sanitize.</param>
    /// <param name="baseUrl">The base URL relative URLs are resolved against. No resolution if empty.</param>
    /// <returns>The sanitized HTML document.</returns>
    IHtmlDocument SanitizeDom(IHtmlDocument document, IHtmlElement? context = null, string baseUrl = "");

    /// <summary>
    /// Sanitizes the specified HTML document. Even if only a fragment is given, a whole document will be returned.
    /// </summary>
    /// <param name="html">The HTML document to sanitize.</param>
    /// <param name="baseUrl">The base URL relative URLs are resolved against. No resolution if empty.</param>
    /// <param name="outputFormatter">The formatter used to render the DOM. Using the <see cref="OutputFormatter"/> if null.</param>
    /// <returns>The sanitized HTML document.</returns>
    string SanitizeDocument(string html, string baseUrl = "", IMarkupFormatter? outputFormatter = null);
}
