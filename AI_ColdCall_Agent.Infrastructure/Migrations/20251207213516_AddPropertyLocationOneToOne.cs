using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_ColdCall_Agent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyLocationOneToOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertiesLocations_Properties_PropertyId",
                table: "PropertiesLocations");

            migrationBuilder.DropIndex(
                name: "IX_PropertiesLocations_PropertyId",
                table: "PropertiesLocations");

            migrationBuilder.DropColumn(
                name: "PropertyId",
                table: "PropertiesLocations");

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "Properties",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Properties_LocationId",
                table: "Properties",
                column: "LocationId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_PropertiesLocations_LocationId",
                table: "Properties",
                column: "LocationId",
                principalTable: "PropertiesLocations",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_PropertiesLocations_LocationId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_LocationId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Properties");

            migrationBuilder.AddColumn<int>(
                name: "PropertyId",
                table: "PropertiesLocations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PropertiesLocations_PropertyId",
                table: "PropertiesLocations",
                column: "PropertyId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertiesLocations_Properties_PropertyId",
                table: "PropertiesLocations",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "PropertyId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
