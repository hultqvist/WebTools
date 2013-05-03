using System;
using System.Collections.Generic;

namespace SilentOrbit.Extractor
{
	public enum SelectorType
	{
		ID,
		Class
	}

	public class SelectorData
	{
		public static SelectorData ID(string name)
		{
			var s = new SelectorData();
			s.Selector = name;
			s.Type = SelectorType.ID;
			return s;
		}

		public static SelectorData Class(string name)
		{
			var s = new SelectorData();
			s.Selector = name;
			s.Type = SelectorType.Class;
			return s;
		}

		public SelectorType Type { get; set; }

		public string Selector { get; set; }

		public List<SelectorData> Elements = new List<SelectorData>();

		public void AddElements(SelectorData s)
		{
			foreach(var e in Elements)
			{
				if (e.Selector == s.Selector)
				{
					//Merge siblings
					foreach(var sub in s.Elements)
						e.AddElements(sub);
					return;
				}
			}
			Elements.Add(s);
		}

		public void GetClasses(List<string> classes)
		{
			foreach(SelectorData sd in this.Elements)
			{
				if (sd.Type != SelectorType.Class)
					continue;

				if (classes.Contains(sd.Selector))
					continue;
				classes.Add(sd.Selector);
			}

			foreach(var s in Elements)
				s.GetClasses(classes);
		}

		public void GetIDs(List<string> ids)
		{
			foreach(SelectorData sd in this.Elements)
			{
				if (sd.Type != SelectorType.ID)
					continue;

				if (ids.Contains(sd.Selector))
					throw new InvalidOperationException("Duplicate ID: " + sd.Selector);
				ids.Add(sd.Selector);
			}
		}

		public override string ToString()
		{
			return string.Format("[{0}, {1} sub]", Selector, Elements.Count);
		}
	}
}

