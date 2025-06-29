using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sr_server.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVoteCountsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VoteCounts");

            migrationBuilder.DropColumn(
                name: "CurrentTotalCount",
                table: "Votes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentTotalCount",
                table: "Votes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "VoteCounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Count = table.Column<int>(type: "INTEGER", nullable: false),
                    Version = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoteCounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoteCounts_VoteSubjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "VoteSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VoteCounts_SubjectId",
                table: "VoteCounts",
                column: "SubjectId",
                unique: true);
        }
    }
}
