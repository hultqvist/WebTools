using System;

namespace SilentOrbit.Parsing
{
    public class NullOutput : ITagOutput
    {
        public void ParsedAttribute(Tag tag, TagNamespace ns, string key, string val)
        {
        }

        public void ParsedOpeningTag(Tag tag)
        {
        }

        public void ParsedClosingTag(Tag tag)
        {
        }

        public void ParsedText(Tag parent, string decodedText)
        {
        }

        public void ParseError(string message)
        {
        }

    }
}

