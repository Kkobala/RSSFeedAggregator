using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSFeed.Fetcher.Services
{
    public class BackGroundService : BackgroundService
    {
        private readonly NewsFeedService _newsFeedService;

        public BackGroundService(NewsFeedService newsFeedService)
        {
            _newsFeedService = newsFeedService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var feedUrl = new List<string>();

            while (!stoppingToken.IsCancellationRequested)
            {
                await _newsFeedService.FetchAndSaveArticlesAsync(feedUrl);
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }
    }
}
