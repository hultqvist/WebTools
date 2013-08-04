using System;
using System.IO;
using System.Web;

namespace SilentOrbit.Parsing
{
    /// <summary>
    /// Writes the parsed html onto another file.
    /// By overriding this one slight modifications can be done to the html before it is being written
    /// </summary>
    public class HtmlRewriter : TagParser
    {
        readonly TextWriter writer;

        public HtmlRewriter(Stream input, Stream output) : base(input)
        {
            this.writer = new StreamWriter(output);
        }

        protected override void ParsedAttribute(Tag tag, TagNamespace ns, string key, string val)
        {
        }

        protected override void ParsedOpeningTag(Tag tag)
        {
            writer.Write("<" + tag.Name);
            foreach (var kv in tag.Attributes)
            {
                writer.Write(" " + kv.Key + "=\"" + HttpUtility.HtmlEncode(kv.Value) + "\"");
            }
            if (tag.SelfClosed)
                writer.Write("/");
            writer.Write(">");
        }

        protected override void ParsedClosingTag(Tag tag)
        {
            writer.Write("</" + tag.Name + ">");
        }

        protected override void ParsedText(string decodedText)
        {
            writer.Write(HttpUtility.HtmlEncode(decodedText));
        }

    }
}

