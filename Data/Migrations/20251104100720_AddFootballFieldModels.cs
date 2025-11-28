using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FootballFieldBooking_New.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFootballFieldModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FieldTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PlayerCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FootballFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    FieldTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FootballFields_FieldTypes_FieldTypeId",
                        column: x => x.FieldTypeId,
                        principalTable: "FieldTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TimeSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    PricePerHour = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsWeekend = table.Column<bool>(type: "bit", nullable: false),
                    FootballFieldId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeSlots_FootballFields_FootballFieldId",
                        column: x => x.FootballFieldId,
                        principalTable: "FootballFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlayDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FootballFieldId = table.Column<int>(type: "int", nullable: false),
                    TimeSlotId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_FootballFields_FootballFieldId",
                        column: x => x.FootballFieldId,
                        principalTable: "FootballFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_TimeSlots_TimeSlotId",
                        column: x => x.TimeSlotId,
                        principalTable: "TimeSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Method = table.Column<int>(type: "int", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_FootballFieldId",
                table: "Bookings",
                column: "FootballFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TimeSlotId",
                table: "Bookings",
                column: "TimeSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballFields_FieldTypeId",
                table: "FootballFields",
                column: "FieldTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingId",
                table: "Payments",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeSlots_FootballFieldId",
                table: "TimeSlots",
                column: "FootballFieldId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "TimeSlots");

            migrationBuilder.DropTable(
                name: "FootballFields");

            migrationBuilder.DropTable(
                name: "FieldTypes");
        }
    }
}
