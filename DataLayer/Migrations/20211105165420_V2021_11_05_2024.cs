using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLayer.Migrations
{
    public partial class V2021_11_05_2024 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<string>(type: "TEXT", nullable: true),
                    Cod_Meal = table.Column<string>(type: "TEXT", nullable: true),
                    Receiver_Meal_Plan_Day_Id = table.Column<string>(type: "TEXT", nullable: true),
                    Reciver_Coupon_Id = table.Column<string>(type: "TEXT", nullable: true),
                    Meal_Plan_Day_Id = table.Column<string>(type: "TEXT", nullable: true),
                    Num_Ide = table.Column<string>(type: "TEXT", nullable: true),
                    First_Name_Ide = table.Column<string>(type: "TEXT", nullable: true),
                    Last_Name_Ide = table.Column<string>(type: "TEXT", nullable: true),
                    Cod_Serial = table.Column<string>(type: "TEXT", nullable: true),
                    Cod_Contract_Order = table.Column<string>(type: "TEXT", nullable: true),
                    Des_Contract_Order = table.Column<string>(type: "TEXT", nullable: true),
                    Employee_Shift_Name = table.Column<string>(type: "TEXT", nullable: true),
                    Dat_Day_Mepdy = table.Column<string>(type: "TEXT", nullable: true),
                    Des_Nam_Meal = table.Column<string>(type: "TEXT", nullable: true),
                    Num_Tim_Str_Meal_Rsmls = table.Column<string>(type: "TEXT", nullable: true),
                    Num_Tim_End_Meal_Rsmls = table.Column<string>(type: "TEXT", nullable: true),
                    Des_Nam_Resturant_Rstm = table.Column<string>(type: "TEXT", nullable: true),
                    Num_Tot_Coupon_Rccpn = table.Column<string>(type: "TEXT", nullable: true),
                    Des_Food_Order_Mepdy = table.Column<string>(type: "TEXT", nullable: true),
                    Lkp_Cod_Order_Mepdy_Means = table.Column<string>(type: "TEXT", nullable: true),
                    Cod_Resturant = table.Column<string>(type: "TEXT", nullable: true),
                    Cod_Coupon = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: true),
                    Date_Use = table.Column<string>(type: "TEXT", nullable: true),
                    Time_Use = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reservations_Appetizer_Dessert",
                columns: table => new
                {
                    ReservationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Des_Food = table.Column<string>(type: "TEXT", nullable: true),
                    Num_Amount = table.Column<string>(type: "TEXT", nullable: true),
                    Typ_Serv_Unit = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations_Appetizer_Dessert", x => new { x.ReservationId, x.Id });
                    table.ForeignKey(
                        name: "FK_Reservations_Appetizer_Dessert_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservations_Main_Course",
                columns: table => new
                {
                    ReservationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Des_Food = table.Column<string>(type: "TEXT", nullable: true),
                    Num_Amount = table.Column<string>(type: "TEXT", nullable: true),
                    Typ_Serv_Unit = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations_Main_Course", x => new { x.ReservationId, x.Id });
                    table.ForeignKey(
                        name: "FK_Reservations_Main_Course_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Date_Cod_Meal_Reciver_Coupon_Id_Num_Ide",
                table: "Reservations",
                columns: new[] { "Date", "Cod_Meal", "Reciver_Coupon_Id", "Num_Ide" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservations_Appetizer_Dessert");

            migrationBuilder.DropTable(
                name: "Reservations_Main_Course");

            migrationBuilder.DropTable(
                name: "Reservations");
        }
    }
}
