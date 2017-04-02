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
        private static string CharFromInt(uint code)
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
            if (source.IndexOf('&') == -1) return source;
            var sb = new StringBuilder(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                int semicolonIndex;
                if (source[i] != '&'
                    || (semicolonIndex = source.IndexOf(';', i + 3)) == -1)
                {
                    sb.Append(source[i]);
                    continue;
                }

                var s = source.Substring(i + 1, semicolonIndex - i - 1);
                switch (s)
                {
                    case "nbsp": sb.Append(' '); break;
                    case "lt": sb.Append('<'); break;
                    case "gt": sb.Append('>'); break;
                    case "amp": sb.Append('&'); break;
                    case "quot": sb.Append('"'); break;
                    case "apos": sb.Append('\''); break;
                    default:
                        if (s[0] == '#')
                        {
                            var code = s[1] == 'x'
                                ? uint.Parse(s.Substring(2), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo)
                                : uint.Parse(s.Substring(1), NumberFormatInfo.InvariantInfo);
                            sb.Append(CharFromInt(code));
                        }
                        else
                        {
                            sb.Append('&').Append(s).Append(';');
                        }
                        break;
                }

                i = semicolonIndex;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Parse source field.
        /// </summary>
        /// <param name="html">The content of source field.</param>
        /// <returns>A <see cref="Source"/> instance.</returns>
        public static Source ParseSource(string html)
        {
            if (!html.StartsWith("<", StringComparison.Ordinal))
                return new Source()
                {
                    Name = html
                };

            var match = Regex.Match(html, @"^<a href=""(.+)"" rel=""nofollow"">(.+)</a>$");
            return match.Success
                ? new Source()
                {
                    Name = HtmlDecode(match.Groups[2].Value),
                    Href = HtmlDecode(match.Groups[1].Value)
                }
                : new Source()
                {
                    Name = html
                };
        }

        /// <summary>
        /// Parse the html of <see cref="Status.Source"/>.
        /// </summary>
        /// <param name="status">The <see cref="Status"/> instance.</param>
        /// <returns>A <see cref="Source"/> instance.</returns>
        public static Source ParseSource(this Status status)
        {
            return ParseSource(status.Source);
        }

        private static List<DoubleUtf16Char> GetCodePoints(string str)
        {
            var result = new List<DoubleUtf16Char>(str.Length);
            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                result.Add(char.IsHighSurrogate(c)
                    ? new DoubleUtf16Char(c, str[++i])
                    : new DoubleUtf16Char(c));
            }
            return result;
        }

        private static string ToString(IList<DoubleUtf16Char> source, int start, int count)
        {
            var arr = new char[count * 2];
            var end = start + count;
            var strLen = 0;
            for (var i = start; i < end; i++)
            {
                var x = source[i];
                arr[strLen++] = x.X;
                if (char.IsHighSurrogate(x.X))
                    arr[strLen++] = x.Y;
            }
            return new string(arr, 0, strLen);
        }

        /// <summary>
        /// Enumerates parts split into Tweet Entities.
        /// </summary>
        /// <param name="text">The text such as <see cref="Status.Text"/>, <see cref="DirectMessage.Text"/> and <see cref="User.Description"/>.</param>
        /// <param name="entities">The <see cref="Entities"/> instance.</param>
        /// <returns>An <see cref="IEnumerable{TextPart}"/> whose elements are parts of <paramref name="text"/>.</returns>
        public static IEnumerable<TextPart> EnumerateTextParts(string text, Entities entities)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var chars = GetCodePoints(text);
            return EnumerateTextParts(chars, entities, 0, chars.Count);
        }

        /// <summary>
        /// Enumerates parts split into Tweet Entities.
        /// </summary>
        /// <param name="text">The text such as <see cref="Status.Text"/>, <see cref="DirectMessage.Text"/> and <see cref="User.Description"/>.</param>
        /// <param name="entities">The <see cref="Entities"/> instance.</param>
        /// <param name="startIndex">The starting character position in code point.</param>
        /// <param name="endIndex">The ending character position in code point.</param>
        /// <returns>An <see cref="IEnumerable{TextPart}"/> whose elements are parts of <paramref name="text"/>.</returns>
        public static IEnumerable<TextPart> EnumerateTextParts(string text, Entities entities, int startIndex, int endIndex)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var chars = GetCodePoints(text);

            if (startIndex < 0 || startIndex >= chars.Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (endIndex < startIndex || endIndex > chars.Count)
                throw new ArgumentOutOfRangeException(nameof(endIndex));

            return EnumerateTextParts(chars, entities, startIndex, endIndex);
        }

        private static IEnumerable<TextPart> EnumerateTextParts(IList<DoubleUtf16Char> chars, Entities entities, int startIndex, int endIndex)
        {
            if (startIndex == endIndex) yield break;

            if (entities == null)
            {
                var text = ToString(chars, startIndex, endIndex - startIndex);
                yield return new TextPart()
                {
                    RawText = text,
                    Text = HtmlDecode(text)
                };
                yield break;
            }

            var media = entities.Media ?? Enumerable.Empty<UrlEntity>();
            if (entities.Urls != null)
            {
                // Remove duplicate entities in DM
                media = media.Where(e => !entities.Urls.Any(x => x.Indices[0] == e.Indices[0]));
            }

            var list = new LinkedList<TextPart>(
                (entities.HashTags ?? Enumerable.Empty<HashtagEntity>())
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
                        (entities.Symbols ?? Enumerable.Empty<CashtagEntity>())
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
                            .Concat(media)
                            .Select(e => new TextPart()
                            {
                                Type = TextPartType.Url,
                                Start = e.Indices[0],
                                End = e.Indices[1],
                                RawText = e.Url,
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
                    .Where(e => e.Start >= startIndex && e.Start < endIndex)
                    .OrderBy(part => part.Start)
            );

            if (list.Count == 0)
            {
                var text = ToString(chars, startIndex, endIndex - startIndex);
                yield return new TextPart()
                {
                    RawText = text,
                    Text = HtmlDecode(text)
                };
                yield break;
            }

            var current = list.First;

            while (true)
            {
                var start = current.Previous?.Value.End ?? startIndex;
                var count = current.Value.Start - start;
                if (count > 0)
                {
                    var output = ToString(chars, start, count);
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
            if (lastStart < endIndex)
            {
                var lastOutput = ToString(chars, lastStart, endIndex - lastStart);
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
        /// <param name="status">The <see cref="Status"/> instance.</param>
        /// <returns>An <see cref="IEnumerable{ITextPart}"/> whose elements are parts of <paramref name="status"/>'s text.</returns>
        public static IEnumerable<TextPart> EnumerateTextParts(this Status status)
        {
            return EnumerateTextParts(status.Text, status.Entities);
        }

        /// <summary>
        /// Enumerates parts split into Tweet Entities.
        /// </summary>
        /// <param name="dm">The <see cref="DirectMessage"/> instance.</param>
        /// <returns>An <see cref="IEnumerable{ITextPart}"/> whose elements are parts of <paramref name="dm"/>'s text.</returns>
        public static IEnumerable<TextPart> EnumerateTextParts(this DirectMessage dm)
        {
            return EnumerateTextParts(dm.Text, dm.Entities);
        }

        /// <summary>
        /// Enumerates parts split into Tweet Entities.
        /// </summary>
        /// <param name="status">The <see cref="Status"/> instance.</param>
        /// <returns>An <see cref="ExtendedTweetInfo"/> instance.</returns>
        public static ExtendedTweetInfo GetExtendedTweetElements(this Status status)
        {
            var displayTextRange = status.DisplayTextRange ?? status.ExtendedTweet?.DisplayTextRange;
            if (displayTextRange == null)
                return new ExtendedTweetInfo
                {
                    TweetText = status.EnumerateTextParts().ToArray(),
                    HiddenPrefix = new UserMentionEntity[0],
                    HiddenSuffix = new UrlEntity[0]
                };

            var start = displayTextRange[0];
            var end = displayTextRange[1];

            var entities = status.ExtendedTweet?.Entities ?? status.Entities;

            return new ExtendedTweetInfo
            {
                TweetText = EnumerateTextParts(
                    status.FullText ?? status.ExtendedTweet?.FullText ?? status.Text,
                    entities,
                    start, end).ToArray(),
                HiddenPrefix = entities?.UserMentions == null ? new UserMentionEntity[0]
                    : entities.UserMentions.Where(x => x.Indices[0] < start).ToArray(),
                HiddenSuffix = (entities?.Urls ?? Enumerable.Empty<UrlEntity>())
                    .Concat(entities?.Media ?? Enumerable.Empty<UrlEntity>())
                    .Where(x => x.Indices[0] >= end).ToArray()
            };
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
            if (string.Equals(size, "orig"))
                return "";
            return "_" + size;
        }

        /// <summary>
        /// Returns the URI for given profile image URI and alternative size. See User Profile Images and Banners.
        /// </summary>
        /// <returns>The alternative profile image URI.</returns>
        /// <param name="uri">The original URI of <see cref="User.ProfileImageUrl" /> or <see cref="User.ProfileImageUrlHttps" />.</param>
        /// <param name="size">Size of the image to obtain ("orig" to obtain the original size).</param>
        private static Uri GetAlternativeProfileImageUri(string uri, string size)
        {
            var uriBuilder = new UriBuilder(uri);
            var path = uriBuilder.Path;
            int index = path.LastIndexOf("_normal", StringComparison.Ordinal);
            if (index < 0)
                return uriBuilder.Uri;
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
        /// <param name="user">A <see cref="User"/> instance.</param>
        /// <param name="size">Size of the image to obtain ("orig" to obtain the original size).</param>
        public static Uri GetProfileImageUrl(this User user, string size = "normal")
        {
            return GetAlternativeProfileImageUri(user.ProfileImageUrl, size);
        }

        /// <summary>
        /// Gets a HTTPS-based URL pointing to the user's avatar image with given size. See User Profile Images and Banners.
        /// </summary>
        /// <returns>The profile image URL.</returns>
        /// <param name="user">A <see cref="User"/> instance.</param>
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
        public string Href { get; set; }
    }

    /// <summary>
    /// Types of <see cref="TextPart"/>.
    /// </summary>
    public enum TextPartType
    {
        /// <summary>
        /// Plain text, which is related to no entity.
        /// <see cref="TextPart.Entity"/> will be <c>null</c>.
        /// </summary>
        Plain,

        /// <summary>
        /// Hashtag.
        /// <see cref="TextPart.Entity"/> will be a <see cref="HashtagEntity" /> instance.
        /// </summary>
        Hashtag,

        /// <summary>
        /// Cashtag.
        /// <see cref="TextPart.Entity"/> will be a <see cref="CashtagEntity" /> instance.
        /// </summary>
        Cashtag,

        /// <summary>
        /// URL.
        /// <see cref="TextPart.Entity"/> will be a <see cref="UrlEntity" /> instance.
        /// </summary>
        Url,

        /// <summary>
        /// User mention.
        /// <see cref="TextPart.Entity"/> will be a <see cref="UserMentionEntity" /> instance.
        /// </summary>
        UserMention
    }

    /// <summary>
    /// A part of text.
    /// </summary>
    public class TextPart
    {
        /// <summary>
        /// The type of this instance.
        /// </summary>
        public TextPartType Type { get; set; }

        internal int Start { get; set; }
        internal int End { get; set; }

        /// <summary>
        /// The raw text.
        /// </summary>
        public string RawText { get; set; }

        /// <summary>
        /// The decoded text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The base entity information.
        /// </summary>
        public Entity Entity { get; set; }
    }

    internal struct DoubleUtf16Char
    {
        public char X;
        public char Y;

        public DoubleUtf16Char(char x)
        {
            this.X = x;
            this.Y = default(char);
        }

        public DoubleUtf16Char(char x, char y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    /// <summary>
    /// Represents a Tweet rendered in Extended mode.
    /// </summary>
    public class ExtendedTweetInfo
    {
        /// <summary>
        /// The elements of Tweet Text part.
        /// </summary>
        public TextPart[] TweetText { get; set; }

        /// <summary>
        /// Replies.
        /// </summary>
        public UserMentionEntity[] HiddenPrefix { get; set; }

        /// <summary>
        /// Attachment URLs.
        /// </summary>
        public UrlEntity[] HiddenSuffix { get; set; }
    }
}
