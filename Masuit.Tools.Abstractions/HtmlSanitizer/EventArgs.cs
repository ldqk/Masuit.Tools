using AngleSharp.Css.Dom;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Ganss.Xss;

/// <summary>
/// Provides data for the <see cref="HtmlSanitizer.PostProcessDom"/> event.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PostProcessDomEventArgs"/> class.
/// </remarks>
public class PostProcessDomEventArgs(IHtmlDocument document) : EventArgs
{
    /// <summary>
    /// Gets the document.
    /// </summary>
    /// <value>
    /// The document.
    /// </value>
    public IHtmlDocument Document { get; private set; } = document;
}

/// <summary>
/// Provides data for the <see cref="HtmlSanitizer.PostProcessNode"/> event.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PostProcessNodeEventArgs"/> class.
/// </remarks>
public class PostProcessNodeEventArgs(IHtmlDocument document, INode node) : EventArgs
{
    /// <summary>
    /// Gets the document.
    /// </summary>
    /// <value>
    /// The document.
    /// </value>
    public IHtmlDocument Document { get; private set; } = document;

    /// <summary>
    /// Gets the DOM node to be processed.
    /// </summary>
    /// <value>
    /// The DOM node.
    /// </value>
    public INode Node { get; private set; } = node;

    /// <summary>
    /// Gets the replacement nodes. Leave empty if no replacement should occur.
    /// </summary>
    /// <value>
    /// The replacement nodes.
    /// </value>
    public ICollection<INode> ReplacementNodes { get; private set; } = [];
}

/// <summary>
/// Provides data for the <see cref="HtmlSanitizer.RemovingTag"/> event.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RemovingTagEventArgs"/> class.
/// </remarks>
/// <param name="tag">The element to be removed.</param>
/// <param name="reason">The reason why the tag will be removed.</param>
public class RemovingTagEventArgs(IElement tag, RemoveReason reason) : CancelEventArgs
{
    /// <summary>
    /// Gets the tag to be removed.
    /// </summary>
    /// <value>
    /// The tag.
    /// </value>
    public IElement Tag { get; private set; } = tag;

    /// <summary>
    /// Gets the reason why the tag will be removed.
    /// </summary>
    /// <value>
    /// The reason.
    /// </value>
    public RemoveReason Reason { get; private set; } = reason;
}

/// <summary>
/// Provides data for the <see cref="HtmlSanitizer.RemovingAttribute"/> event.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RemovingAttributeEventArgs"/> class.
/// </remarks>
/// <param name="tag">The element containing the attribute.</param>
/// <param name="attribute">The attribute to be removed.</param>
/// <param name="reason">The reason why the attribute will be removed.</param>
public class RemovingAttributeEventArgs(IElement tag, IAttr attribute, RemoveReason reason) : CancelEventArgs
{
    /// <summary>
    /// Gets the tag containing the attribute to be removed.
    /// </summary>
    /// <value>
    /// The tag.
    /// </value>
    public IElement Tag { get; private set; } = tag;

    /// <summary>
    /// Gets the attribute to be removed.
    /// </summary>
    /// <value>
    /// The attribute.
    /// </value>
    public IAttr Attribute { get; private set; } = attribute;

    /// <summary>
    /// Gets the reason why the attribute will be removed.
    /// </summary>
    /// <value>
    /// The reason.
    /// </value>
    public RemoveReason Reason { get; private set; } = reason;
}

/// <summary>
/// Provides data for the <see cref="HtmlSanitizer.RemovingStyle"/> event.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RemovingStyleEventArgs"/> class.
/// </remarks>
/// <param name="tag">The element containing the attribute.</param>
/// <param name="style">The style to be removed.</param>
/// <param name="reason">The reason why the attribute will be removed.</param>
public class RemovingStyleEventArgs(IElement tag, ICssProperty style, RemoveReason reason) : CancelEventArgs
{
    /// <summary>
    /// Gets the tag containing the style to be removed.
    /// </summary>
    /// <value>
    /// The tag.
    /// </value>
    public IElement Tag { get; private set; } = tag;

