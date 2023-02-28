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
        optionsBuilder.UseSqlServer("Server=LocalHost;Database=rssfeedaggregator_db;Trusted_Connection=true;MultipleActiveResultSets=True;Encrypt=False;");

        var db = new AppDbContext(optionsBuilder.Options);

        db.Database.EnsureCreated();

        var rssFeedFetcherService = new NewsFeedFetcherService(db);
        await rssFeedFetcherService.FetchAndSaveArticlesAsync();


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
                    options.UseSqlServer("Server=LocalHost;Database=rssfeedaggregator_db;Trusted_Connection=true;MultipleActiveResultSets=True;Encrypt=False;"), ServiceLifetime.Scoped);
            });
}