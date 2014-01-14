using System;
using System.Collections.Generic;
using SilentOrbit.Code;

namespace SilentOrbit.Data
{
	public enum SelectorType
	{
		ID = 1,
		Class = 2,
	}

	public class SelectorData
	{
		public string TagName { get; set; }
	
		public SelectorType Type { get; set; }

		public string Selector { get; set; }

		public List<SelectorData> SubID = new List<SelectorData>();
		public List<SelectorData> SubClass = new List<SelectorData>();

		protected SelectorData()
		{	
		}

		public void AddElement(SelectorData s)
		{
			SelectorData sub = Get(s);
			if (sub == null)
			{
				if (s.Type == SelectorType.ID)
					SubID.Add(s);
				else if (s.Type == SelectorType.Class)
					SubClass.Add(s);
				else
					throw new NotImplementedException();
				return;
			}

			//Merge s into sub
			foreach (var e in s.SubID)
				sub.AddElement(e);
			foreach (var e in s.SubClass)
				sub.AddElement(e);
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
			foreach (var s in this.SubClass)
			{
				if (s.Type != SelectorType.Class)
					throw new InvalidOperationException();
				if (s.Selector != name)
					continue;
				return s;
			}
			return null;
		}

		public SelectorData CreateClass(string name, string tagName)
		{
			//Find existing
			var s = GetClass(name);
			if (s != null)
				return s;
        
			//Create new
			s = new SelectorData();
			s.TagName = tagName;
			s.Selector = name;
			s.Type = SelectorType.Class;
			SubClass.Add(s);
			return s;
		}

		public SelectorData GetID(string name)
		{
			foreach (var s in this.SubID)
			{
				if (s.Type != SelectorType.ID)
					throw new InvalidOperationException();
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
			SubID.Add(s);
			return s;
		}

		public void GetClasses(List<string> classes)
		{
			foreach (SelectorData sd in this.SubClass)
			{
				if (sd.Type != SelectorType.Class)
					throw new InvalidOperationException();

				if (classes.Contains(sd.Selector))
					continue;
				classes.Add(sd.Selector);
			}

			foreach (var s in SubID)
				s.GetClasses(classes);
			foreach (var s in SubClass)
				s.GetClasses(classes);
		}

		/// <summary>
		/// Implementation warning, this one only works if --bubble-id is set.
		/// </summary>
		public void GetIDs(List<string> ids)
		{
			foreach (SelectorData sd in this.SubID)
			{
				if (sd.Type != SelectorType.ID)
					continue;

				if (ids.Contains(sd.Selector))
					throw new InvalidOperationException("Duplicate ID: " + sd.Selector);
				ids.Add(sd.Selector);
			}
		}

		public string ClassName
		{
			get
			{
				if (SubID.Count + SubClass.Count == 0)
					return SharpKitClasses.FromSelectorData(this);

				string className = Name.ToCamelCase(Selector);
				//className += "Element";
				if (Options.Instance.GenerateTypeSuffix == false)
					return className;
				if (Type == SelectorType.ID)
					return "Id" + className;
				if (Type == SelectorType.Class)
					return "Class" + className;

				throw new NotImplementedException();
			}
		}

		public string PropertyName
		{
			get
			{
				string className = Name.ToCamelCase(Selector);
				if (Options.Instance.GenerateTypeSuffix == false)
					return className;
				if (Type == SelectorType.ID)
					return "Id" + className;
				if (Type == SelectorType.Class)
					return "Class" + className;

				throw new NotImplementedException();
			}
		}

		public override string ToString()
		{
			return string.Format("[{0}, {1} sub ID, {2} sub Class]", Selector, SubID.Count, SubClass.Count);
		}
	}
}


