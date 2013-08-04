using System;

namespace SilentOrbit.Parsing
{
	public class TagNamespace
	{
		readonly string Namespace;

		TagNamespace(string ns)
		{
			if (ns == null)
				throw new ArgumentNullException();
			this.Namespace = ns;
		}

		public static implicit operator TagNamespace(string ns)
		{
			if (ns == null)
				return null;
			return new TagNamespace(ns);
		}

		public static bool operator ==(TagNamespace a, TagNamespace b)
		{
			if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null))
				return true;
			if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
				return false;
			return a.Namespace == b.Namespace;
		}

		public static bool operator !=(TagNamespace a, TagNamespace b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			var o = obj as TagNamespace;
			if (o == null)
				return false;
			return Namespace == o.Namespace;
		}

		public override int GetHashCode()
		{
			return Namespace.GetHashCode();
		}

		public override string ToString()
		{
			return "{" + Namespace + "}";
		}
	}
}

