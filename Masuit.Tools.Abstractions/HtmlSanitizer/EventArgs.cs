using AngleSharp.Css.Dom;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Ganss.Xss
{
    /// <summary>
    /// Provides data for the <see cref="HtmlSanitizer.PostProcessDom"/> event.
    /// </summary>
    public class PostProcessDomEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the document.
        /// </summary>
        /// <value>
        /// The document.
        /// </value>
        public IHtmlDocument Document { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostProcessDomEventArgs"/> class.
        /// </summary>
        public PostProcessDomEventArgs(IHtmlDocument document)
        {
            Document = document;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="HtmlSanitizer.PostProcessNode"/> event.
    /// </summary>
    public class PostProcessNodeEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the document.
        /// </summary>
        /// <value>
        /// The document.
        /// </value>
        public IHtmlDocument Document { get; private set; }

        /// <summary>
        /// Gets the DOM node to be processed.
        /// </summary>
        /// <value>
        /// The DOM node.
        /// </value>
        public INode Node { get; private set; }

        /// <summary>
        /// Gets the replacement nodes. Leave empty if no replacement should occur.
        /// </summary>
        /// <value>
        /// The replacement nodes.
        /// </value>
        public ICollection<INode> ReplacementNodes { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostProcessNodeEventArgs"/> class.
        /// </summary>
        public PostProcessNodeEventArgs(IHtmlDocument document, INode node)
        {
            Document = document;
            Node = node;
            ReplacementNodes = new List<INode>();
        }
    }

    /// <summary>
    /// Provides data for the <see cref="HtmlSanitizer.RemovingTag"/> event.
    /// </summary>
    public class RemovingTagEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Gets the tag to be removed.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public IElement Tag { get; private set; }

        /// <summary>
        /// Gets the reason why the tag will be removed.
        /// </summary>
        /// <value>
        /// The reason.
        /// </value>
        public RemoveReason Reason { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemovingTagEventArgs"/> class.
        /// </summary>
        /// <param name="tag">The element to be removed.</param>
        /// <param name="reason">The reason why the tag will be removed.</param>
        public RemovingTagEventArgs(IElement tag, RemoveReason reason)
        {
            Tag = tag;
            Reason = reason;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="HtmlSanitizer.RemovingAttribute"/> event.
    /// </summary>
    public class RemovingAttributeEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Gets the tag containing the attribute to be removed.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public IElement Tag { get; private set; }

        /// <summary>
        /// Gets the attribute to be removed.
        /// </summary>
        /// <value>
        /// The attribute.
        /// </value>
        public IAttr Attribute { get; private set; }

        /// <summary>
        /// Gets the reason why the attribute will be removed.
        /// </summary>
        /// <value>
        /// The reason.
        /// </value>
        public RemoveReason Reason { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemovingAttributeEventArgs"/> class.
        /// </summary>
        /// <param name="tag">The element containing the attribute.</param>
        /// <param name="attribute">The attribute to be removed.</param>
        /// <param name="reason">The reason why the attribute will be removed.</param>
        public RemovingAttributeEventArgs(IElement tag, IAttr attribute, RemoveReason reason)
        {
            Tag = tag;
            Attribute = attribute;
            Reason = reason;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="HtmlSanitizer.RemovingStyle"/> event.
    /// </summary>
    public class RemovingStyleEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Gets the tag containing the style to be removed.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public IElement Tag { get; private set; }

        /// <summary>
        /// Gets the style to be removed.
        /// </summary>
        /// <value>
        /// The style.
        /// </value>
        public ICssProperty Style { get; private set; }

        /// <summary>
        /// Gets the reason why the style will be removed.
        /// </summary>
        /// <value>
        /// The reason.
        /// </value>
        public RemoveReason Reason { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemovingStyleEventArgs"/> class.
        /// </summary>
        /// <param name="tag">The element containing the attribute.</param>
        /// <param name="style">The style to be removed.</param>
        /// <param name="reason">The reason why the attribute will be removed.</param>
        public RemovingStyleEventArgs(IElement tag, ICssProperty style, RemoveReason reason)
        {
            Tag = tag;
            Style = style;
            Reason = reason;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="HtmlSanitizer.RemovingAtRule"/> event.
    /// </summary>
    public class RemovingAtRuleEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Gets the tag containing the at-rule to be removed.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public IElement Tag { get; private set; }

        /// <summary>
        /// Gets the rule to be removed.
        /// </summary>
        /// <value>
        /// The rule.
        /// </value>
        public ICssRule Rule { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemovingAtRuleEventArgs"/> class.
        /// </summary>
        /// <param name="tag">The element containing the attribute.</param>
        /// <param name="rule">The rule to be removed.</param>
        public RemovingAtRuleEventArgs(IElement tag, ICssRule rule)
        {
            Tag = tag;
            Rule = rule;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="HtmlSanitizer.RemovingComment"/> event.
    /// </summary>
    public class RemovingCommentEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Gets the comment node to be removed.
        /// </summary>
        /// <value>
        /// The comment node.
        /// </value>
        public IComment Comment { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemovingCommentEventArgs"/> class.
        /// </summary>
        /// <param name="comment">The comment to be removed.</param>
        public RemovingCommentEventArgs(IComment comment)
        {
            Comment = comment;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="HtmlSanitizer.RemovingCssClass"/> event.
    /// </summary>
    public class RemovingCssClassEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Gets the tag containing the CSS class to be removed.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public IElement Tag { get; private set; }

        /// <summary>
        /// Gets the CSS class to be removed.
        /// </summary>
        /// <value>
        /// The CSS class.
        /// </value>
        public string CssClass { get; private set; }

        /// <summary>
        /// Gets the reason why the CSS class will be removed.
        /// </summary>
        /// <value>
        /// The reason.
        /// </value>
        public RemoveReason Reason { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemovingCssClassEventArgs"/> class.
        /// </summary>
        /// <param name="tag">The element containing the attribute.</param>
        /// <param name="cssClass">The CSS class to be removed.</param>
        /// <param name="reason">The reason why the attribute will be removed.</param>
        public RemovingCssClassEventArgs(IElement tag, string cssClass, RemoveReason reason)
        {
            Tag = tag;
            CssClass = cssClass;
            Reason = reason;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="HtmlSanitizer.FilterUrl"/> event.
    /// </summary>
    public class FilterUrlEventArgs: EventArgs
    {
        /// <summary>
        /// Gets the original URL.
        /// </summary>
        /// <value>
        /// The original URL.
        /// </value>
        public string OriginalUrl { get; private set; }

        /// <summary>
        /// Gets or sets the sanitized URL.
        /// </summary>
        /// <value>
        /// The sanitized URL. If it is null, it will be removed.
        /// </value>
        public string? SanitizedUrl { get; set; }

        /// <summary>
        /// Gets the tag containing the URI being sanitized.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public IElement Tag { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterUrlEventArgs"/> class.
        /// </summary>
        /// <param name="tag">The tag containing the URI being sanitized.</param>
        /// <param name="originalUrl">The original URL.</param>
        /// <param name="sanitizedUrl">The sanitized URL.</param>
        public FilterUrlEventArgs(IElement tag, string originalUrl, string? sanitizedUrl = null)
        {
            OriginalUrl = originalUrl;
            SanitizedUrl = sanitizedUrl;
            Tag = tag;
        }
    }
}
