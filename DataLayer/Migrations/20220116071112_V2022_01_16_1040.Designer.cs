// <auto-generated />
using System;
using DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DataLayer.Migrations
{
    [DbContext(typeof(EsiDbContext))]
    [Migration("20220116071112_V2022_01_16_1040")]
    partial class V2022_01_16_1040
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.11");

            modelBuilder.Entity("DataLayer.Entities.Food", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Des_Food")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsMain")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Num_Amount")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("ReservationId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Typ_Serv_Unit")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ReservationId");

                    b.ToTable("Foods");
                });

            modelBuilder.Entity("DataLayer.Entities.Reservation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Cod_Contract_Order")
                        .HasColumnType("TEXT");

                    b.Property<string>("Cod_Coupon")
                        .HasColumnType("TEXT");

                    b.Property<string>("Cod_Meal")
                        .HasColumnType("TEXT");

                    b.Property<string>("Cod_Resturant")
                        .HasColumnType("TEXT");

                    b.Property<string>("Cod_Serial")
                        .HasColumnType("TEXT");

                    b.Property<string>("Dat_Day_Mepdy")
                        .HasColumnType("TEXT");

                    b.Property<string>("Date")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DateTime_SentToWebService")
                        .HasColumnType("TEXT");

                    b.Property<string>("Date_Use")
                        .HasColumnType("TEXT");

                    b.Property<string>("Des_Contract_Order")
                        .HasColumnType("TEXT");

                    b.Property<string>("Des_Food_Order_Mepdy")
                        .HasColumnType("TEXT");

                    b.Property<string>("Des_Nam_Meal")
                        .HasColumnType("TEXT");

                    b.Property<string>("Des_Nam_Resturant_Rstm")
                        .HasColumnType("TEXT");

                    b.Property<string>("Employee_Shift_Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("First_Name_Ide")
                        .HasColumnType("TEXT");

                    b.Property<string>("Last_Name_Ide")
                        .HasColumnType("TEXT");

                    b.Property<string>("Lkp_Cod_Order_Mepdy_Means")
                        .HasColumnType("TEXT");

                    b.Property<string>("Meal_Plan_Day_Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Num_Ide")
                        .HasColumnType("TEXT");

                    b.Property<string>("Num_Tim_End_Meal_Rsmls")
                        .HasColumnType("TEXT");

                    b.Property<string>("Num_Tim_Str_Meal_Rsmls")
                        .HasColumnType("TEXT");

                    b.Property<string>("Num_Tot_Coupon_Rccpn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Receiver_Meal_Plan_Day_Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Reciver_Coupon_Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .HasColumnType("TEXT");

                    b.Property<string>("Time_Use")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Date", "Cod_Meal", "Reciver_Coupon_Id", "Num_Ide");

                    b.ToTable("Reservations");
                });

            modelBuilder.Entity("DataLayer.Entities.Food", b =>
                {
                    b.HasOne("DataLayer.Entities.Reservation", "Reservation")
                        .WithMany("Foods")
                        .HasForeignKey("ReservationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Reservation");
                });

            modelBuilder.Entity("DataLayer.Entities.Reservation", b =>
                {
                    b.Navigation("Foods");
                });
#pragma warning restore 612, 618
        }
    }
}
