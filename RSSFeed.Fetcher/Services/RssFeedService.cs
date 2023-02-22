using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using RSSFeed.Api.Db;
using RSSFeed.Api.Db.Entities;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml;

namespace RSSFeed.Fetcher.Services
{
    public class RssFeedService
    {
        private readonly AppDbContext _db;

        public RssFeedService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<SyndicationFeed>> LoadFeedAsync(List<string> strXML)
        {
            var tasks = strXML.Select(async url =>
            {
                using (var http = new HttpClient())
                {
                    http.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (platform; rv:geckoversion) Gecko/geckotrail Firefox/firefoxversion");
                    using (var result = await http.GetAsync(url))
                    {
                        var xml = await result.Content.ReadAsStringAsync();
                        using (var stringReader = new StringReader(xml))
                        using (var xmlReader = XmlReader.Create(stringReader))
                        {
                            var feed = SyndicationFeed.Load(xmlReader);
                            // The Load method may close the XmlReader, so we need to create a new one
                            return SyndicationFeed.Load(XmlReader.Create(new StringReader(xml)));
                        }
                    }
                }
            });

            return (await Task.WhenAll(tasks)).ToList();
        }

        private static string SanitizeText(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedAttributes.Add("class");
            sanitizer.AllowedTags.Remove("script");
            sanitizer.AllowedTags.Remove("style");
            return sanitizer.Sanitize(input);
        }

        public async Task FetchAndSaveArticlesAsync(List<string> feedUrl)
        {
            var feeds = await LoadFeedAsync(feedUrl);

            var tags = await _db.Tags.ToListAsync();

            foreach (var feed in feeds)
            {
                foreach (var item in feed.Items)
                {
                    var title = SanitizeText(item.Title.Text);
                    var description = item.Summary?.Text;
                    var parseddescription = Regex.Replace(description!, "<script.*?</script>", "", RegexOptions.Singleline).Trim();

                    var existingArticle = await _db.FeedEntities
                        .Where(a => a.Title == title && a.Link.Contains(feed.Links[0].Uri.Host))
                        .FirstOrDefaultAsync();

                    if (existingArticle == null)
                    {
                        var exsitingTag = tags.Where(t => title.Contains(t.Name) || parseddescription.Contains(t.Name)).ToList();

                        var feedentity = new FeedEntity
                        {
                            Link = item.Links.Count > 0 ? item.Links[0].Uri.ToString() : null,
                            Title = title,
                            Description = parseddescription,
                            Author = item.Authors.Count > 0 ? SanitizeText(item.Authors[0].Name) : null,
                            Picture = item.Links.FirstOrDefault(l => l.MediaType == "image/jpeg")?.Uri.ToString(),
                            Tags = SanitizeText(string.Join(",", item.Categories.Select(c => c.Name))),
                            PublishedDate = item.PublishDate.UtcDateTime
                        };

                        lock (_db.Tags)
                        {
                            foreach (var category in item.Categories)
                            {
                                if (!tags.Any(t => t.Name == category.Name))
                                {
                                    var tag = new TagsEntity
                                    {
                                        Name = category.Name
                                    };

                                    _db.Tags.Add(tag);
                                    exsitingTag.Add(tag);
                                }
                            }
                        }

                        await _db.SaveChangesAsync();

                        feedentity.Tags += "," + string.Join(",", exsitingTag.Select(t => t.Name));

                        _db.FeedEntities.Add(feedentity);
                        await _db.SaveChangesAsync();
                    }
                }
            }
        }

    }
}
