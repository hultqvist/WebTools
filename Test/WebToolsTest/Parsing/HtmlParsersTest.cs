using System;
using NUnit.Framework;
using SilentOrbit.Parsing;
using System.Text;
using System.IO;

namespace SilentOrbit.Test.Parsing
{
    [TestFixture()]
    public class HtmlParsersTest
    {
        /// <summary>
        /// Sample
        /// </summary>
        static readonly string html = @"
<div>
    <p>Hello
<strong>There</strong></p>
    <div/>
    <img src=""http://example.com/image.png"" title=""Test Image""/>
    End <a href=""http://example.com"">Link to <img src=""http://example.com/image.png"">
    <script></script>
</div>
";

        /// <summary>
        /// Minor changes are expected such as tag closing.
        /// </summary>
        static readonly string htmlWritten = @"
<div>
    <p>Hello
<strong>There</strong></p>
    <div/>
    <img src=""http://example.com/image.png"" title=""Test Image""/>
    End <a href=""http://example.com"">Link to <img src=""http://example.com/image.png"">
    <script></script>
</img></a></div>
";
        /// <summary>
        /// Whitelisting and img rewrite
        /// </summary>
        static readonly string htmlCleaned = @"
<div>
    <p>Hello
<strong>There</strong></p>
    <div/>
    [Test Image: <a href=""http://example.com/image.png"">http://example.com/image.png</a>]
    End <a href=""http://example.com/"">Link to [img: <a href=""http://example.com/image.png"">http://example.com/image.png</a>]
    
</img></a></div>
";

        [Test()]
        public void TestWriting()
        {
            var writer = new HtmlWriter();
            TagParser.Parse(html, writer);

            Assert.AreEqual(htmlWritten, writer.ToString());
        }

        [Test()]
        public void TestCleaning()
        {
            var writer = new HtmlWriter();
            var clean = new HtmlCleaner(writer);
            TagParser.Parse(html, clean);

            Assert.AreEqual(htmlCleaned, writer.ToString());
        }
    }
}

