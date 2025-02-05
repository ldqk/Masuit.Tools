using AngleSharp.Html;
using AngleSharp.Dom;
using System;
using System.Text;

namespace Ganss.Xss;

/// <summary>
/// HTML5 markup formatter. Identical to <see cref="HtmlMarkupFormatter"/> except for &lt; and &gt; which are
/// encoded in attribute values.
/// </summary>
public class HtmlFormatter: HtmlMarkupFormatter
{
    /// <summary>
    /// An instance of <see cref="HtmlFormatter"/>.
    /// </summary>
    new public static readonly HtmlFormatter Instance = new ();

    // disable XML comments warnings
    #pragma warning disable 1591

    protected override string Attribute(IAttr attr)
    {
        var namespaceUri = attr.NamespaceUri;
        var localName = attr.LocalName;
        var value = attr.Value;
        var temp = new StringBuilder();

        if (String.IsNullOrEmpty(namespaceUri))
        {
            temp.Append(localName);
        }
        else if (namespaceUri == NamespaceNames.XmlUri)
        {
            temp.Append(NamespaceNames.XmlPrefix).Append(':').Append(localName);
        }
        else if (namespaceUri == NamespaceNames.XLinkUri)
        {
            temp.Append(NamespaceNames.XLinkPrefix).Append(':').Append(localName);
        }
        else if (namespaceUri == NamespaceNames.XmlNsUri)
        {
            temp.Append(XmlNamespaceLocalName(localName));
        }
        else
        {
            temp.Append(attr.Name);
        }

        temp.Append('=').Append('"');

        for (var i = 0; i < value.Length; i++)
        {
            switch (value[i])
            {
                case '&': temp.Append("&amp;"); break;
                case '\u00a0': temp.Append("&nbsp;"); break;
                case '"': temp.Append("&quot;"); break;
                case '<': temp.Append("&lt;"); break;
                case '>': temp.Append("&gt;"); break;
                default: temp.Append(value[i]); break;
            }
        }

        return temp.Append('"').ToString();
    }

    #pragma warning restore 1591
}
