using System;

namespace SilentOrbit.Extractor
{
	class SelectionBubbler
	{
		readonly HtmlData data;

		SelectionBubbler(HtmlData data)
		{
			this.data = data;
		}

		/// <summary>
		/// Bubble classes and ID to top level
		/// </summary>
		public static void Bubble(HtmlData data, Options options)
		{
			var sm = new SelectionBubbler(data);

			if (options.BubbleClass)
				sm.BubbleClasses(data);

			if (options.BubbleID)
				sm.BubbleID(data);
		}

		void BubbleID(SelectorData parent)
		{
			foreach (var s in parent.SubClass.ToArray())
				BubbleID(s);

			foreach (var s in parent.SubID.ToArray())
			{
				BubbleID(s);

				//Add IDs to top level
				if (data.SubID.Contains(s) == false)
					data.AddElement(s);
			}
		}

		void BubbleClasses(SelectorData parent)
		{
			foreach (var s in parent.SubID.ToArray())
			{
				BubbleClasses(s);

				foreach (var sub in s.SubClass)
				{
					if (parent.Type == SelectorType.Class && parent.Selector == sub.Selector)
						continue;

					parent.AddElement(sub);
				}
			}

			foreach (var s in parent.SubClass.ToArray())
			{
				BubbleClasses(s);

				foreach (var sub in s.SubClass)
				{
					if (parent.Type == SelectorType.Class && parent.Selector == sub.Selector)
						continue;

					parent.AddElement(sub);
				}
			}
		}
	}
}

