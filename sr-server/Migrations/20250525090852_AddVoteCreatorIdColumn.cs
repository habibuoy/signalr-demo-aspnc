using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sr_server.Migrations
{
    /// <inheritdoc />
    public partial class AddVoteCreatorIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorId",
                table: "Votes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votes_CreatorId",
                table: "Votes",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Users_CreatorId",
                table: "Votes",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Users_CreatorId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_CreatorId",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Votes");
        }
    }
}
