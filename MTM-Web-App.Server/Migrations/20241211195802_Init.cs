using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTM_Web_App.Server.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HotelResProcessed",
                columns: table => new
                {
                    ReservationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstDigit = table.Column<int>(type: "int", nullable: false),
                    SecondDigit = table.Column<int>(type: "int", nullable: false),
                    ThirdDigit = table.Column<int>(type: "int", nullable: false),
                    FourthDigit = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Salt = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEmailValid = table.Column<bool>(type: "bit", nullable: false),
                    IsUserValid = table.Column<bool>(type: "bit", nullable: false),
                    PfpSrc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsGooglePfp = table.Column<bool>(type: "bit", nullable: false),
                    GoogleSub = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Hotels",
                columns: table => new
                {
                    HotelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    CheckIn = table.Column<TimeOnly>(type: "time", nullable: false),
                    CheckOut = table.Column<TimeOnly>(type: "time", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Facilities = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hotels", x => x.HotelId);
                    table.ForeignKey(
                        name: "FK_Hotels_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Logger",
                columns: table => new
                {
                    LogId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logger", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_Logger_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OTPs",
                columns: table => new
                {
                    OtpId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTPs", x => x.OtpId);
                    table.ForeignKey(
                        name: "FK_OTPs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Restaurants",
                columns: table => new
                {
                    RestaurantId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    Cusines = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Restaurants", x => x.RestaurantId);
                    table.ForeignKey(
                        name: "FK_Restaurants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HotelImage",
                columns: table => new
                {
                    HotelImageId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HotelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    ImageSrc = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotelImage", x => x.HotelImageId);
                    table.ForeignKey(
                        name: "FK_HotelImage_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "HotelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HotelRatings",
                columns: table => new
                {
                    RatingId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    HotelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    Rate = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotelRatings", x => x.RatingId);
                    table.ForeignKey(
                        name: "FK_HotelRatings_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "HotelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    RoomId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoomType = table.Column<int>(type: "int", nullable: false),
                    Facilities = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PersonCount = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HotelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.RoomId);
                    table.ForeignKey(
                        name: "FK_Rooms_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "HotelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Locale = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HouseNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Road = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HotelId = table.Column<decimal>(type: "decimal(20,0)", nullable: true),
                    RestaurantId = table.Column<decimal>(type: "decimal(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "HotelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Addresses_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "RestaurantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpeningHours",
                columns: table => new
                {
                    OpeningHoursId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    OpeningTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    ClosingTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    RestaurantId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpeningHours", x => x.OpeningHoursId);
                    table.ForeignKey(
                        name: "FK_OpeningHours_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "RestaurantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantImage",
                columns: table => new
                {
                    RestaurantImageId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RestaurantId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    ImageSrc = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantImage", x => x.RestaurantImageId);
                    table.ForeignKey(
                        name: "FK_RestaurantImage_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "RestaurantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantRating",
                columns: table => new
                {
                    RatingId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    RestaurantId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    Rate = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantRating", x => x.RatingId);
                    table.ForeignKey(
                        name: "FK_RestaurantRating_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "RestaurantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tables",
                columns: table => new
                {
                    TableId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PersonCount = table.Column<int>(type: "int", nullable: false),
                    TableNumber = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RestaurantId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tables", x => x.TableId);
                    table.ForeignKey(
                        name: "FK_Tables_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "RestaurantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HotelRes",
                columns: table => new
                {
                    HotelResId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReservationVerification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SummaryCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoomId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    UserId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotelRes", x => x.HotelResId);
                    table.ForeignKey(
                        name: "FK_HotelRes_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HotelRes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoomEntities",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomNumber = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomEntities_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoomImage",
                columns: table => new
                {
                    RoomImageId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    ImageSrc = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomImage", x => x.RoomImageId);
                    table.ForeignKey(
                        name: "FK_RoomImage_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantsRes",
                columns: table => new
                {
                    RestaurantResId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReservationVerification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    SummaryCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TableId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    UserId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantsRes", x => x.RestaurantResId);
                    table.ForeignKey(
                        name: "FK_RestaurantsRes_Tables_TableId",
                        column: x => x.TableId,
                        principalTable: "Tables",
                        principalColumn: "TableId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RestaurantsRes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TableEntities",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableNumber = table.Column<int>(type: "int", nullable: false),
                    TableId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TableEntities_Tables_TableId",
                        column: x => x.TableId,
                        principalTable: "Tables",
                        principalColumn: "TableId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableImage",
                columns: table => new
                {
                    TableImageId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    ImageSrc = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableImage", x => x.TableImageId);
                    table.ForeignKey(
                        name: "FK_TableImage_Tables_TableId",
                        column: x => x.TableId,
                        principalTable: "Tables",
                        principalColumn: "TableId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoomAvailability",
                columns: table => new
                {
                    AvailabilityId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    IsReserved = table.Column<bool>(type: "bit", nullable: false),
                    RoomId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    RoomEntityId = table.Column<decimal>(type: "decimal(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomAvailability", x => x.AvailabilityId);
                    table.ForeignKey(
                        name: "FK_RoomAvailability_RoomEntities_RoomEntityId",
                        column: x => x.RoomEntityId,
                        principalTable: "RoomEntities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoomAvailability_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableAvailability",
                columns: table => new
                {
                    TableAvailabilityId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    IsReserved = table.Column<bool>(type: "bit", nullable: false),
                    TableId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    TableEntityId = table.Column<decimal>(type: "decimal(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableAvailability", x => x.TableAvailabilityId);
                    table.ForeignKey(
                        name: "FK_TableAvailability_TableEntities_TableEntityId",
                        column: x => x.TableEntityId,
                        principalTable: "TableEntities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TableAvailability_Tables_TableId",
                        column: x => x.TableId,
                        principalTable: "Tables",
                        principalColumn: "TableId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_HotelId",
                table: "Addresses",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_RestaurantId",
                table: "Addresses",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_HotelImage_HotelId",
                table: "HotelImage",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_HotelRatings_HotelId",
                table: "HotelRatings",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_HotelRes_RoomId",
                table: "HotelRes",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_HotelRes_UserId",
                table: "HotelRes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_UserId",
                table: "Hotels",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Logger_UserId",
                table: "Logger",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningHours_RestaurantId",
                table: "OpeningHours",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_OTPs_UserId",
                table: "OTPs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantImage_RestaurantId",
                table: "RestaurantImage",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantRating_RestaurantId",
                table: "RestaurantRating",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_UserId",
                table: "Restaurants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantsRes_TableId",
                table: "RestaurantsRes",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantsRes_UserId",
                table: "RestaurantsRes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomAvailability_RoomEntityId",
                table: "RoomAvailability",
                column: "RoomEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomAvailability_RoomId",
                table: "RoomAvailability",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomEntities_RoomId",
                table: "RoomEntities",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomImage_RoomId",
                table: "RoomImage",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_HotelId",
                table: "Rooms",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_TableAvailability_TableEntityId",
                table: "TableAvailability",
                column: "TableEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_TableAvailability_TableId",
                table: "TableAvailability",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_TableEntities_TableId",
                table: "TableEntities",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_TableImage_TableId",
                table: "TableImage",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_Tables_RestaurantId",
                table: "Tables",
                column: "RestaurantId");

            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS HotelResProcessed; 
                DROP TABLE IF EXISTS RestaurantResProcessed;
            ");

            migrationBuilder.Sql(@"
                CREATE VIEW HotelResProcessed AS
                SELECT 
                    ReservationNumber,
                    CAST(SUBSTRING(ReservationNumber, 1, 1) AS INT) AS FirstDigit,
                    CAST(SUBSTRING(ReservationNumber, 3, 1) AS INT) AS SecondDigit,
                    CAST(SUBSTRING(ReservationNumber, 5, 1) AS INT) AS ThirdDigit,
                    CAST(SUBSTRING(ReservationNumber, 7, 1) AS INT) AS FourthDigit
                FROM HotelRes;
            ");

            migrationBuilder.Sql(@"
                CREATE VIEW RestaurantResProcessed AS
                SELECT 
                    ReservationNumber,
                    CAST(SUBSTRING(ReservationNumber, 1, 1) AS INT) AS FirstDigit,
                    CAST(SUBSTRING(ReservationNumber, 3, 1) AS INT) AS SecondDigit,
                    CAST(SUBSTRING(ReservationNumber, 5, 1) AS INT) AS ThirdDigit,
                    CAST(SUBSTRING(ReservationNumber, 7, 1) AS INT) AS FourthDigit
                FROM RestaurantsRes;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "HotelImage");

            migrationBuilder.DropTable(
                name: "HotelRatings");

            migrationBuilder.DropTable(
                name: "HotelRes");

            migrationBuilder.DropTable(
                name: "HotelResProcessed");

            migrationBuilder.DropTable(
                name: "Logger");

            migrationBuilder.DropTable(
                name: "OpeningHours");

            migrationBuilder.DropTable(
                name: "OTPs");

            migrationBuilder.DropTable(
                name: "RestaurantImage");

            migrationBuilder.DropTable(
                name: "RestaurantRating");

            migrationBuilder.DropTable(
                name: "RestaurantsRes");

            migrationBuilder.DropTable(
                name: "RoomAvailability");

            migrationBuilder.DropTable(
                name: "RoomImage");

            migrationBuilder.DropTable(
                name: "TableAvailability");

            migrationBuilder.DropTable(
                name: "TableImage");

            migrationBuilder.DropTable(
                name: "RoomEntities");

            migrationBuilder.DropTable(
                name: "TableEntities");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "Tables");

            migrationBuilder.DropTable(
                name: "Hotels");

            migrationBuilder.DropTable(
                name: "Restaurants");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS HotelResProcessed; 
                DROP TABLE IF EXISTS RestaurantResProcessed;
            ");
        }
    }
}
