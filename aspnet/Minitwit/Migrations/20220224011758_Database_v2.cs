using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Minitwit.Migrations
{
    public partial class Database_v2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Follows_FolloweeId",
                table: "Follows",
                column: "FolloweeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Follows_FolloweeId",
                table: "Follows");
        }
    }
}
