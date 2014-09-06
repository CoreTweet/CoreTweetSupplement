using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CoreTweet
{
    /// <summary>
    /// Provides extensions for objects of CoreTweet.
    /// </summary>
    public static class CoreTweetSupplement
    {
        private static string CharFromInt(int code)
        {
            if (code <= char.MaxValue) return ((char)code).ToString();

            code -= 0x10000;
            return new string(new[]
            {
                (char)((code / 0x400) + 0xD800),
                (char)((code % 0x400) + 0xDC00)
            });
        }

        private static string HtmlDecode(string source)
        {
            if (!source.Contains("&")) return source;
            var result = Regex.Replace(source, "&#([0-9]+);", match => CharFromInt(int.Parse(match.Groups[1].Value)));
            result = Regex.Replace(result, "&#x([0-9a-f]+);",
                match => CharFromInt(int.Parse(match.Groups[1].Value, NumberStyles.HexNumber)),
                RegexOptions.IgnoreCase
            );
            return result.Replace("&nbsp;", " ")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&amp;", "&")
                .Replace("&quot;", "\"")
                .Replace("&apos;", "'");
        }

        /// <summary>
        /// Parse source field.
        /// </summary>
        /// <param name="html">The content of source field.</param>
        /// <returns>A <see cref="CoreTweet.Source"/> instance.</returns>
        public static Source ParseSource(string html)
        {
            if (!html.StartsWith("<"))
                return new Source()
                {
                    Name = html
                };

            var match = Regex.Match(html, @"^<a href=""(.+)"" rel=""nofollow"">(.+)</a>$");
            return match.Success
                ? new Source()
                {
                    Name = HtmlDecode(match.Groups[2].Value),
                    Href = new Uri(HtmlDecode(match.Groups[1].Value))
                }
                : new Source()
                {
                    Name = html
                };
        }

        /// <summary>
        /// Parse the html of <see cref="CoreTweet.Status.Source"/>.
        /// </summary>
        /// <param name="status">The <see cref="CoreTweet.Status"/> instance.</param>
        /// <returns>A <see cref="CoreTweet.Source"/> instance.</returns>
        public static Source ParseSource(this Status status)
        {
            return ParseSource(status.Source);
        }

        private static IEnumerable<string> EnumerateChars(string str)
        {
            for (var i = 0; i < str.Length; i++)
            {
                if (char.IsSurrogatePair(str, i))
                {
                    yield return new string(new[] { str[i], str[++i] });
                }
                else
                {
                    yield return str[i].ToString();
                }
            }
        }

        private class TextPart : ITextPart
        {
            public TextPartType Type { get; set; }
            internal int Start { get; set; }
            internal int End { get; set; }
            public string RawText { get; set; }
            public string Text { get; set; }
            public Entity Entity { get; set; }
        }

        /// <summary>
        /// Enumerates parts split into Tweet Entities.
        /// </summary>
        /// <param name="text">The text such as <see cref="CoreTweet.Status.Text"/>, <see cref="CoreTweet.DirectMessage.Text"/> and <see cref="CoreTweet.User.Description"/>.</param>
        /// <param name="entities">The <see cref="CoreTweet.Entities"/> instance.</param>
        /// <returns>An <see cref="T:System.Collections.Generic.IEnumerable{CoreTweet.ITextPart}"/> whose elements are parts of <paramref name="text"/>.</returns>
        public static IEnumerable<ITextPart> EnumerateTextParts(string text, Entities entities)
        {
            if (entities == null)
            {
                yield return new TextPart()
                {
                    RawText = text,
                    Text = HtmlDecode(text)
                };
                yield break;
            }

            var list = new LinkedList<TextPart>(
                (entities.HashTags ?? Enumerable.Empty<SymbolEntity>())
                    .Select(e => new TextPart()
                    {
                        Type = TextPartType.Hashtag,
                        Start = e.Indices[0],
                        End = e.Indices[1],
                        RawText = "#" + e.Text,
                        Text = "#" + e.Text,
                        Entity = e
                    })
                    .Concat(
                        (entities.Symbols ?? Enumerable.Empty<SymbolEntity>())
                            .Select(e => new TextPart()
                            {
                                Type = TextPartType.Cashtag,
                                Start = e.Indices[0],
                                End = e.Indices[1],
                                RawText = "$" + e.Text,
                                Text = "$" + e.Text,
                                Entity = e
                            })
                    )
                    .Concat(
                        (entities.Urls ?? Enumerable.Empty<UrlEntity>())
                            .Concat(entities.Media ?? Enumerable.Empty<UrlEntity>())
                            .Select(e => new TextPart()
                            {
                                Type = TextPartType.Url,
                                Start = e.Indices[0],
                                End = e.Indices[1],
                                RawText = e.Url.ToString(),
                                Text = e.DisplayUrl,
                                Entity = e
                            })
                    )
                    .Concat(
                        (entities.UserMentions ?? Enumerable.Empty<UserMentionEntity>())
                            .Select(e => new TextPart()
                            {
                                Type = TextPartType.UserMention,
                                Start = e.Indices[0],
                                End = e.Indices[1],
                                RawText = "@" + e.ScreenName,
                                Text = "@" + e.ScreenName,
                                Entity = e
                            })
                    )
                    .OrderBy(part => part.Start)
            );

            if (list.Count == 0)
            {
                yield return new TextPart()
                {
                    RawText = text,
                    Text = HtmlDecode(text)
                };
                yield break;
            }

            var current = list.First;
            var chars = EnumerateChars(text).ToArray();

            while (true)
            {
                var start = current.Previous != null ? current.Previous.Value.End : 0;
                var count = current.Value.Start - start;
                if (count > 0)
                {
                    var output = string.Concat(chars.Skip(start).Take(count));
                    yield return new TextPart()
                    {
                        RawText = output,
                        Text = HtmlDecode(output)
                    };
                }

                yield return current.Value;

                if (current.Next == null) break;
                current = current.Next;
            }

            var lastStart = current.Value.End;
            if (lastStart < chars.Length)
            {
                var lastOutput = string.Concat(chars.Skip(lastStart));
                yield return new TextPart()
                {
                    RawText = lastOutput,
                    Text = HtmlDecode(lastOutput)
                };
            }
        }

        /// <summary>
        /// Enumerates parts split into Tweet Entities.
        /// </summary>
        /// <param name="status">The <see cref="CoreTweet.Status"/> instance.</param>
        /// <returns>An <see cref="T:System.Collections.Generic.IEnumerable{CoreTweet.ITextPart}"/> whose elements are parts of <paramref name="status"/>'s text.</returns>
        public static IEnumerable<ITextPart> EnumerateTextParts(this Status status)
        {
            return EnumerateTextParts(status.Text, status.Entities);
        }

        /// <summary>
        /// Enumerates parts split into Tweet Entities.
        /// </summary>
        /// <param name="dm">The <see cref="CoreTweet.DirectMessage"/> instance.</param>
        /// <returns>An <see cref="T:System.Collections.Generic.IEnumerable{CoreTweet.ITextPart}"/> whose elements are parts of <paramref name="dm"/>'s text.</returns>
        public static IEnumerable<ITextPart> EnumerateTextParts(this DirectMessage dm)
        {
            return EnumerateTextParts(dm.Text, dm.Entities);
        }

        /// <summary>
        /// Returns the URI suffix for given profile size image variant. See User Profile Images and Banners.
        /// </summary>
        /// <returns>The alternative URI suffix in profile image.</returns>
        /// <param name="size">Size of the image to obtain ("orig" to obtain the original size).</param>
        private static string GetAlternativeProfileImageUriSuffix(string size)
        {
            if (string.IsNullOrEmpty(size))
                return "";
            else if (string.Equals(size, "orig"))
                return "";
            else
                return "_" + size;
        }

        /// <summary>
        /// Returns the URI for given profile image URI and alternative size. See User Profile Images and Banners.
        /// </summary>
        /// <returns>The alternative profile image URI.</returns>
        /// <param name="uri">The original URI of <see cref="CoreTweet.User.ProfileImageUrl" /> or <see cref="CoreTweet.User.ProfileImageUrlHttps" />.</param>
        /// <param name="size">Size of the image to obtain ("orig" to obtain the original size).</param>
        private static Uri GetAlternativeProfileImageUri(Uri uri, string size)
        {
            var uriBuilder = new UriBuilder(uri);
            var path = uriBuilder.Path;
            int index = path.LastIndexOf("_normal", StringComparison.Ordinal);
            if (index < 0)
                return uri;
            var pathBuilder = new StringBuilder(path.Length);
            pathBuilder.Append(path, 0, index);
            pathBuilder.Append(GetAlternativeProfileImageUriSuffix(size));
            pathBuilder.Append(path, index + 7, path.Length - (index + 7));
            uriBuilder.Path = pathBuilder.ToString();
            return uriBuilder.Uri;
        }

        /// <summary>
        /// Gets a HTTP-based URL pointing to the user's avatar image with given size. See User Profile Images and Banners.
        /// </summary>
        /// <returns>The profile image URL.</returns>
        /// <param name="user">A <see cref="CoreTweet.User"/> instance.</param>
        /// <param name="size">Size of the image to obtain ("orig" to obtain the original size).</param>
        public static Uri GetProfileImageUrl(this User user, string size = "normal")
        {
            return GetAlternativeProfileImageUri(user.ProfileImageUrl, size);
        }

        /// <summary>
        /// Gets a HTTPS-based URL pointing to the user's avatar image with given size. See User Profile Images and Banners.
        /// </summary>
        /// <returns>The profile image URL.</returns>
        /// <param name="user">A <see cref="CoreTweet.User"/> instance.</param>
        /// <param name="size">Size of the image to obtain ("orig" to obtain the original size).</param>
        public static Uri GetProfileImageUrlHttps(this User user, string size = "normal")
        {
            return GetAlternativeProfileImageUri(user.ProfileImageUrlHttps, size);
        }
    }

    /// <summary>
    /// Resolved source field
    /// </summary>
    public struct Source
    {
        /// <summary>
        /// The name of the client which the user tweeted with.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The URL of the client witch the user tweeted with.
        /// </summary>
        public Uri Href { get; set; }
    }

    /// <summary>
    /// Types of <see cref="CoreTweet.ITextPart"/>.
    /// </summary>
    public enum TextPartType
    {
        /// <summary>
        /// Plain text, which is related to no entity.
        /// <see cref="CoreTweet.ITextPart.Entity"/> will be <c>null</c>.
        /// </summary>
        Plain,

        /// <summary>
        /// Hashtag.
        /// <see cref="CoreTweet.ITextPart.Entity"/> will be a <see cref="CoreTweet.SymbolEntity" /> instance.
        /// </summary>
        Hashtag,

        /// <summary>
        /// Cashtag.
        /// <see cref="CoreTweet.ITextPart.Entity"/> will be a <see cref="CoreTweet.SymbolEntity" /> instance.
        /// </summary>
        Cashtag,

        /// <summary>
        /// URL.
        /// <see cref="CoreTweet.ITextPart.Entity"/> will be a <see cref="CoreTweet.UrlEntity" /> instance.
        /// </summary>
        Url,

        /// <summary>
        /// User mention.
        /// <see cref="CoreTweet.ITextPart.Entity"/> will be a <see cref="CoreTweet.UserMentionEntity" /> instance.
        /// </summary>
        UserMention
    }

    /// <summary>
    /// A part of text.
    /// </summary>
    public interface ITextPart
    {
        /// <summary>
        /// The type of this instance.
        /// </summary>
        TextPartType Type { get; }

        /// <summary>
        /// The raw text.
        /// </summary>
        string RawText { get; }

        /// <summary>
        /// The decoded text.
        /// </summary>
        string Text { get; }

        /// <summary>
        /// The base entity information.
        /// </summary>
        Entity Entity { get; }
    }
}
