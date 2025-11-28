using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FootballFieldBooking_New.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_AspNetUsers_UserId",
                table: "Bookings");

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "FootballFields",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "FootballFields",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "FootballFields",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "FootballFields",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "FieldTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "FieldTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "FieldTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_AspNetUsers_UserId",
                table: "Bookings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_AspNetUsers_UserId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.InsertData(
                table: "FieldTypes",
                columns: new[] { "Id", "Description", "Name", "PlayerCount" },
                values: new object[,]
                {
                    { 1, "Sân bóng 5 người", "Sân 5 người", 5 },
                    { 2, "Sân bóng 7 người", "Sân 7 người", 7 },
                    { 3, "Sân bóng 11 người tiêu chuẩn", "Sân 11 người", 11 }
                });

            migrationBuilder.InsertData(
                table: "FootballFields",
                columns: new[] { "Id", "Description", "FieldTypeId", "ImageUrl", "IsAvailable", "Location", "Name" },
                values: new object[,]
                {
                    { 1, "Sân cỏ nhân tạo chất lượng cao, có mái che", 1, "/images/fields/san-a1.jpg", true, "Khu A, Tầng 1", "Sân A1" },
                    { 2, "Sân cỏ nhân tạo, đèn chiếu sáng hiện đại", 1, "/images/fields/san-a2.jpg", true, "Khu A, Tầng 1", "Sân A2" },
                    { 3, "Sân cỏ tự nhiên, view đẹp", 2, "/images/fields/san-b1.jpg", true, "Khu B, Tầng 2", "Sân B1" },
                    { 4, "Sân cỏ tiêu chuẩn FIFA, phù hợp thi đấu", 3, "/images/fields/san-c1.jpg", true, "Khu C, Sân chính", "Sân C1" }
                });

            migrationBuilder.InsertData(
                table: "TimeSlots",
                columns: new[] { "Id", "EndTime", "FootballFieldId", "IsWeekend", "PricePerHour", "StartTime" },
                values: new object[,]
                {
                    { 1, new TimeSpan(0, 8, 0, 0, 0), 1, false, 200000m, new TimeSpan(0, 6, 0, 0, 0) },
                    { 2, new TimeSpan(0, 10, 0, 0, 0), 1, false, 250000m, new TimeSpan(0, 8, 0, 0, 0) },
                    { 3, new TimeSpan(0, 16, 0, 0, 0), 1, false, 300000m, new TimeSpan(0, 14, 0, 0, 0) },
                    { 4, new TimeSpan(0, 18, 0, 0, 0), 1, false, 350000m, new TimeSpan(0, 16, 0, 0, 0) },
                    { 5, new TimeSpan(0, 20, 0, 0, 0), 1, false, 400000m, new TimeSpan(0, 18, 0, 0, 0) },
                    { 6, new TimeSpan(0, 22, 0, 0, 0), 1, false, 400000m, new TimeSpan(0, 20, 0, 0, 0) },
                    { 7, new TimeSpan(0, 8, 0, 0, 0), 2, false, 200000m, new TimeSpan(0, 6, 0, 0, 0) },
                    { 8, new TimeSpan(0, 20, 0, 0, 0), 2, false, 400000m, new TimeSpan(0, 18, 0, 0, 0) },
                    { 9, new TimeSpan(0, 8, 0, 0, 0), 3, false, 300000m, new TimeSpan(0, 6, 0, 0, 0) },
                    { 10, new TimeSpan(0, 20, 0, 0, 0), 3, false, 500000m, new TimeSpan(0, 18, 0, 0, 0) },
                    { 11, new TimeSpan(0, 8, 0, 0, 0), 4, false, 500000m, new TimeSpan(0, 6, 0, 0, 0) },
                    { 12, new TimeSpan(0, 20, 0, 0, 0), 4, false, 800000m, new TimeSpan(0, 18, 0, 0, 0) }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_AspNetUsers_UserId",
                table: "Bookings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
