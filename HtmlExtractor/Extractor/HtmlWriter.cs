using System;
using System.IO;
using System.Web;
using SilentOrbit.Parsing;

namespace SilentOrbit.Extractor
{
    /// <summary>
    /// Writes the parsed html onto another file.
    /// By overriding this one slight modifications can be done to the html before it is being written
    /// </summary>
    public class HtmlWriter : ITagOutput, IDisposable
    {
        readonly TextWriter writer;

		public HtmlWriter(string path)
        {
            this.writer = new StreamWriter(File.Open(path, FileMode.Create));
		}

        public void Dispose()
        {
            writer.Close();
        }

        public void ParsedAttribute(Tag tag, TagNamespace ns, string key, string val)
        {
            tag.Attributes.Add(key, val);
        }

        public void ParsedOpeningTag(Tag tag)
        {
            if (tag.Name == "!doctype")
            {
                writer.WriteLine("<!DOCTYPE html>");
                return;
            }

            writer.Write("<" + tag.Name);
            foreach (var kv in tag.Attributes)
            {
                writer.Write(" " + kv.Key + "=\"" + HttpUtility.HtmlEncode(kv.Value) + "\"");
            }
            if (tag.SelfClosed)
                writer.Write("/");
            writer.Write(">");
        }

        public void ParsedClosingTag(Tag tag)
        {
            writer.Write("</" + tag.Name + ">");
        }

        public void ParsedText(Tag parent, string decodedText)
        {
            writer.Write(HttpUtility.HtmlEncode(decodedText));
        }

        public void ParseError(string message)
        {
            throw new NotImplementedException();
        }
    }
}

