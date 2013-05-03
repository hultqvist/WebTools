using System;
using System.Collections.Generic;

namespace SilentOrbit.Extractor
{
	public class HtmlData : SelectorData
	{
		/// <summary>
		/// Filename as referenced by Javascript
		/// </summary>
		/// <value>The name of the file.</value>
		public string FileName { get; set; }
		/// <summary>
		/// Top Class name that we generate a partial of
		/// </summary>
		public string ClassName { get; set; }

		public override string ToString()
		{
			return FileName + " " + base.ToString();
		}
	}
}

