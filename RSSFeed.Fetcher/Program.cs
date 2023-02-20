using Microsoft.EntityFrameworkCore;
using RSSFeed.Api.Db;
using RSSFeed.Fetcher.Services;

var optionBuilder = new DbContextOptionsBuilder<AppDbContext>();

var db = new AppDbContext(optionBuilder.Options);

optionBuilder.UseSqlServer("Server=LocalHost;Database=rssfeedaggregatorapi_db;Trusted_Connection=true;MultipleActiveResultSets=True;Encrypt=False;");

var newsfeedService = new NewsFeedService(db);

List<string> feedUrl = new List<string>
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

await newsfeedService.FetchAndSaveArticlesAsync(feedUrl);

Console.WriteLine("Parse Is Done");