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
		/// Path to the html directory, subdirectories will be included
		/// </summary>
		[Option("htmlroot", Required = true, HelpText = "Path to the html directory, subdirectories will be included")]
		public string WebRoot { get; set; }

		/// <summary>
		/// Root Namespace
		/// </summary>
		[Option("namespace", Required = true, HelpText = "Root Namespace")]
		public string Namespace { get; set; }

		/// <summary>
		/// Append to the end of each file related class name
		/// </summary>
		[Option("suffix", Required = false, HelpText = "Append to the end of each file related class name")]
		public string FileSuffix { get; set; }

		/// <summary>
		/// Generated code is internal rather than public(default)
		/// </summary>
		[Option("internal", Required = false, HelpText = "Generate internal classes rather than public(default)")]
		public bool AccessInternal { get; set; }

		/// <summary>
		/// Generate properties for each file containing its name
		/// </summary>
		[Option("filename-property", Required = false, HelpText = "Generate properties for each file containing its name")]
		public bool GenerateFilenameProperties { get; set; }

		/// <summary>
		/// Generate Element property for each id
		/// </summary>
		[Option("element-property", Required = false, HelpText = "Generate Element property for each id")]
		public bool GenerateElementProperties { get; set; }

		/// <summary>
		/// Generate "Id" and "Class" suffix to generated names to avoid collision between them.
		/// </summary>
		[Option("type-suffix", Required = false, HelpText = "Generate \"Id\" and \"Class\" suffix to generated names to avoid collision between them.")]
		public bool GenerateTypeSuffix { get; set; }

		/// <summary>
		/// Generate one class Classes containing all parsed classes
		/// </summary>
		[Option("generate-classes", Required = false, HelpText = "Generate one class Classes containing all parsed classes")]
		public bool GenerateGlobalClasses { get; set; }

		/// <summary>
		/// Allow ID to bubble to the top making them available earlier in the tree.
		/// </summary>
		[Option("bubble-id", Required = false, HelpText = "Allow ID to bubble to the top making them available earlier in the tree.")]
		public bool BubbleID { get; set; }

		/// <summary>
		/// Allow Classes to bubble to the top making them available earlier in the tree.
		/// </summary>
		[Option("bubble-class", Required = false, HelpText = "Allow Classes to bubble to the top making them available earlier in the tree.")]
		public bool BubbleClass { get; set; }

		/// <summary>
		/// Output path to generated .cs file
		/// </summary>
		[Option("outputCS", Required = true, HelpText = "Output path to generated .cs file")]
		public string OutputCS { get; set; }

		/// <summary>
		/// Output path to modified HTML files
		/// </summary>
		[Option("outputHTML", Required = false, HelpText = "Output path to modified HTML files")]
		public string OutputHTML { get; set; }

		/// <summary>
		/// Generate obfuscated id/class names when not in debug mode
		/// </summary>
		[Option("minimize-names", Required = false, HelpText = "Generate obfuscated id/class names when not in debug mode")]
		public bool MinimizeNames { get; set; }

		/// <summary>
		/// Single CSS file to be read to parse IDs and Classes.
		/// </summary>
		[Option("inputCSS", Required = false, HelpText = "Single CSS file to be read to parse IDs and Classes.")]
		public string InputCSS { get; set; }

		/// <summary>
		/// Path to write CSS with obfuscated id and class names
		/// </summary>
		[Option("outputCSS", Required = false, HelpText = "Path to write CSS with obfuscated id and class names")]
		public string OutputCSS { get; set; }

		//Static instance

		public static Options Instance;

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
			Instance = options;
			return options;
		}
	}
}

