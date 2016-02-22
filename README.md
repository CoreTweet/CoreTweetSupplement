# CoreTweetSupplement #
Provides useful extensions for client developers; made for [CoreTweet](http://coretweet.github.io/).

# How to Install #
If you use NuGet, run the following command:
```
PM> Install-Package CoreTweetSupplement
```

If not, add [one file](https://github.com/azyobuzin/CoreTweetSupplement/blob/master/CoreTweetSupplement/CoreTweetSupplement.cs) to your project.

# Methods #
## ParseSource(this Status) ##
This is a method to parse HTML-formated source field.
Returns a Source struct, which includes the name of the source app and the URI of that.

```csharp
StatusResponse status = token.Statuses.Show(id => 469320399126134784);
Source source = status.ParseSource();
//source.Name: "twicca"
//source.Href.AbsoluteUri: "http://twicca.r246.jp/"
```

## EnumerateTextParts(this Status/DirectMessage) ##
This is a method to split the text into Tweet Entities.
Enumerates order sorted and HTML-decoded parts of the text.

Example: [TestEnumerateTextParts](https://github.com/azyobuzin/CoreTweetSupplement/blob/f695b971fb2415180b7091bbc0b78280bda5e7ff/CoreTweetSupplementTest/CoreTweetSupplementTest.cs#L53-L332)

## GetProfileImageUrl / GetProfileImageUrlHttps(this User, string) ##
This is a method to get a URL pointing to the user's avatar image with given size.

Example: [TestAlternativeProfileImageUri](https://github.com/CoreTweet/CoreTweetSupplement/blob/0d805a0b2df96fe7f57e26dfb9b45a65e9f3ea12/CoreTweetSupplementTest/CoreTweetSupplementTest.cs#L345-L372)
