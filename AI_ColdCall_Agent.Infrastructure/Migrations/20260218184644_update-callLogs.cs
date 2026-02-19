using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_ColdCall_Agent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatecallLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CallIDFromAI",
                table: "CallLogs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CallIDFromAI",
                table: "CallLogs");
        }
    }
}
