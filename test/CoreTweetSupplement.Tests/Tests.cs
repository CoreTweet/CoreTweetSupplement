using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CoreTweet;
using Newtonsoft.Json;
using Xunit;

namespace CoreTweetSupplementTest
{
    public class Tests
    {
        private static object InvokeStatic<T>(string methodName, params object[] parameters)
        {
            return (T)typeof(CoreTweetSupplement).GetTypeInfo().GetDeclaredMethod(methodName).Invoke(null, parameters);
        }

        private static string LoadTestData(string name)
        {
            var resourceName = nameof(CoreTweetSupplementTest) + ".testdata." + name;
            using (var reader = new StreamReader(typeof(Tests).GetTypeInfo().Assembly.GetManifestResourceStream(resourceName)))
                return reader.ReadToEnd();
        }

        [Fact]
        public void TestCharFromInt()
        {
            Assert.Equal("a", InvokeStatic<string>("CharFromInt", 97u));
            Assert.Equal("𠮟", InvokeStatic<string>("CharFromInt", (uint)0x20B9F));
        }

        [Fact]
        public void TestHtmlDecode()
        {
            Assert.Equal("test<>a&\"'♪♪0", InvokeStatic<string>("HtmlDecode", "test&lt;&gt;a&amp;&quot;&apos;&#9834;&#x266A;0"));
        }

        [Fact]
        public void TestParseSource()
        {
            var source1 = CoreTweetSupplement.ParseSource(@"<a href=""http://twitter.com/download/iphone"" rel=""nofollow"">Twitter for iPhone</a>");
            Assert.Equal("Twitter for iPhone", source1.Name);
            Assert.Equal("http://twitter.com/download/iphone", source1.Href);

            var source2 = CoreTweetSupplement.ParseSource("web");
            Assert.Equal("web", source2.Name);
            Assert.Null(source2.Href);
        }

        [Fact]
        public void TestEnumerateTextPartsPlain()
        {
            var array = CoreTweetSupplement.EnumerateTextParts("test", null).ToArray();
            Assert.Equal(1, array.Length);
            Assert.Equal(TextPartType.Plain, array[0].Type);
            Assert.Equal("test", array[0].RawText);
            Assert.Equal("test", array[0].Text);
            Assert.Null(array[0].Entity);
        }

        [Fact]
        public void TestEnumerateTextPartsNonBmp()
        {
            var json = LoadTestData("Status469054246503989248.json");
            var array = JsonConvert.DeserializeObject<Status>(json).EnumerateTextParts().ToArray();
            Assert.Equal(6, array.Length);
            Assert.Equal(TextPartType.Plain, array[0].Type);
            Assert.Equal("てすとtest𠮷野家 ", array[0].RawText);
            Assert.Equal("てすとtest𠮷野家 ", array[0].Text);
            Assert.Null(array[0].Entity);
            Assert.Equal(TextPartType.Hashtag, array[1].Type);
            Assert.Equal("#𠮷野家", array[1].RawText);
            Assert.Equal("#𠮷野家", array[1].Text);
            Assert.NotNull(array[1].Entity);
            Assert.Equal(TextPartType.Plain, array[2].Type);
            Assert.Equal(" aa ", array[2].RawText);
            Assert.Equal(" aa ", array[2].Text);
            Assert.Null(array[2].Entity);
            Assert.Equal(TextPartType.UserMention, array[3].Type);
            Assert.Equal("@azyobuzin_test", array[3].RawText);
            Assert.Equal("@azyobuzin_test", array[3].Text);
            Assert.NotNull(array[3].Entity);
            Assert.Equal(TextPartType.Plain, array[4].Type);
            Assert.Equal(" ", array[4].RawText);
            Assert.Equal(" ", array[4].Text);
            Assert.Null(array[4].Entity);
            Assert.Equal(TextPartType.Url, array[5].Type);
            Assert.Equal("http://t.co/KmtlVpXaUN", array[5].RawText);
            Assert.Equal("pic.twitter.com/KmtlVpXaUN", array[5].Text);
            Assert.NotNull(array[5].Entity);
        }