    /// <summary>
    /// Gets the style to be removed.
    /// </summary>
    /// <value>
    /// The style.
    /// </value>
    public ICssProperty Style { get; private set; } = style;

    /// <summary>
    /// Gets the reason why the style will be removed.
    /// </summary>
    /// <value>
    /// The reason.
    /// </value>
    public RemoveReason Reason { get; private set; } = reason;
}

/// <summary>
/// Provides data for the <see cref="HtmlSanitizer.RemovingAtRule"/> event.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RemovingAtRuleEventArgs"/> class.
/// </remarks>
/// <param name="tag">The element containing the attribute.</param>
/// <param name="rule">The rule to be removed.</param>
public class RemovingAtRuleEventArgs(IElement tag, ICssRule rule) : CancelEventArgs
{
    /// <summary>
    /// Gets the tag containing the at-rule to be removed.
    /// </summary>
    /// <value>
    /// The tag.
    /// </value>
    public IElement Tag { get; private set; } = tag;

    /// <summary>
    /// Gets the rule to be removed.
    /// </summary>
    /// <value>
    /// The rule.
    /// </value>
    public ICssRule Rule { get; private set; } = rule;
}

/// <summary>
/// Provides data for the <see cref="HtmlSanitizer.RemovingComment"/> event.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RemovingCommentEventArgs"/> class.
/// </remarks>
/// <param name="comment">The comment to be removed.</param>
public class RemovingCommentEventArgs(IComment comment) : CancelEventArgs
{
    /// <summary>
    /// Gets the comment node to be removed.
    /// </summary>
    /// <value>
    /// The comment node.
    /// </value>
    public IComment Comment { get; private set; } = comment;
}

/// <summary>
/// Provides data for the <see cref="HtmlSanitizer.RemovingCssClass"/> event.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RemovingCssClassEventArgs"/> class.
/// </remarks>
/// <param name="tag">The element containing the attribute.</param>
/// <param name="cssClass">The CSS class to be removed.</param>
/// <param name="reason">The reason why the attribute will be removed.</param>
public class RemovingCssClassEventArgs(IElement tag, string cssClass, RemoveReason reason) : CancelEventArgs
{
    /// <summary>
    /// Gets the tag containing the CSS class to be removed.
    /// </summary>
    /// <value>
    /// The tag.
    /// </value>
    public IElement Tag { get; private set; } = tag;

    /// <summary>
    /// Gets the CSS class to be removed.
    /// </summary>
    /// <value>
    /// The CSS class.
    /// </value>
    public string CssClass { get; private set; } = cssClass;

    /// <summary>
    /// Gets the reason why the CSS class will be removed.
    /// </summary>
    /// <value>
    /// The reason.
    /// </value>
    public RemoveReason Reason { get; private set; } = reason;
}

/// <summary>
/// Provides data for the <see cref="HtmlSanitizer.FilterUrl"/> event.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FilterUrlEventArgs"/> class.
/// </remarks>
/// <param name="tag">The tag containing the URI being sanitized.</param>
/// <param name="originalUrl">The original URL.</param>
/// <param name="sanitizedUrl">The sanitized URL.</param>
public class FilterUrlEventArgs(IElement tag, string originalUrl, string? sanitizedUrl = null) : EventArgs
{
    /// <summary>
    /// Gets the original URL.
    /// </summary>
    /// <value>
    /// The original URL.
    /// </value>
    public string OriginalUrl { get; private set; } = originalUrl;

    /// <summary>
    /// Gets or sets the sanitized URL.
    /// </summary>
    /// <value>
    /// The sanitized URL. If it is null, it will be removed.
    /// </value>
    public string? SanitizedUrl { get; set; } = sanitizedUrl;

    /// <summary>
    /// Gets the tag containing the URI being sanitized.
    /// </summary>
    /// <value>
    /// The tag.
    /// </value>
    public IElement Tag { get; private set; } = tag;
}
