using System;
using NUnit.Framework;
using SilentOrbit.Parsing;

namespace SilentOrbit.Test.Parsing
{
	[TestFixture()]
	public class DocumentParserTest
	{
		[Test()]
		public void TestCase()
		{
			//Parse valid html
			string html = @"
<div>
	<p>Hello
<strong>There</strong></p>
	<div/>
	End
	<script></script>
</div>
";
			var doc = DocumentParser.Parse(html);
			Assert.IsNotNull(doc);
			Tag t;

			t = doc;
			Assert.AreEqual("div", t.Name);
			Assert.AreEqual("\n\t", t.Value);
			Assert.AreEqual(3, t.Children.Count);
			Assert.AreEqual("", t.After);
			Assert.AreEqual(false, t.SelfClosed);

			t = doc.Children[0];
			Assert.AreEqual("p", t.Name);
			Assert.AreEqual("Hello\n", t.Value);
			Assert.AreEqual(1, t.Children.Count);
			Assert.AreEqual("\n\t", t.After);
			Assert.AreEqual(false, t.SelfClosed);

			t = doc.Children[0].Children[0];
			Assert.AreEqual("strong", t.Name);
			Assert.AreEqual("There", t.Value);
			Assert.AreEqual(0, t.Children.Count);
			Assert.AreEqual("", t.After);
			Assert.AreEqual(false, t.SelfClosed);

			t = doc.Children[1];
			Assert.AreEqual("div", t.Name);
			Assert.AreEqual(null, t.Value);
			Assert.AreEqual(0, t.Children.Count);
			Assert.AreEqual("\n\tEnd\n\t", t.After);
			Assert.AreEqual(true, t.SelfClosed);

			t = doc.Children[2];
			Assert.AreEqual("script", t.Name);
			Assert.AreEqual("", t.Value);
			Assert.AreEqual(0, t.Children.Count);
			Assert.AreEqual("\n", t.After);
			Assert.AreEqual(false, t.SelfClosed);
		}
	}
}

