﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sr_server.Migrations
{
    /// <inheritdoc />
    public partial class AddVotesCountVersionColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Version",
                table: "VoteCounts",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "VoteCounts");
        }
    }
}
