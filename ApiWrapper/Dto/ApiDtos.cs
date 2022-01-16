using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiWrapper.Dto
{
    #region Base
    public abstract class BaseInput<T> where T : class
    {
        public List<T> JSON_CHARACTER { get; set; } = new List<T>();

        public BaseInput()
        {

        }

        public BaseInput(List<T> jsonCharacter) : this()
        {
            JSON_CHARACTER.AddRange(jsonCharacter);
        }

        public BaseInput(params T[] inputs) : this()
        {
            foreach (var input in inputs)
            {
                JSON_CHARACTER.Add(input);
            }
        }

    }

    public class ServerMessage
    {
        public string Message_Category { get; set; }
        public string Message_Type { get; set; }
        public string Message_Code { get; set; }
        public string Message_Description { get; set; }
    }
    #endregion

    #region MainInfo_Send_Lookup_Data_Fun
    public class MainInfo_Send_Lookup_Data_Fun_Output
    {
        public string MAININFO_SEND_LOOKUP_DATA_FUN { get; set; }
    }

    public class MainInfo_Send_Lookup_Data_Fun
    {
        public List<Meal> Meals { get; set; }
        public List<Restaurant> Restaurant { get; set; }
        public List<Direction> Direction { get; set; }

        //public List<DeviceCategory> Device_Category { get; set; }
        //public List<DeviceType> Device_Type { get; set; }
        //public List<QueueNumber> Queue_Number { get; set; }

        public DateTime ServerDateTime { get; set; }
    }

    public class Meal
    {
        public string Cod_Data { get; set; }
        public string Des_Data { get; set; }

        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public bool IsCurrentMeal => StartTime.HasValue && EndTime.HasValue && DateTime.Now.TimeOfDay >= StartTime && DateTime.Now.TimeOfDay <= EndTime;
    }

    public class Device
    {
        public string Device_Category { get; set; }
        public string Device_Id { get; set; }
        public string Des_Device { get; set; }
        public string Cod_Device { get; set; }
    }

    public class Restaurant
    {
        public string Cod_Data { get; set; }
        public string Des_Data { get; set; }
        public List<Device> Devices { get; set; }

    }

    public class Direction
    {
        public string Cod_Data { get; set; }
        public string Des_Data { get; set; }
    }

    public class DeviceCategory
    {
        public string Cod_Data { get; set; }
        public string Des_Data { get; set; }
    }

    public class DeviceType
    {
        public string Cod_Data { get; set; }
        public string Des_Data { get; set; }
    }


    public class QueueNumber
    {
        public string Cod_Data { get; set; }
        public string Des_Data { get; set; }
    }
    #endregion

    #region MainInfo_Send_Offline_Data_Fun

    public class MainInfo_Send_Offline_Data_Fun_Input_Data
    {
        public string Device_Cod { get; set; }
        public string Date { get; set; }
        public string Cod_Meal { get; set; }

        public MainInfo_Send_Offline_Data_Fun_Input_Data()
        {

        }

        public MainInfo_Send_Offline_Data_Fun_Input_Data(string deviceCod, string date, string codMeal)
        {
            Device_Cod = deviceCod;
            Date = date;
            Cod_Meal = codMeal;
        }

        public string ToJsonString()
        {
            return "{\"JSON_CHARACTER\": \"[{\\\"DEVICE_COD\\\":\\\"" + Device_Cod + "\\\", \\\"DATE\\\":\\\"" + Date + "\\\", \\\"COD_MEAL\\\":\\\"" + Cod_Meal + "\\\"}]\" }";
        }
    }

    public class MainInfo_Send_Offline_Data_Fun_Input : BaseInput<MainInfo_Send_Offline_Data_Fun_Input_Data>
    {

    }

    public class MainInfo_Send_Offline_Data_Fun_Output
    {
        public string MAININFO_SEND_OFFLINE_DATA_FUN { get; set; }
    }
    public class MainInfo_Send_Offline_Data_Fun_Output_Data
    {
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

        public List<MainCourse> Main_Course { get; set; }
        public List<AppetizerCourse> Appetizer_Dessert { get; set; }

    }

    public class MainCourse
    {
        public string Des_Food { get; set; }
        public int Num_Amount { get; set; }
        public string Typ_Serv_Unit { get; set; }

        public MainCourse()
        {

        }

        public MainCourse(string desFood, int numAmount, string typServUnit)
        {
            Des_Food = desFood;
            Num_Amount = numAmount;
            Typ_Serv_Unit = typServUnit;
        }
    }

    public class AppetizerCourse
    {
        public string Des_Food { get; set; }
        public string Num_Amount { get; set; }
        public string Typ_Serv_Unit { get; set; }

        public AppetizerCourse()
        {

        }

        public AppetizerCourse(string desFood, string numAmount, string typServUnit)
        {
            Des_Food = desFood;
            Num_Amount = numAmount;
            Typ_Serv_Unit = typServUnit;
        }
    }
    #endregion

    #region MainInfo_Synchronize_Data_Fun

    public class MainInfo_Synchronize_Data_Fun_Input_Data
    {
        public string Device_Cod { get; set; }
        public string Reciver_Coupon_Id { get; set; }
        public string Status { get; set; }
        public string Date_Use { get; set; }
        public string Time_Use { get; set; }

        public MainInfo_Synchronize_Data_Fun_Input_Data()
        {

        }

        public MainInfo_Synchronize_Data_Fun_Input_Data(string deviceCod, string reciverCouponId, string status, string dateUse, string timeUse)
        {
            Device_Cod = deviceCod;
            Reciver_Coupon_Id = reciverCouponId;
            Status = status;
            Date_Use = dateUse;
            Time_Use = timeUse;
        }
    }

    public class MainInfo_Synchronize_Data_Fun_Input
    {
        public List<MainInfo_Synchronize_Data_Fun_Input_Data> Items { get; set; }

        public MainInfo_Synchronize_Data_Fun_Input()
        {

        }

        public MainInfo_Synchronize_Data_Fun_Input(List<MainInfo_Synchronize_Data_Fun_Input_Data> items)
        {
            Items = items;
        }

        public string ToJsonString()
        {
            var itemsStr = Items.Select(x =>

                "{\\\"Device_Cod\\\":\\\"" + x.Device_Cod + "\\\"," +
                " \\\"Receiver_Coupun_Id\\\":\\\"" + x.Reciver_Coupon_Id + "\\\"," +
                " \\\"Status\\\":\\\"" + x.Status + "\\\"," +
                " \\\"Date_Use\\\":\\\"" + x.Date_Use + "\\\"," +
                " \\\"Time_Use\\\":\\\"" + x.Time_Use + "\\\"}");

            return "{\"JSON_CHARACTER\": \"[" + itemsStr.Aggregate((a, b) => a + "," + b) + "]\" }";
        }
    }

    public class MainInfo_Synchronize_Data_Fun_Output
    {
        public string MAININFO_SYNCHRONIZE_DATA_FUN { get; set; }
    }

    #endregion

    #region RESTRN_QUEUE_HAVE_RESERVE_FUN

    public class RESTRN_QUEUE_HAVE_RESERVE_FUN_Input_Data
    {
        public string Device_Cod { get; set; }
        public string Num_Prsn { get; set; }

        public RESTRN_QUEUE_HAVE_RESERVE_FUN_Input_Data()
        {

        }

        public RESTRN_QUEUE_HAVE_RESERVE_FUN_Input_Data(string deviceCod, string numPrsn)
        {
            Device_Cod = deviceCod;
            Num_Prsn = numPrsn;
        }
        public string ToJsonString()
        {
            var itemsStr =

                "{\\\"DEVICE_COD\\\":\\\"" + Device_Cod + "\\\"," +
                " \\\"Num_Prsn\\\":\\\"" + Num_Prsn + "\\\"}";

            return "{\"JSON_CHARACTER\": \"[" + itemsStr + "]\" }";
        }
    }

    public class RESTRN_QUEUE_HAVE_RESERVE_FUN_Input : BaseInput<RESTRN_QUEUE_HAVE_RESERVE_FUN_Input_Data>
    {

    }

    public class RESTRN_QUEUE_HAVE_RESERVE_FUN_Output_Data
    {
        public string Receiver_Full_Name { get; set; }
        public string Dat_Day_Mepdy { get; set; }
        public string Des_Nam_Meal { get; set; }
        public string Des_Food_Order { get; set; }
        public string Reciver_Coupon_Id { get; set; }
    }

    public class RESTRN_QUEUE_HAVE_RESERVE_FUN_Output
    {
        public bool IsSuccessFull { get; set; }
        public List<ServerMessage> Messages { get; set; }
        public List<RESTRN_QUEUE_HAVE_RESERVE_FUN_Output_Data> Data { get; set; }

        public RESTRN_QUEUE_HAVE_RESERVE_FUN_Output()
        {

        }

        public RESTRN_QUEUE_HAVE_RESERVE_FUN_Output(bool isSuccessFull, List<ServerMessage> messages, List<RESTRN_QUEUE_HAVE_RESERVE_FUN_Output_Data> data)
        {
            IsSuccessFull = isSuccessFull;
            Messages = messages;
            Data = data;
        }
    }


    public class RESTRN_QUEUE_HAVE_RESERVE_FUN_Output_OnSuccess
    {
        public string RESTRN_QUEUE_HAVE_RESERVE_FUN { get; set; }
    }
    #endregion

    #region MAININFO_REGISTER_DEVICE_FUN

    public class MAININFO_REGISTER_DEVICE_FUN_Input_Data
    {
        public string Restaurant_Cod { get; set; }
        public string Device_Category { get; set; }
        public string Device_Type { get; set; }
        public string Device_Name { get; set; }
        public string Device_Cod { get; set; }
        public string IP { get; set; }
        public string Num_Queue { get; set; }
    }

    public class MAININFO_REGISTER_DEVICE_FUN_Input : BaseInput<MAININFO_REGISTER_DEVICE_FUN_Input_Data>
    {

        public MAININFO_REGISTER_DEVICE_FUN_Input()
        {

        }
        public MAININFO_REGISTER_DEVICE_FUN_Input(params MAININFO_REGISTER_DEVICE_FUN_Input_Data[] inputs)
        {
            foreach (var input in inputs)
            {
                JSON_CHARACTER.Add(input);
            }
        }
    }

    public class MAININFO_REGISTER_DEVICE_FUN_Output
    {
        public List<ServerMessage> MAININFO_REGISTER_DEVICE_FUN { get; set; }
    }

    #endregion

    #region MainInfo_User_Authenticate_Fun

    public class MainInfo_User_Authenticate_Fun_Input_Data
    {
        public string Num_prsn { get; set; }
        public string Cod_Serial { get; set; }
        public string Cod_Application { get; set; }
    }

    public class MainInfo_User_Authenticate_Fun_Input : BaseInput<MainInfo_User_Authenticate_Fun_Input_Data>
    {

    }

    public class MainInfo_User_Authenticate_Fun_Output
    {
        public List<ServerMessage> MainInfo_User_Authenticate_Fun { get; set; }
    }

    #endregion
}
