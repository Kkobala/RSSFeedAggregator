using AngleSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RSSFeed.Api.Db;
using RSSFeed.Fetcher;
using RSSFeed.Fetcher.Services;

public class Program
{
    private static async Task Main(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer("Server=LocalHost;Database=rssfeedaggregatorapi_db;Trusted_Connection=true;MultipleActiveResultSets=True;Encrypt=False;");

        var db = new AppDbContext(optionsBuilder.Options);

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

            var rssFeedFetcherService = new NewsFeedFetcherService(db);
            await rssFeedFetcherService.FetchAndSaveArticlesAsync(feedUrls);
        

        Console.WriteLine("Parse is done");

        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Worker>();
                services.AddScoped<NewsFeedFetcherService>();
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer("Server=LocalHost;Database=rssfeedaggregatorapi_db;Trusted_Connection=true;MultipleActiveResultSets=True;Encrypt=False;"), ServiceLifetime.Scoped);
            });
}