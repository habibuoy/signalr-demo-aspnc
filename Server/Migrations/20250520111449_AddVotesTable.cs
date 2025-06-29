using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sr_server.Migrations
{
    /// <inheritdoc />
    public partial class AddVotesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Votes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiredTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MaximumCount = table.Column<int>(type: "INTEGER", nullable: true),
                    CurrentTotalCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VoteSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VoteId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoteSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoteSubjects_Votes_VoteId",
                        column: x => x.VoteId,
                        principalTable: "Votes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoteCounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Count = table.Column<int>(type: "INTEGER", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_VoteSubjects_VoteId",
                table: "VoteSubjects",
                column: "VoteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VoteCounts");

            migrationBuilder.DropTable(
                name: "VoteSubjects");

            migrationBuilder.DropTable(
                name: "Votes");
        }
    }
}
