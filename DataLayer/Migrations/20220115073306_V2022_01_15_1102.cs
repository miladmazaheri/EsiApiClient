using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLayer.Migrations
{
    public partial class V2022_01_15_1102 : Migration
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
                name: "Foods",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Des_Food = table.Column<string>(type: "TEXT", nullable: true),
                    Num_Amount = table.Column<string>(type: "TEXT", nullable: true),
                    Typ_Serv_Unit = table.Column<string>(type: "TEXT", nullable: true),
                    IsMain = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReservationId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Foods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Foods_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Foods_ReservationId",
                table: "Foods",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Date_Cod_Meal_Reciver_Coupon_Id_Num_Ide",
                table: "Reservations",
                columns: new[] { "Date", "Cod_Meal", "Reciver_Coupon_Id", "Num_Ide" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Foods");

            migrationBuilder.DropTable(
                name: "Reservations");
        }
    }
}
