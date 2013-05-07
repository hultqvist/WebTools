using System;

namespace SilentOrbit.Extractor
{
	public class SelectionMerger
	{
		readonly HtmlData data;

		public SelectionMerger(HtmlData data)
		{
			this.data = data;
		}
		/// <summary>
		/// Merge classes and ID to top level
		/// </summary>
		public static void Merge(HtmlData data)
		{
			var sm = new SelectionMerger(data);
			Console.WriteLine("Merging: " + data.FileName);

			sm.MergeClasses(data);

			sm.MergeID(data);
		}

		void MergeID(SelectorData parent)
		{
			foreach (var s in parent.Elements.ToArray())
			{
				MergeID(s);

				//Add IDs to top level
				if (s.Type == SelectorType.ID)
				{
					if (data.Elements.Contains(s) == false)
						data.AddElement(s);
				}
			}
		}

		void MergeClasses(SelectorData parent)
		{
			foreach (var s in parent.Elements.ToArray())
			{
				MergeClasses(s);

				foreach (var sub in s.Elements)
				{
					if (sub.Type != SelectorType.Class)
						continue;

					if (parent.Type == SelectorType.Class && parent.Selector == sub.Selector)
						continue;

					parent.AddElement(sub);
				}
			}
		}
	}
}

