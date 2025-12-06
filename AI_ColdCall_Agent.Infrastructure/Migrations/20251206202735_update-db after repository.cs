using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AI_ColdCall_Agent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatedbafterrepository : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Meetings");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "PropertiesLocations");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "PropertiesLocations");

            migrationBuilder.DropColumn(
                name: "Furnished",
                table: "Properties");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Deals",
                newName: "FinalSaleAmount");

            migrationBuilder.AddColumn<int>(
                name: "BuyerConfirmationStatusId",
                table: "Deals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "MeetingDate",
                table: "Deals",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "MeetingLocation",
                table: "Deals",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MeetingStatusId",
                table: "Deals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SellerConfirmationStatusId",
                table: "Deals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "UserHistories",
                columns: table => new
                {
                    UserHistoryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserRoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHistories", x => x.UserHistoryId);
                    table.ForeignKey(
                        name: "FK_UserHistories_AspNetRoles_UserRoleId",
                        column: x => x.UserRoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserHistories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Deals_BuyerConfirmationStatusId",
                table: "Deals",
                column: "BuyerConfirmationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_MeetingStatusId",
                table: "Deals",
                column: "MeetingStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_SellerConfirmationStatusId",
                table: "Deals",
                column: "SellerConfirmationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHistories_UserId",
                table: "UserHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHistories_UserRoleId",
                table: "UserHistories",
                column: "UserRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_ConfirmationStatuses_BuyerConfirmationStatusId",
                table: "Deals",
                column: "BuyerConfirmationStatusId",
                principalTable: "ConfirmationStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_ConfirmationStatuses_SellerConfirmationStatusId",
                table: "Deals",
                column: "SellerConfirmationStatusId",
                principalTable: "ConfirmationStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_MeetingStatuses_MeetingStatusId",
                table: "Deals",
                column: "MeetingStatusId",
                principalTable: "MeetingStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deals_ConfirmationStatuses_BuyerConfirmationStatusId",
                table: "Deals");

            migrationBuilder.DropForeignKey(
                name: "FK_Deals_ConfirmationStatuses_SellerConfirmationStatusId",
                table: "Deals");

            migrationBuilder.DropForeignKey(
                name: "FK_Deals_MeetingStatuses_MeetingStatusId",
                table: "Deals");

            migrationBuilder.DropTable(
                name: "UserHistories");

            migrationBuilder.DropIndex(
                name: "IX_Deals_BuyerConfirmationStatusId",
                table: "Deals");

            migrationBuilder.DropIndex(
                name: "IX_Deals_MeetingStatusId",
                table: "Deals");

            migrationBuilder.DropIndex(
                name: "IX_Deals_SellerConfirmationStatusId",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "BuyerConfirmationStatusId",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "MeetingDate",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "MeetingLocation",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "MeetingStatusId",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "SellerConfirmationStatusId",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "FinalSaleAmount",
                table: "Deals",
                newName: "Amount");

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "PropertiesLocations",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "PropertiesLocations",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Furnished",
                table: "Properties",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Meetings",
                columns: table => new
                {
                    MeetingId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerConfirmationStatusId = table.Column<int>(type: "integer", nullable: false),
                    BuyerContactId = table.Column<int>(type: "integer", nullable: false),
                    MeetingStatusId = table.Column<int>(type: "integer", nullable: false),
                    PropertyId = table.Column<int>(type: "integer", nullable: false),
                    SellerConfirmationStatusId = table.Column<int>(type: "integer", nullable: false),
                    SellerContactId = table.Column<int>(type: "integer", nullable: false),
                    MeetingSlot = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meetings", x => x.MeetingId);
                    table.ForeignKey(
                        name: "FK_Meetings_AspNetUsers_AgentId",
                        column: x => x.AgentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade,
                        onUpdate: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Meetings_ConfirmationStatuses_BuyerConfirmationStatusId",
                        column: x => x.BuyerConfirmationStatusId,
                        principalTable: "ConfirmationStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade,
						onUpdate: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Meetings_ConfirmationStatuses_SellerConfirmationStatusId",
                        column: x => x.SellerConfirmationStatusId,
                        principalTable: "ConfirmationStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade,
						onUpdate: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Meetings_Contacts_BuyerContactId",
                        column: x => x.BuyerContactId,
                        principalTable: "Contacts",
                        principalColumn: "ContactId",
                        onDelete: ReferentialAction.Cascade,
						onUpdate: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Meetings_Contacts_SellerContactId",
                        column: x => x.SellerContactId,
                        principalTable: "Contacts",
                        principalColumn: "ContactId",
                        onDelete: ReferentialAction.Cascade,
						onUpdate: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Meetings_MeetingStatuses_MeetingStatusId",
                        column: x => x.MeetingStatusId,
                        principalTable: "MeetingStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade,
						onUpdate: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Meetings_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "PropertyId",
                        onDelete: ReferentialAction.Cascade,
						onUpdate: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_AgentId",
                table: "Meetings",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_BuyerConfirmationStatusId",
                table: "Meetings",
                column: "BuyerConfirmationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_BuyerContactId",
                table: "Meetings",
                column: "BuyerContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_MeetingStatusId",
                table: "Meetings",
                column: "MeetingStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_PropertyId",
                table: "Meetings",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_SellerConfirmationStatusId",
                table: "Meetings",
                column: "SellerConfirmationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_SellerContactId",
                table: "Meetings",
                column: "SellerContactId");
        }
    }
}
