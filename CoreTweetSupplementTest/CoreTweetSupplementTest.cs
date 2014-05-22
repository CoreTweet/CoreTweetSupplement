using System;
using System.Collections.Generic;
using System.Linq;
using CoreTweet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace CoreTweetSupplementTest
{
    [TestClass]
    public class CoreTweetSupplementTest
    {
        private static readonly PrivateType TestTarget = new PrivateType(typeof(CoreTweetSupplement));

        [TestMethod]
        public void TestCharFromInt()
        {
            TestTarget.InvokeStatic("CharFromInt", 97).Is("a");
            TestTarget.InvokeStatic("CharFromInt", 0x20B9F).Is("𠮟");
        }

        [TestMethod]
        public void TestHtmlDecode()
        {
            TestTarget.InvokeStatic("HtmlDecode", "test&lt;&gt;a&amp;&quot;&apos;&#9834;&#x266A;0")
                .Is("test<>a&\"'♪♪0");
        }

        [TestMethod]
        public void TestParseSource()
        {
            CoreTweetSupplement.ParseSource(@"<a href=""http://twitter.com/download/iphone"" rel=""nofollow"">Twitter for iPhone</a>")
                .IsStructuralEqual(new Source()
                {
                    Name = "Twitter for iPhone",
                    Href = new Uri("http://twitter.com/download/iphone")
                });
            CoreTweetSupplement.ParseSource("web")
                .IsStructuralEqual(new Source()
                {
                    Name = "web",
                    Href = null
                });
        }

        [TestMethod]
        public void TestEnumerateChars()
        {
            ((IEnumerable<string>)TestTarget.InvokeStatic("EnumerateChars", "𠮷野家こそ至高!"))
                .Is("𠮷", "野", "家", "こ", "そ", "至", "高", "!");
        }

        [TestMethod]
        public void TestEnumerateTextParts()
        {
            var array = CoreTweetSupplement.EnumerateTextParts("test", null).ToArray();
            array.Length.Is(1);
            array[0].Type.Is(TextPartType.Plain);
            array[0].RawText.Is("test");
            array[0].Text.Is("test");
            array[0].Entity.IsNull();

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
            array.Length.Is(4);
            array[0].Type.Is(TextPartType.Plain);
            array[0].RawText.Is("てすとtest𠮷野家 #𠮷野家 aa ");
            array[0].Text.Is("てすとtest𠮷野家 #𠮷野家 aa ");
            array[0].Entity.IsNull();
            array[1].Type.Is(TextPartType.UserMention);
            array[1].RawText.Is("@azyobuzin_test");
            array[1].Text.Is("@azyobuzin_test");
            array[1].Entity.IsNotNull();
            array[2].Type.Is(TextPartType.Plain);
            array[2].RawText.Is(" ");
            array[2].Text.Is(" ");
            array[2].Entity.IsNull();
            array[3].Type.Is(TextPartType.Url);
            array[3].RawText.Is("http://t.co/KmtlVpXaUN");
            array[3].Text.Is("pic.twitter.com/KmtlVpXaUN");
            array[3].Entity.IsNotNull();

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
            array.Length.Is(3);
            array[0].Type.Is(TextPartType.Plain);
            array[0].RawText.Is("ってって ");
            array[0].Text.Is("ってって ");
            array[0].Entity.IsNull();
            array[1].Type.Is(TextPartType.Hashtag);
            array[1].RawText.Is("#test");
            array[1].Text.Is("#test");
            array[1].Entity.IsNotNull();
            array[2].Type.Is(TextPartType.Plain);
            array[2].RawText.Is(" &amp;&lt;てすと&gt;&amp;&amp;amp;");
            array[2].Text.Is(" &<てすと>&&amp;");
            array[2].Entity.IsNull();
        }
    }
}
