using System;

namespace SilentOrbit.Parsing
{
	/// <summary>
	/// Helps to send only a subtree of the document to an ITagOutput
	/// Not used at the moment
	/// </summary>
	public abstract class NestedTagOutput : ITagOutput
	{
		/// <summary>
		/// Current root being sent
		/// </summary>
		Tag subTag;
		/// <summary>
		/// Where to send all messages
		/// </summary>
		ITagOutput subOutput;

		public NestedTagOutput()
		{
		}

		protected abstract ITagOutput ParseTag(Tag tag);

		#region ITagOutput implementation

		public void ParsedAttribute(Tag tag, TagNamespace ns, string key, string val)
		{
			if (subOutput != null)
				subOutput.ParsedAttribute(tag, ns, key, val);
		}

		public void ParsedOpeningTag(Tag tag)
		{
			if (subOutput == null)
			{
				subOutput = ParseTag(tag);
				if (subOutput == null)
					return;
				subTag = tag;
			}
			subOutput.ParsedOpeningTag(tag);
		}

		public void ParsedClosingTag(Tag tag)
		{
			if (subOutput != null)
				subOutput.ParsedClosingTag(tag);
			if (subTag == tag)
			{
				subOutput = null;
				subTag = null;
			}
		}

		public void ParsedText(string decodedText)
		{
			if (subOutput != null)
				subOutput.ParsedText(decodedText);
		}

		public abstract void ParseError(string message);

		#endregion
	}
}

