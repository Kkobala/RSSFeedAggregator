using Microsoft.Extensions.Hosting;
using RSSFeed.Fetcher.Services;

namespace RSSFeed.Fetcher
{
    public class Worker : BackgroundService
    {
        private readonly NewsFeedFetcherService _rssFeedFetcherService;

        public Worker(NewsFeedFetcherService rssFeedFetcherService)
        {
            _rssFeedFetcherService = rssFeedFetcherService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var feedUrl = new List<string>();

            while (!stoppingToken.IsCancellationRequested)
            {
                await _rssFeedFetcherService.FetchAndSaveArticlesAsync(feedUrl);
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }
    }
}
