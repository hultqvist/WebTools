using System;
using SilentOrbit.Extractor;

namespace SilentOrbit.Data
{
	class SelectorBubbler
	{
		readonly HtmlData data;

		SelectorBubbler(HtmlData data)
		{
			this.data = data;
		}

		/// <summary>
		/// Bubble classes and ID to top level
		/// </summary>
		public static void Bubble(HtmlData data, Options options)
		{
			var sm = new SelectorBubbler(data);

			if (options.BubbleClass)
				sm.BubbleClasses(data);

			if (options.BubbleID)
				sm.BubbleID(data);
		}

		void BubbleID(SelectorData parent)
		{
			foreach (var s in parent.SubID.ToArray())
			{
				//Add IDs to top level
				if (data.SubID.Contains(s) == false)
					data.AddElement(s);

				BubbleID(s);

				foreach (var sub in s.SubID)
					parent.AddElement(sub);
			}

			foreach (var s in parent.SubClass.ToArray())
			{
				BubbleID(s);

				foreach (var sub in s.SubID)
					parent.AddElement(sub);
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

