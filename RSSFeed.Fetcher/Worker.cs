using Microsoft.Extensions.Hosting;
using RSSFeed.Fetcher.Services;

namespace RSSFeed.Fetcher
{
    public class Worker : BackgroundService
    {
        private readonly RssFeedService _rssFeedService;

        public Worker(RssFeedService rssFeedService)
        {
            _rssFeedService = rssFeedService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var feedUrl = new List<string>();

            while (!stoppingToken.IsCancellationRequested)
            {
                await _rssFeedService.FetchAndSaveArticlesAsync(feedUrl);
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }
    }
}
