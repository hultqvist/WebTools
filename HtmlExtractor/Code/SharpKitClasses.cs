using System;
using SilentOrbit.Data;
using SilentOrbit.Extractor;

namespace SilentOrbit.Code
{
	public static class SharpKitClasses
	{
		public static string FromSelectorData(SelectorData sel)
		{
			if (sel.TagName == null)
				throw new ArgumentNullException("TagName can't be null");

			switch (sel.TagName)
			{
				case "a":
					return "HtmlAnchorElement";
				case "applet":
					return "HtmlAppletElement";
				case "area":
					return "HtmlAreaElement";
				case "audio":
					return "HtmlAudioElement";
				case "base":
					return "HtmlBaseElement";
				case "basefont":
					return "HtmlBaseFontElement";
				case "body":
					return "HtmlBodyElement";
				case "br":
					return "HtmlBRElement";
				case "button":
					return "HtmlButtonElement";
				case "canvas":
					return "HtmlCanvasElement";
				case "datalist":
					return "HtmlDataListElement";
				case "details":
					return "HtmlDetailsElement";
				case "dialog":
					return "HtmlDialogElement";
				case "directory":
					return "HtmlDirectoryElement";
				case "div":
					return "HtmlDivElement";
				case "embed":
					return "HtmlEmbedElement";
				case "fieldset":
					return "HtmlFieldSetElement";
				case "font":
					return "HtmlFontElement";
				case "form":
					return "HtmlFormElement";
				case "frame":
					return "HtmlFrameElement";
				case "frameset":
					return "HtmlFrameSetElement";
				case "head":
					return "HtmlHeadElement";
				case "heading":
					return "HtmlHeadingElement";
				case "hr":
					return "HtmlHRElement";
				case "html":
					return "HtmlHtmlElement";
				case "iframe":
					return "HtmlIFrameElement";
				case "img":
					return "HtmlImageElement";
				case "input":
					return "HtmlInputElement";
				case "intent":
					return "HtmlIntentElement";
				case "keygen":
					return "HtmlKeygenElement";
				case "label":
					return "HtmlLabelElement";
				case "legend":
					return "HtmlLegendElement";
				case "li":
					return "HtmlLIElement";
				case "link":
					return "HtmlLinkElement";
				case "map":
					return "HtmlMapElement";
				case "media":
					return "HtmlMediaElement";
				case "menu":
					return "HtmlMenuElement";
				case "meta":
					return "HtmlMetaElement";
				case "meter":
					return "HtmlMeterElement";
				case "object":
					return "HtmlObjectElement";
				case "ol":
					return "HtmlOListElement";
				case "optgroup":
					return "HtmlOptGroupElement";
				case "option":
					return "HtmlOptionElement";
				case "output":
					return "HtmlOutputElement";
				case "p":
					return "HtmlParagraphElement";
				case "param":
					return "HtmlParamElement";
				case "pre":
					return "HtmlPreElement";
				case "progress":
					return "HtmlProgressElement";
				case "quote":
					return "HtmlQuoteElement";
				case "script":
					return "HtmlScriptElement";
				case "select":
					return "HtmlSelectElement";
				case "source":
					return "HtmlSourceElement";
				case "span":
					return "HtmlSpanElement";
				case "style":
					return "HtmlStyleElement";
				case "caption":
					return "HtmlTableCaptionElement";
				case "td":
					return "HtmlTableCellElement";
				case "col":
					return "HtmlTableColElement";
				case "table":
					return "HtmlTableElement";
				case "tr":
					return "HtmlTableRowElement";
				case "tbody":
					return "HtmlTableSectionElement";
				case "template":
					return "HtmlTemplateElement";
				case "textarea":
					return "HtmlTextAreaElement";
				case "title":
					return "HtmlTitleElement";
				case "track":
					return "HtmlTrackElement";
				case "ul":
					return "HtmlUListElement";
				case "video":
					return "HtmlVideoElement";

				default:
					return "HtmlElement";
			}
		}
	}
}