        [Fact]
        public void TestEnumerateTextPartsCharReference()
        {
            var json = LoadTestData("Status469059084289708032.json");
            var array = JsonConvert.DeserializeObject<Status>(json).EnumerateTextParts().ToArray();
            Assert.Equal(3, array.Length);
            Assert.Equal(TextPartType.Plain, array[0].Type);
            Assert.Equal("ってって ", array[0].RawText);
            Assert.Equal("ってって ", array[0].Text);
            Assert.Null(array[0].Entity);
            Assert.Equal(TextPartType.Hashtag, array[1].Type);
            Assert.Equal("#test", array[1].RawText);
            Assert.Equal("#test", array[1].Text);
            Assert.NotNull(array[1].Entity);
            Assert.Equal(TextPartType.Plain, array[2].Type);
            Assert.Equal(" &amp;&lt;てすと&gt;&amp;&amp;amp;", array[2].RawText);
            Assert.Equal(" &<てすと>&&amp;", array[2].Text);
            Assert.Null(array[2].Entity);
        }

        [Fact]
        public void TestAlternativeProfileImageUriSuffix()
        {
            Assert.Equal("", InvokeStatic<string>("GetAlternativeProfileImageUriSuffix", new object[] { null }));
            Assert.Equal("", InvokeStatic<string>("GetAlternativeProfileImageUriSuffix", ""));
            Assert.Equal("", InvokeStatic<string>("GetAlternativeProfileImageUriSuffix", "orig"));
            Assert.Equal("_mini", InvokeStatic<string>("GetAlternativeProfileImageUriSuffix", "mini"));
            Assert.Equal("_normal", InvokeStatic<string>("GetAlternativeProfileImageUriSuffix", "normal"));
            Assert.Equal("_bigger", InvokeStatic<string>("GetAlternativeProfileImageUriSuffix", "bigger"));
        }

        [Fact]
        public void TestAlternativeProfileImageUri()
        {
            // Basic tests
            Assert.Equal(
                new Uri("http://example.com/path1/path2/test.png"),
                InvokeStatic<Uri>("GetAlternativeProfileImageUri", "http://example.com/path1/path2/test_normal.png", "orig")
            );
            Assert.Equal(
                new Uri("http://example.com/path1/path2/test_normal.gif"),
                InvokeStatic<Uri>("GetAlternativeProfileImageUri", "http://example.com/path1/path2/test_normal.gif", "normal")
            );
            Assert.Equal(
                new Uri("http://example.com/path1/path2/test_mini.jpg"),
                InvokeStatic<Uri>("GetAlternativeProfileImageUri", "http://example.com/path1/path2/test_normal.jpg", "mini")
            );
            Assert.Equal(
                new Uri("http://example.com/path1/path2/test_bigger"),
                InvokeStatic<Uri>("GetAlternativeProfileImageUri", "http://example.com/path1/path2/test_normal", "bigger")
            );
            // URL escape tests
            Assert.Equal(
                new Uri("http://example.com/%E3%83%86%E3%82%B9%E3%83%88.png"),
                InvokeStatic<Uri>("GetAlternativeProfileImageUri", "http://example.com/%E3%83%86%E3%82%B9%E3%83%88_normal.png", "orig")
            );
            Assert.Equal(
                new Uri("http://example.com/%83%65%83%58%83%67_mini.jpeg"),
                InvokeStatic<Uri>("GetAlternativeProfileImageUri", "http://example.com/%83%65%83%58%83%67_normal.jpeg", "mini")
            );
            // Complex paths
            Assert.Equal(
                new Uri("http://example.com//path1//path2/test_bigger.gif?test1=test2&test3=%83%65%83%58%83%674&test5=%E3%83%86%E3%82%B9%E3%83%886#example-section"),
                InvokeStatic<Uri>(
                    "GetAlternativeProfileImageUri",
                    "http://example.com//path1//path2/test_normal.gif?test1=test2&test3=%83%65%83%58%83%674&test5=%E3%83%86%E3%82%B9%E3%83%886#example-section",
                    "bigger"
                )
            );
        }

        [Fact]
        public void TestGetExtendedTweetElementsEmptyTweetText()
        {
            var json = LoadTestData("Status848569108350226434.json");
            var tweetInfo = JsonConvert.DeserializeObject<Status>(json).GetExtendedTweetElements();
            Assert.Equal(1, tweetInfo.HiddenPrefix.Length);
            Assert.Equal(0, tweetInfo.TweetText.Length);
            Assert.Equal(1, tweetInfo.HiddenSuffix.Length);
        }
    }
}
