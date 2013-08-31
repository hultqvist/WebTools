using System;

namespace SilentOrbit.Parsing
{
    /// <summary>
    /// Output of TagParser
    /// </summary>
    public interface ITagOutput
    {
        void ParsedAttribute(Tag tag, TagNamespace ns, string key, string val);

        void ParsedOpeningTag(Tag tag);

        void ParsedClosingTag(Tag tag);

        void ParsedText(Tag parent, string decodedText);

        void ParseError(string message);
    }
}

