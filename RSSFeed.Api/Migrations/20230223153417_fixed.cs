using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSSFeed.Api.Migrations
{
    /// <inheritdoc />
    public partial class @fixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Summery",
                table: "FeedEntities",
                newName: "Summary");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Summary",
                table: "FeedEntities",
                newName: "Summery");
        }
    }
}
