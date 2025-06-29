using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sr_server.Migrations
{
    /// <inheritdoc />
    public partial class AddVoteSubjectVersionColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Version",
                table: "VoteSubjects",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "VoteSubjects");
        }
    }
}
