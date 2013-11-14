using System;
using CommandLine;
using CommandLine.Text;
using System.IO;
using System.Linq;

namespace SilentOrbit
{
	class Options
	{
		/// <summary>
		/// Root path to the html files.
		/// </summary>
		[Option("htmlroot", Required = true, HelpText = "Path to the html directory, subdirectories will be included")]
		public string WebRoot { get; set; }

		[Option("namespace", Required = true, HelpText = "Root Namespace")]
		public string Namespace { get; set; }

		[Option("suffix", Required = false, HelpText = "Append to the end of each file related class name")]
		public string FileSuffix { get; set; }

		/// <summary>
		/// Generated code is internal rather than public(default)
		/// </summary>
		[Option("internal", Required = false, HelpText = "Generate internal classes rather than public(default)")]
		public bool AccessInternal { get; set; }

		[Option("filename-property", Required = false, HelpText = "Generate properties for each file containing its name")]
		public bool GenerateFilenameProperties { get; set; }

		[Option("outputCS", Required = true, HelpText = "Output path to generated .cs file")]
		public string OutputCS { get; set; }

		[Option("outputHTML", Required = false, HelpText = "Output path to modified HTML files")]
		public string OutputHTML { get; set; }


		[Option("inputCSS", Required = false, HelpText = "Single CSS file to be read")]
		public string InputCSS { get; set; }

		[Option("outputCSS", Required = false, HelpText = "Obfuscated CSS file to be written")]
		public string OutputCSS { get; set; }

		public static Options Parse(string[] args)
		{
			var result = CommandLine.Parser.Default.ParseArguments<Options>(args);
			var options = result.Value;
			if (result.Errors.Any())
			{
				//Console.WriteLine(HelpText.AutoBuild<Options>(result));
				return null;
			}

			options.WebRoot = Path.GetFullPath(options.WebRoot).TrimEnd(Path.DirectorySeparatorChar);
			options.OutputCS = Path.GetFullPath(options.OutputCS);
			if(options.OutputHTML != null)
				options.OutputHTML = Path.GetFullPath(options.OutputHTML);
			return options;
		}
	}
}

