using Microsoft.EntityFrameworkCore;
using RSSFeed.Api.Db;
using RSSFeed.Fetcher.Services;

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseSqlServer("Server=LocalHost;Database=rssfeedaggregatorapi_db;Trusted_Connection=true;MultipleActiveResultSets=True;Encrypt=False;");

using var db = new AppDbContext(optionsBuilder.Options);

try
{
    db.Database.EnsureCreated();

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

    var rssFeedService = new RssFeedService(db);
    await rssFeedService.FetchAndSaveArticlesAsync(feedUrls);
}
finally
{
    db.Dispose();
}

Console.WriteLine("Parse is done");
