using System;
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
        public void TestEnumerateTextParts()
        {
            var array = CoreTweetSupplement.EnumerateTextParts("test", null).ToArray();
            Assert.Equal(1, array.Length);
            Assert.Equal(TextPartType.Plain, array[0].Type);
            Assert.Equal("test", array[0].RawText);
            Assert.Equal("test", array[0].Text);
            Assert.Null(array[0].Entity);

            #region Case 1
            const string case1 = @"{
  ""created_at"": ""Wed May 21 09:57:01 +0000 2014"",
  ""id"": 469054246503989250,
  ""id_str"": ""469054246503989248"",
  ""text"": ""てすとtest𠮷野家 #𠮷野家 aa @azyobuzin_test http://t.co/KmtlVpXaUN"",
  ""source"": ""<a href=\""http://azyobuzi.net/\"" rel=\""nofollow\"">ひゅいっ</a>"",
  ""truncated"": false,
  ""in_reply_to_status_id"": null,
  ""in_reply_to_status_id_str"": null,
  ""in_reply_to_user_id"": null,
  ""in_reply_to_user_id_str"": null,
  ""in_reply_to_screen_name"": null,
  ""user"": {
    ""id"": 265093120,
    ""id_str"": ""265093120"",
    ""name"": ""ぺぺたろう（テストなーーう）"",
    ""screen_name"": ""azyobuzin_test"",
    ""location"": ""@azyobuzinの脳の一部"",
    ""description"": ""@azyobuzinのテスト用アカウント"",
    ""url"": ""https://t.co/Fmkl9K35Fu"",
    ""entities"": {
      ""url"": {
        ""urls"": [
          {
            ""url"": ""https://t.co/Fmkl9K35Fu"",
            ""expanded_url"": ""https://twitter.com/azyobuzin"",
            ""display_url"": ""twitter.com/azyobuzin"",
            ""indices"": [
              0,
              23
            ]
          }
        ]
      },
      ""description"": {
        ""urls"": []
      }
    },
    ""protected"": false,
    ""followers_count"": 6,
    ""friends_count"": 2,
    ""listed_count"": 1,
    ""created_at"": ""Sun Mar 13 00:52:42 +0000 2011"",
    ""favourites_count"": 14,
    ""utc_offset"": 32400,
    ""time_zone"": ""Tokyo"",
    ""geo_enabled"": true,
    ""verified"": false,
    ""statuses_count"": 84,
    ""lang"": ""ja"",
    ""contributors_enabled"": false,
    ""is_translator"": false,
    ""is_translation_enabled"": false,
    ""profile_background_color"": ""C0DEED"",
    ""profile_background_image_url"": ""http://abs.twimg.com/images/themes/theme1/bg.png"",
    ""profile_background_image_url_https"": ""https://abs.twimg.com/images/themes/theme1/bg.png"",
    ""profile_background_tile"": false,
    ""profile_image_url"": ""http://abs.twimg.com/sticky/default_profile_images/default_profile_5_normal.png"",
    ""profile_image_url_https"": ""https://abs.twimg.com/sticky/default_profile_images/default_profile_5_normal.png"",
    ""profile_link_color"": ""0084B4"",
    ""profile_sidebar_border_color"": ""C0DEED"",
    ""profile_sidebar_fill_color"": ""DDEEF6"",
    ""profile_text_color"": ""333333"",
    ""profile_use_background_image"": true,
    ""default_profile"": true,
    ""default_profile_image"": true,
    ""following"": true,
    ""follow_request_sent"": false,
    ""notifications"": false
  },
  ""geo"": null,
  ""coordinates"": null,
  ""place"": null,
  ""contributors"": null,
  ""retweet_count"": 0,
  ""favorite_count"": 0,
  ""entities"": {
    ""hashtags"": [],
    ""symbols"": [],
    ""urls"": [],
    ""user_mentions"": [
      {
        ""screen_name"": ""azyobuzin_test"",
        ""name"": ""ぺぺたろう（テストなーーう）"",
        ""id"": 265093120,
        ""id_str"": ""265093120"",
        ""indices"": [
          19,
          34
        ]
      }
    ],
    ""media"": [
      {
        ""id"": 469054246201991200,
        ""id_str"": ""469054246201991168"",
        ""indices"": [
          35,
          57
        ],
        ""media_url"": ""http://pbs.twimg.com/media/BoJqWg1CIAAn2KB.png"",
        ""media_url_https"": ""https://pbs.twimg.com/media/BoJqWg1CIAAn2KB.png"",
        ""url"": ""http://t.co/KmtlVpXaUN"",
        ""display_url"": ""pic.twitter.com/KmtlVpXaUN"",
        ""expanded_url"": ""http://twitter.com/azyobuzin_test/status/469054246503989248/photo/1"",
        ""type"": ""photo"",
        ""sizes"": {
          ""large"": {
            ""w"": 616,
            ""h"": 485,
            ""resize"": ""fit""
          },
          ""thumb"": {
            ""w"": 150,
            ""h"": 150,
            ""resize"": ""crop""
          },
          ""small"": {
            ""w"": 339,
            ""h"": 267,
            ""resize"": ""fit""
          },
          ""medium"": {
            ""w"": 599,
            ""h"": 472,
            ""resize"": ""fit""
          }
        }
      }
    ]
  },
  ""favorited"": false,
  ""retweeted"": false,
  ""possibly_sensitive"": false,
  ""lang"": ""ja""
}";
            #endregion

            array = JsonConvert.DeserializeObject<Status>(case1).EnumerateTextParts().ToArray();
            Assert.Equal(4, array.Length);
            Assert.Equal(TextPartType.Plain, array[0].Type);
            Assert.Equal("てすとtest𠮷野家 #𠮷野家 aa ", array[0].RawText);
            Assert.Equal("てすとtest𠮷野家 #𠮷野家 aa ", array[0].Text);
            Assert.Null(array[0].Entity);
            Assert.Equal(TextPartType.UserMention, array[1].Type);
            Assert.Equal("@azyobuzin_test", array[1].RawText);
            Assert.Equal("@azyobuzin_test", array[1].Text);
            Assert.NotNull(array[1].Entity);
            Assert.Equal(TextPartType.Plain, array[2].Type);
            Assert.Equal(" ", array[2].RawText);
            Assert.Equal(" ", array[2].Text);
            Assert.Null(array[2].Entity);
            Assert.Equal(TextPartType.Url, array[3].Type);
            Assert.Equal("http://t.co/KmtlVpXaUN", array[3].RawText);
            Assert.Equal("pic.twitter.com/KmtlVpXaUN", array[3].Text);
            Assert.NotNull(array[3].Entity);

            #region Case 2
            const string case2 = @"{
  ""created_at"": ""Wed May 21 10:16:15 +0000 2014"",
  ""id"": 469059084289708000,
  ""id_str"": ""469059084289708032"",
  ""text"": ""ってって #test &amp;&lt;てすと&gt;&amp;&amp;amp;"",
  ""source"": ""<a href=\""http://azyobuzi.net/\"" rel=\""nofollow\"">ひゅいっ</a>"",
  ""truncated"": false,
  ""in_reply_to_status_id"": null,
  ""in_reply_to_status_id_str"": null,
  ""in_reply_to_user_id"": null,
  ""in_reply_to_user_id_str"": null,
  ""in_reply_to_screen_name"": null,
  ""user"": {
    ""id"": 265093120,
    ""id_str"": ""265093120"",
    ""name"": ""ぺぺたろう（テストなーーう）"",
    ""screen_name"": ""azyobuzin_test"",
    ""location"": ""@azyobuzinの脳の一部"",
    ""description"": ""@azyobuzinのテスト用アカウント"",
    ""url"": ""https://t.co/Fmkl9K35Fu"",
    ""entities"": {
      ""url"": {
        ""urls"": [
          {
            ""url"": ""https://t.co/Fmkl9K35Fu"",
            ""expanded_url"": ""https://twitter.com/azyobuzin"",
            ""display_url"": ""twitter.com/azyobuzin"",
            ""indices"": [
              0,
              23
            ]
          }
        ]
      },
      ""description"": {
        ""urls"": []
      }
    },
    ""protected"": false,
    ""followers_count"": 6,
    ""friends_count"": 2,
    ""listed_count"": 1,
    ""created_at"": ""Sun Mar 13 00:52:42 +0000 2011"",
    ""favourites_count"": 14,
    ""utc_offset"": 32400,
    ""time_zone"": ""Tokyo"",
    ""geo_enabled"": true,
    ""verified"": false,
    ""statuses_count"": 85,
    ""lang"": ""ja"",
    ""contributors_enabled"": false,
    ""is_translator"": false,
    ""is_translation_enabled"": false,
    ""profile_background_color"": ""C0DEED"",
    ""profile_background_image_url"": ""http://abs.twimg.com/images/themes/theme1/bg.png"",
    ""profile_background_image_url_https"": ""https://abs.twimg.com/images/themes/theme1/bg.png"",
    ""profile_background_tile"": false,
    ""profile_image_url"": ""http://abs.twimg.com/sticky/default_profile_images/default_profile_5_normal.png"",
    ""profile_image_url_https"": ""https://abs.twimg.com/sticky/default_profile_images/default_profile_5_normal.png"",
    ""profile_link_color"": ""0084B4"",
    ""profile_sidebar_border_color"": ""C0DEED"",
    ""profile_sidebar_fill_color"": ""DDEEF6"",
    ""profile_text_color"": ""333333"",
    ""profile_use_background_image"": true,
    ""default_profile"": true,
    ""default_profile_image"": true,
    ""following"": true,
    ""follow_request_sent"": false,
    ""notifications"": false
  },
  ""geo"": null,
  ""coordinates"": null,
  ""place"": null,
  ""contributors"": null,
  ""retweet_count"": 0,
  ""favorite_count"": 0,
  ""entities"": {
    ""hashtags"": [
      {
        ""text"": ""test"",
        ""indices"": [
          5,
          10
        ]
      }
    ],
    ""symbols"": [],
    ""urls"": [],
    ""user_mentions"": []
  },
  ""favorited"": false,
  ""retweeted"": false,
  ""lang"": ""ja""
}";
            #endregion

            array = JsonConvert.DeserializeObject<Status>(case2).EnumerateTextParts().ToArray();
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
    }
}
