using System;
using System.Collections.Generic;

namespace SilentOrbit.Extractor
{
	public enum SelectorType
	{
		ID
,
		Class
	}

	public class SelectorData
	{
		public string TagName { get; set; }
	
		public SelectorType Type { get; set; }

		public string Selector { get; set; }

		public List<SelectorData> Elements = new List<SelectorData>();

		public void AddElement(SelectorData s)
		{
			SelectorData sub = Get(s);
			if (sub == null)
			{
				Elements.Add(s);
				return;
			}

			//Merge sub with s
			foreach (var e in s.Elements)
			{
				sub.AddElement(e);
			}
		}

		public SelectorData Get(SelectorData s)
		{
			if (s.Type == SelectorType.Class)
				return GetClass(s.Selector);
			if (s.Type == SelectorType.ID)
				return GetID(s.Selector);
			throw new NotImplementedException();
		}

		public SelectorData GetClass(string name)
		{
			foreach (var s in this.Elements)
			{
				if (s.Type != SelectorType.Class)
					continue;
				if (s.Selector != name)
					continue;
				return s;
			}
			return null;
		}

		public SelectorData CreateClass(string name)
		{
			//Find existing
			var s = GetClass(name);
			if (s != null)
				return s;
        
			//Create new
			s = new SelectorData();
			s.Selector = name;
			s.Type = SelectorType.Class;
			Elements.Add(s);
			return s;
		}

		public SelectorData GetID(string name)
		{
			foreach (var s in this.Elements)
			{
				if (s.Type != SelectorType.ID)
					continue;
				if (s.Selector != name)
					continue;
				return s;
			}
			return null;
		}

		public SelectorData CreateID(string name, string tagName)
		{
			//Find existing
			var s = GetID(name);
			if (s != null)
				return s;

			//Create new
			s = new SelectorData();
			s.TagName = tagName;
			s.Selector = name;
			s.Type = SelectorType.ID;
			Elements.Add(s);
			return s;
		}

		public void GetClasses(List<string> classes)
		{
			foreach (SelectorData sd in this.Elements)
			{
				if (sd.Type != SelectorType.Class)
					continue;

				if (classes.Contains(sd.Selector))
					continue;
				classes.Add(sd.Selector);
			}

			foreach (var s in Elements)
				s.GetClasses(classes);
		}

		public void GetIDs(List<string> ids)
		{
			foreach (SelectorData sd in this.Elements)
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

