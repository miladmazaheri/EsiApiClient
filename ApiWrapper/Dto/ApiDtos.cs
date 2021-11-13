﻿using System.CodeDom;
using System.Collections.Generic;

namespace EsiApiClient.Api.Dto
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
        public MainInfo_Send_Lookup_Data_Fun MainInfo_Send_Lookup_Data_Fun { get; set; }
    }

    public class MainInfo_Send_Lookup_Data_Fun
    {
        public List<Meals> Meals { get; set; }
    }

    public class Meals
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
    }

    public class MainInfo_Send_Offline_Data_Fun_Input : BaseInput<MainInfo_Send_Offline_Data_Fun_Input_Data>
    {

    }

    public class MainInfo_Send_Offline_Data_Fun_Output
    {
        public List<MainInfo_Send_Offline_Data_Fun_Output_Data> MainInfoSendOfflineDataFun { get; set; }
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

        public List<Course> Main_Course { get; set; }
        public List<Course> Appetizer_Dessert { get; set; }

        public MainInfo_Send_Offline_Data_Fun_Output_Data()
        {

        }

        public MainInfo_Send_Offline_Data_Fun_Output_Data(string receiverMealPlanDayId, string reciverCouponId, string mealPlanDayId, string numIde, string firstNameIde, string lastNameIde, string codSerial, string codContractOrder, string desContractOrder, string employeeShiftName, string datDayMepdy, string desNamMeal, string numTimStrMealRsmls, string numTimEndMealRsmls, string desNamResturantRstm, string numTotCouponRccpn, string desFoodOrderMepdy, string lkpCodOrderMepdyMeans, string codResturant, string codCoupon, List<Course> mainCourse, List<Course> appetizerDessert)
        {
            Receiver_Meal_Plan_Day_Id = receiverMealPlanDayId;
            Reciver_Coupon_Id = reciverCouponId;
            Meal_Plan_Day_Id = mealPlanDayId;
            Num_Ide = numIde;
            First_Name_Ide = firstNameIde;
            Last_Name_Ide = lastNameIde;
            Cod_Serial = codSerial;
            Cod_Contract_Order = codContractOrder;
            Des_Contract_Order = desContractOrder;
            Employee_Shift_Name = employeeShiftName;
            Dat_Day_Mepdy = datDayMepdy;
            Des_Nam_Meal = desNamMeal;
            Num_Tim_Str_Meal_Rsmls = numTimStrMealRsmls;
            Num_Tim_End_Meal_Rsmls = numTimEndMealRsmls;
            Des_Nam_Resturant_Rstm = desNamResturantRstm;
            Num_Tot_Coupon_Rccpn = numTotCouponRccpn;
            Des_Food_Order_Mepdy = desFoodOrderMepdy;
            Lkp_Cod_Order_Mepdy_Means = lkpCodOrderMepdyMeans;
            Cod_Resturant = codResturant;
            Cod_Coupon = codCoupon;
            Main_Course = mainCourse;
            Appetizer_Dessert = appetizerDessert;
        }

    }

    public class Course
    {
        public string Des_Food { get; set; }
        public string Num_Amount { get; set; }
        public string Typ_Serv_Unit { get; set; }

        public Course()
        {

        }

        public Course(string desFood, string numAmount, string typServUnit)
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

    public class MainInfo_Synchronize_Data_Fun_Input : BaseInput<MainInfo_Synchronize_Data_Fun_Input_Data>
    {

    }

    public class MainInfo_Synchronize_Data_Fun_Output
    {
        public ServerMessage MainInfo_Synchronize_Data_Fun { get; set; }
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
        public RESTRN_QUEUE_HAVE_RESERVE_FUN_Output_OnFail FailOutput { get; set; }
        public RESTRN_QUEUE_HAVE_RESERVE_FUN_Output_OnSuccess SuccessOutput { get; set; }

        public RESTRN_QUEUE_HAVE_RESERVE_FUN_Output()
        {
            
        }

        public RESTRN_QUEUE_HAVE_RESERVE_FUN_Output(bool isSuccessFull, RESTRN_QUEUE_HAVE_RESERVE_FUN_Output_OnFail failOutput, RESTRN_QUEUE_HAVE_RESERVE_FUN_Output_OnSuccess successOutput)
        {
            IsSuccessFull = isSuccessFull;
            FailOutput = failOutput;
            SuccessOutput = successOutput;
        }
    }

    public class RESTRN_QUEUE_HAVE_RESERVE_FUN_Output_OnFail
    {
        public List<ServerMessage> RESTRN_QUEUE_HAVE_RESERVE_FUN { get; set; }
    }

    public class RESTRN_QUEUE_HAVE_RESERVE_FUN_Output_OnSuccess
    {
        public List<RESTRN_QUEUE_HAVE_RESERVE_FUN_Output_Data> RESTRN_QUEUE_HAVE_RESERVE_FUN { get; set; }
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