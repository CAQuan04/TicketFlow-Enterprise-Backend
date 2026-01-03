using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventSalesConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxTicketsPerUser",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<DateTime>(
                name: "TicketSaleEndTime",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TicketSaleStartTime",
                table: "Events",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxTicketsPerUser",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "TicketSaleEndTime",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "TicketSaleStartTime",
                table: "Events");
        }
    }
}
