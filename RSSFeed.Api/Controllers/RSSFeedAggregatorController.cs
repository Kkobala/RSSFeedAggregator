using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSSFeed.Api.Db;
using RSSFeed.Api.Models;

namespace RSSFeed.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RSSFeedAggregatorController : ControllerBase
    {
        private readonly AppDbContext _db;

        public RSSFeedAggregatorController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllFeeds(int page = 1, int pageSize = 10)
        {
            var feedEntities = await _db.FeedEntities
                .OrderByDescending(a => a.PublishedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var feedDtos = feedEntities.Select(a => new FeedDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Link = a.Link,
                Author = a.Author,
                Picture = a.Picture,
                Summery = a.Summery,
                Tags = a.Tags,
                PublishedDate = a.PublishedDate
            }).ToList();

            return Ok(feedDtos);
        }

        [HttpGet("tag/{tagName}")]
        public async Task<IActionResult> GetFeedsByTag(string tagName, int page = 1, int pageSize = 10)
        {
            var feedEntities = await _db.FeedEntities
                .Where(a => a.Tags!.Contains(tagName))
                .OrderByDescending(a => a.PublishedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var feedDtos = feedEntities.Select(a => new FeedDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Link = a.Link,
                Author = a.Author,
                Picture = a.Picture,
                Summery = a.Summery,
                Tags = a.Tags,
                PublishedDate = a.PublishedDate
            }).ToList();

            return Ok(feedDtos);
        }
    }
}
