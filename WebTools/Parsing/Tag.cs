using System;
using System.IO;
using System.Web;
using System.Collections.Generic;
using System.Text;

namespace SilentOrbit.Parsing
{
	public class Tag
	{
		/// <summary>
		/// Namespace of the current tag
		/// </summary>
		/// <value>The namespace.</value>
		public TagNamespace Namespace { get; set; }

		public string Name { get; set; }

		/// <summary>
		/// Text directly following start tag, text inside tag.
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Text following the closing or selfclosed tag
		/// </summary>
		public string After { get; set; }

		public readonly Dictionary<string,string> Attributes = new Dictionary<string, string>();

		/// <summary>
		/// Local namespace of this tag and below
		/// </summary>
		public Dictionary<string,TagNamespace> NS;

		public bool SelfClosed { get; set; }

		public readonly List<Tag> Children = new List<Tag>();
		public readonly Tag Parent;

		public Tag(string ns, string name, Tag parent)
		{
			this.Namespace = ns;
			this.Name = name;
			this.Parent = parent;
			this.After = ""; //After is never null
			//this.Value; //Value is null in self closed tags

			if (parent == null)
				this.NS = new Dictionary<string, TagNamespace>();
			else
				this.NS = parent.NS;

			//Selfclosing
			if (name == null)
				return; //Text "tag"
			if (name == "!doctype")
				SelfClosed = true;
			if (name.StartsWith("?xml"))
				SelfClosed = true;
		}

		public override string ToString()
		{
			var s = new StringBuilder();
			s.Append("<");
			if (Namespace != null)
				s.Append(Namespace + ":");
			s.Append(Name);
			foreach(var a in Attributes)
				s.Append(" " + a.Key + "=\"" + Html.Escape(a.Value) + "\"");
			if (SelfClosed)
				s.Append("/>");
			else
			{
				s.Append(">");
				s.Append(Value);
				s.Append("</" + Name + ">");
			}
			s.Append(After);
			s.Replace("\r", "");
			s.Replace("\n", "");
			return s.ToString();
		}

		/// <summary>
		/// Render tag and its content
		/// </summary>
		public string Render()
		{
			StringBuilder s = new StringBuilder();
			RenderTag(this, s);
			return s.ToString();
		}

		/// <summary>
		/// Render what's inside the tag, exclude the tag itself
		/// </summary>
		/// <returns>The content.</returns>
		public string RenderContent()
		{
			if (Children.Count == 0)
				return Value ?? "";

			StringBuilder s = new StringBuilder();
			s.Append(Value);
			foreach (var c in Children)
				RenderTag(c, s);
			return s.ToString();
		}

		static void RenderTag(Tag t, StringBuilder s)
		{
			s.Append("<");
			s.Append(t.Name);
			foreach (var a in t.Attributes)
				s.Append(" " + a.Key + "=\"" + Html.Escape(a.Value) + "\"");
			if (t.SelfClosed)
			{
				s.Append("/>");
				s.Append(t.After);
				return;
			}
			else
			{
				s.Append(">");
				s.Append(t.Value);
			}
			foreach (var c in t.Children)
				RenderTag(c, s);
			s.Append("</" + t.Name + ">");
			s.Append(t.After);
		}

		/// <summary>
		/// Scan up the hiearchy for a matching start tag
		/// </summary>
		public bool HasMatchingStartTag(Tag tag)
		{
			if (this.Name == tag.Name && this.Namespace == tag.Namespace)
				return true;
			if (Parent == null)
				return false;
			return Parent.HasMatchingStartTag(tag);
		}

		public Tag Element(string tagName)
		{
			if (tagName.Contains("://"))
				throw new ArgumentException("Namespace not valid inside tagname");

			tagName = tagName.ToLowerInvariant();
			foreach(var t in Children)
			{
				if (t.Name == tagName)
					return t;
			}
			return null;
		}

		public Tag Element(TagNamespace ns, string tagName)
		{
			tagName = tagName.ToLowerInvariant();
			foreach(var t in Children)
			{
				if (!ns.Equals(t.Namespace))
					continue;
				if (t.Name == tagName)
					return t;
			}
			return null;
		}

		public string Attribute(string key)
		{
			key = key.ToLowerInvariant();
			if (Attributes.ContainsKey(key) == false)
				return null;
			return Attributes[key];
		}
	}
}

