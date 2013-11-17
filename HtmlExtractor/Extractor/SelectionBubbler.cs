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

			if(options.BubbleClass)
				sm.BubbleClasses(data);

			if(options.BubbleID)
				sm.BubbleID(data);
		}

		void BubbleID(SelectorData parent)
		{
			foreach (var s in parent.Elements.ToArray())
			{
				BubbleID(s);

				//Add IDs to top level
				if (s.Type == SelectorType.ID)
				{
					if (data.Elements.Contains(s) == false)
						data.AddElement(s);
				}
			}
		}

		void BubbleClasses(SelectorData parent)
		{
			foreach (var s in parent.Elements.ToArray())
			{
				BubbleClasses(s);

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

