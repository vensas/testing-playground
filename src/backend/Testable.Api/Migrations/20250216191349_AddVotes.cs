using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Testable.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "testable");

            migrationBuilder.CreateTable(
                name: "Votes",
                schema: "testable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Candidate = table.Column<string>(type: "text", nullable: false),
                    Party = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Votes",
                schema: "testable");
        }
    }
}
