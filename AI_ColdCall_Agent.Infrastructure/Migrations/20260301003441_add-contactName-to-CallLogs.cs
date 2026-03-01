using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_ColdCall_Agent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addcontactNametoCallLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "CallLogs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "CallLogs");
        }
    }
}
