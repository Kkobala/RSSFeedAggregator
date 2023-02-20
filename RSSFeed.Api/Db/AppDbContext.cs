using Microsoft.EntityFrameworkCore;
using RSSFeed.Api.Db.Entities;

namespace RSSFeed.Api.Db
{
    public class AppDbContext : DbContext
    {
        public DbSet<FeedEntity> FeedEntities { get; set; }
        public DbSet<TagsEntity> Tags { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
