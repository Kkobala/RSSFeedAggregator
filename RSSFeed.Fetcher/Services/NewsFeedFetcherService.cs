using Microsoft.EntityFrameworkCore;
using RSSFeed.Api.Db;
using RSSFeed.Api.Db.Entities;
using System.ServiceModel.Syndication;
using System.Xml;

namespace RSSFeed.Fetcher.Services
{
    public class NewsFeedFetcherService
    {
        private readonly AppDbContext _db;

        public NewsFeedFetcherService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<SyndicationFeed> LoadFeedAsync(string feedUrl)
        {
            var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("User-Agent", "MyUserAgent");

            var response = await httpClient.GetStringAsync(feedUrl);

            var stringReader = new StringReader(response);

            var reader = XmlReader.Create(stringReader);

            return SyndicationFeed.Load(reader);
        }

        public async Task FetchAndSaveArticlesAsync()
        {
            List<string> feedUrls = new List<string>
            {
                 "https://stackoverflow.blog/feed/",
                 "https://dev.to/feed",
                 "https://www.freecodecamp.org/news/rss",
                 "https://martinfowler.com/feed.atom",
                 "https://codeblog.jonskeet.uk/feed/",
                 "https://devblogs.microsoft.com/visualstudio/feed/",
                 "https://feed.infoq.com/",
                 "https://css-tricks.com/feed/",
                 "https://codeopinion.com/feed/",
                 "https://andrewlock.net/rss.xml",
                 "https://michaelscodingspot.com/index.xml",
                 "https://www.tabsoverspaces.com/feed.xml"
            };

            foreach (var feedUrl in feedUrls)
            {
                var feed = await LoadFeedAsync(feedUrl);
                foreach (var item in feed.Items)
                {
                    var feedTitle = item.Title.Text;
                    var feedLink = item.Links.FirstOrDefault()?.Uri.ToString();

                    if (await FeedExistsAsync(feedUrl, feedTitle))
                    {
                        continue;
                    }

                    var feedEntity = new FeedEntity
                    {
                        Link = item.Links.FirstOrDefault()?.Uri.ToString(),
                        Title = item.Title.Text,
                        Description = item.Summary?.Text,
                        Author = item.Authors.FirstOrDefault()?.Name,
                        Picture = item.Links.FirstOrDefault(l => l.MediaType == "image/jpeg")?.Uri.ToString(),
                        Tags = string.Join(",", item.Categories.Select(c => c.Name)),
                        PublishedDate = item.PublishDate.UtcDateTime
                    };

                    await CategorizeAndSaveFeedAsync(feedEntity);
                }
            }
        }

        private async Task CategorizeAndSaveFeedAsync(FeedEntity feed)
        {
            feed.Title = RemoveJavascriptCode(feed.Title!);
            feed.Description = RemoveJavascriptCode(feed.Description!);
            feed.Author = RemoveJavascriptCode(feed.Author!);
            feed.Tags = RemoveJavascriptCode(feed.Tags!);

            var existingTags = await _db.Tags
                .Where(t => feed.Title.Contains(t.Name!) || feed.Description.Contains(t.Name!))
                .ToListAsync();

            foreach (var tag in existingTags)
            {
                if (!feed.Tags.Contains(tag.Name!))
                {
                    if (!string.IsNullOrEmpty(feed.Tags))
                    {
                        feed.Tags += ",";
                    }

                    feed.Tags += tag.Name;
                }
            }

            var existingArticle = await _db.FeedEntities
                .FirstOrDefaultAsync(a => a.Link == feed.Link);

            if (existingArticle == null)
            {
                _db.FeedEntities.Add(feed);
                await _db.SaveChangesAsync();
            }
        }

        private string RemoveJavascriptCode(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(text);
            return doc.DocumentNode.InnerText;
        }

        public async Task<bool> FeedExistsAsync(string feedUrl, string feedTitle)
        {
            return await _db.FeedEntities
                .AnyAsync(a => a.Link!.StartsWith(feedUrl) && a.Title!.ToLower() == feedTitle.ToLower());
        }
    }
}
