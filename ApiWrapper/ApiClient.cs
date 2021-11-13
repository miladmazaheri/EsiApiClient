using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EsiApiClient.Api.Dto;

namespace EsiApiClient.Api
{
    public static class ApiClient
    {
        private static string baseUrl;
        private static HttpClient httpClient = new HttpClient();
        /// <summary>
        /// تنظیم آدرس اصلی سرور
        /// </summary>
        /// <param name="url"></param>
        public static void SetBaseUrl(string url)
        {
            if (!url.EndsWith("/")) url += "/";
            baseUrl = url;
        }
        /// <summary>
        /// تنظیم توکن احراز هویت
        /// </summary>
        /// <param name="token"></param>
        public static void SetAuthToken(string token)
        {
            var key = "Authorization";
            if (httpClient.DefaultRequestHeaders.Any(x => x.Key == key))
            {
                httpClient.DefaultRequestHeaders.Remove(key);
            }
            httpClient.DefaultRequestHeaders.Add(key, token);
        }
        /// <summary>
        /// جهت تعیین وعده غذایی توسط کاربر جهت دریافت لیست افراد دریافت کننده
        /// </summary>
        public static async Task<MainInfo_Send_Lookup_Data_Fun_Output> MainInfo_Send_Lookup_Data_Fun()
        {
            try
            {
                var response = await httpClient.PostAsync($"{baseUrl}osb/namfood/restservices/MainInfo_Send_Lookup_Data_Fun", null);
                var resAsString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<MainInfo_Send_Lookup_Data_Fun_Output>(resAsString);
            }
            catch (Exception)
            {
                //TODO : Log
                return null;
            }
        }
        /// <summary>
        /// ارسال اطلاعات رزرواسیون سرویس گیرنده ها در حالت آفلاین
        /// </summary>
        public static async Task<MainInfo_Send_Offline_Data_Fun_Output> MainInfo_Send_Offline_Data_Fun(MainInfo_Send_Offline_Data_Fun_Input input)
        {
            try
            {
                var response = await httpClient.PostAsync($"{baseUrl}osb/namfood/restservices/MainInfo_Send_Offline_Data_Fun", new StringContent(JsonSerializer.Serialize(input, new JsonSerializerOptions(JsonSerializerDefaults.Web))));
                var resAsString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<MainInfo_Send_Offline_Data_Fun_Output>(resAsString);
            }
            catch (Exception)
            {
                //TODO : Log
                return null;
            }
        }
        /// <summary>
        /// همگام سازی اطلاعات Client
        /// </summary>
        public static async Task<MainInfo_Synchronize_Data_Fun_Output> MainInfo_Synchronize_Data_Fun(MainInfo_Synchronize_Data_Fun_Input input)
        {
            try
            {
                var response = await httpClient.PostAsync($"{baseUrl}osb/namfood/restservices/MainInfo_Synchronize_Data_Fun", new StringContent(JsonSerializer.Serialize(input, new JsonSerializerOptions(JsonSerializerDefaults.Web))));
                var resAsString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<MainInfo_Synchronize_Data_Fun_Output>(resAsString);
            }
            catch (Exception)
            {
                //TODO : Log
                return null;
            }
        }
        /// <summary>
        /// ثبت اطلاعات تجهیز جدید
        /// </summary>
        public static async Task<MAININFO_REGISTER_DEVICE_FUN_Output> Maininfo_Register_Device_Fun(MAININFO_REGISTER_DEVICE_FUN_Input input)
        {
            try
            {
                var response = await httpClient.PostAsync($"{baseUrl}osb/namfood/restservices/Maininfo_Register_Device_Fun", new StringContent(JsonSerializer.Serialize(input, new JsonSerializerOptions(JsonSerializerDefaults.Web))));
                var resAsString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<MAININFO_REGISTER_DEVICE_FUN_Output>(resAsString);
            }
            catch (Exception)
            {
                //TODO : Log
                return null;
            }
        }
        /// <summary>
        /// بررسی وضعیت دسترسی به برنامه
        /// </summary>
        public static async Task<MainInfo_User_Authenticate_Fun_Output> MainInfo_User_Authenticate_Fun(MainInfo_User_Authenticate_Fun_Input input)
        {
            try
            {
                var response = await httpClient.PostAsync($"{baseUrl}osb/namfood/restservices/MainInfo_User_Authenticate_Fun", new StringContent(JsonSerializer.Serialize(input, new JsonSerializerOptions(JsonSerializerDefaults.Web))));
                var resAsString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<MainInfo_User_Authenticate_Fun_Output>(resAsString);
            }
            catch (Exception)
            {
                //TODO : Log
                return null;
            }
        }
        /// <summary>
        /// بررسی رزرو افراد در حالت آنلاین
        /// </summary>
        public static async Task<RESTRN_QUEUE_HAVE_RESERVE_FUN_Output> Restrn_Queue_Have_Reserve_Fun(RESTRN_QUEUE_HAVE_RESERVE_FUN_Input input)
        {
            try
            {
                var response = await httpClient.PostAsync($"{baseUrl}osb/namfood/restservices/Restrn_Queue_Have_Reserve_Fun", new StringContent(JsonSerializer.Serialize(input, new JsonSerializerOptions(JsonSerializerDefaults.Web))));
                var resAsString = await response.Content.ReadAsStringAsync();
                try
                {
                    var res = JsonSerializer.Deserialize<RESTRN_QUEUE_HAVE_RESERVE_FUN_Output_OnSuccess>(resAsString);
                    return new RESTRN_QUEUE_HAVE_RESERVE_FUN_Output(true, null, res);
                }
                catch (Exception e)
                {
                    var res = JsonSerializer.Deserialize<RESTRN_QUEUE_HAVE_RESERVE_FUN_Output_OnFail>(resAsString);
                    return new RESTRN_QUEUE_HAVE_RESERVE_FUN_Output(true, res, null);
                }
            }
            catch (Exception)
            {
                //TODO : Log
                return null;
            }
        }
    }
}
