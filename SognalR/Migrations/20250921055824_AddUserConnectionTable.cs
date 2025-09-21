using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SignalR.Migrations
{
    /// <inheritdoc />
    public partial class AddUserConnectionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DisconnectedAt",
                table: "UserConnections",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConnected",
                table: "UserConnections",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisconnectedAt",
                table: "UserConnections");

            migrationBuilder.DropColumn(
                name: "IsConnected",
                table: "UserConnections");
        }
    }
}
