using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DataLayer.Entities
{
    public class Reservation
    {
        public Reservation()
        {
            Foods = new HashSet<Food>();
        }
        public Guid Id { get; set; }
        public string Date { get; set; }
        public string Cod_Meal { get; set; }

        public string Receiver_Meal_Plan_Day_Id { get; set; }
        public string Reciver_Coupon_Id { get; set; }
        public string Meal_Plan_Day_Id { get; set; }
        public string Num_Ide { get; set; }
        public string First_Name_Ide { get; set; }
        public string Last_Name_Ide { get; set; }
        public string Cod_Serial { get; set; }
        public string Cod_Contract_Order { get; set; }
        public string Des_Contract_Order { get; set; }
        public string Employee_Shift_Name { get; set; }
        public string Dat_Day_Mepdy { get; set; }
        public string Des_Nam_Meal { get; set; }
        public string Num_Tim_Str_Meal_Rsmls { get; set; }
        public string Num_Tim_End_Meal_Rsmls { get; set; }
        public string Des_Nam_Resturant_Rstm { get; set; }
        public string Num_Tot_Coupon_Rccpn { get; set; }
        public string Des_Food_Order_Mepdy { get; set; }
        public string Lkp_Cod_Order_Mepdy_Means { get; set; }
        public string Cod_Resturant { get; set; }
        public string Cod_Coupon { get; set; }

        public string Status { get; set; }
        public string Date_Use { get; set; }
        public string Time_Use { get; set; }
        public DateTime? DateTime_SentToWebService { get; set; }
        public ICollection<Food> Foods { get; set; }
    }

    public class Food
    {
        public long Id { get; set; }
        public string Des_Food { get; set; }
        public string Num_Amount { get; set; }
        public string Typ_Serv_Unit { get; set; }
        public bool IsMain { get; set; }
        [JsonIgnore]
        public Guid ReservationId { get; set; }
        [JsonIgnore]
        public Reservation Reservation { get; set; }
    }
}
