using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ApiWrapper.Dto;
using Microsoft.Extensions.Logging;

namespace ApiWrapper
{
    public static class ApiClient
    {
        private static string baseUrl;
        private static readonly HttpClient httpClient = new HttpClient();
        private static ILogger _logger;


        public static void SetLogger(ILogger logger)
        {
            _logger = logger;
        }
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
        public static async Task<MainInfo_Send_Lookup_Data_Fun> MainInfo_Send_Lookup_Data_Fun()
        {
            try
            {
                var response = await httpClient.PostAsync($"{baseUrl}osb/namfood/restservices/MainInfo_Send_Lookup_Data_Fun", new StringContent("{\"JSON_CHARACTER\":\"[]\"}"));
                var resAsString = await response.Content.ReadAsStringAsync();
                var res1 = JsonSerializer.Deserialize<MainInfo_Send_Lookup_Data_Fun_Output>(resAsString);
                if (res1 != null && string.IsNullOrWhiteSpace(res1.MainInfo_Send_Lookup_Data_Fun)) return null;
                var res2 = JsonSerializer.Deserialize<MainInfo_Send_Lookup_Data_Fun>(res1.MainInfo_Send_Lookup_Data_Fun);
                if (res2 == null) return null;

                //Read Server Date Time From Response Header
                if (response.Headers.TryGetValues("Date", out var dateValues))
                {
                    var dateStr = dateValues.First();
                    if (DateTime.TryParse(dateStr, out var date))
                    {
                        res2.ServerDateTime = date;
                    }
                    else
                    {
                        res2.ServerDateTime = DateTime.Now;
                    }
                }
                else
                {
                    res2.ServerDateTime = DateTime.Now;
                }
                return res2;
            }
            catch (Exception ex)
            {
                AddLog(ex);
                return null;
            }
        }
        /// <summary>
        /// ارسال اطلاعات رزرواسیون سرویس گیرنده ها در حالت آفلاین
        /// </summary>
        public static async Task<List<MainInfo_Send_Offline_Data_Fun_Output_Data>> MainInfo_Send_Offline_Data_Fun(MainInfo_Send_Offline_Data_Fun_Input_Data input)
        {
            try
            {
                var response = await httpClient.PostAsync($"{baseUrl}osb/namfood/restservices/MainInfo_Send_Offline_Data_Fun", new StringContent(input.ToJsonString()));
                var resAsString = await response.Content.ReadAsStringAsync();
                var res1 = JsonSerializer.Deserialize<MainInfo_Send_Offline_Data_Fun_Output>(resAsString);
                if (res1 != null && string.IsNullOrWhiteSpace(res1.MainInfoSendOfflineDataFun)) return null;
                return JsonSerializer.Deserialize<List<MainInfo_Send_Offline_Data_Fun_Output_Data>>(res1.MainInfoSendOfflineDataFun);
            }
            catch (Exception ex)
            {
                AddLog(ex);
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
                var response = await httpClient.PostAsync($"{baseUrl}osb/namfood/restservices/MainInfo_Synchronize_Data_Fun", new StringContent(JsonSerializer.Serialize(input)));
                var resAsString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<MainInfo_Synchronize_Data_Fun_Output>(resAsString);
            }
            catch (Exception ex)
            {
                AddLog(ex);
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
                var response = await httpClient.PostAsync($"{baseUrl}osb/namfood/restservices/Maininfo_Register_Device_Fun", new StringContent(JsonSerializer.Serialize(input)));
                var resAsString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<MAININFO_REGISTER_DEVICE_FUN_Output>(resAsString);
            }
            catch (Exception ex)
            {
                AddLog(ex);
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
                var response = await httpClient.PostAsync($"{baseUrl}osb/namfood/restservices/MainInfo_User_Authenticate_Fun", new StringContent(JsonSerializer.Serialize(input)));
                var resAsString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<MainInfo_User_Authenticate_Fun_Output>(resAsString);
            }
            catch (Exception ex)
            {
                AddLog(ex);
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
                var response = await httpClient.PostAsync($"{baseUrl}osb/namfood/restservices/Restrn_Queue_Have_Reserve_Fun", new StringContent(JsonSerializer.Serialize(input)));
                var resAsString = await response.Content.ReadAsStringAsync();
                try
                {
                    var res = JsonSerializer.Deserialize<RESTRN_QUEUE_HAVE_RESERVE_FUN_Output_OnSuccess>(resAsString);
                    return new RESTRN_QUEUE_HAVE_RESERVE_FUN_Output(true, null, res);
                }
                catch (Exception e)
                {
                    AddLog(e);
                    var res = JsonSerializer.Deserialize<RESTRN_QUEUE_HAVE_RESERVE_FUN_Output_OnFail>(resAsString);
                    return new RESTRN_QUEUE_HAVE_RESERVE_FUN_Output(true, res, null);
                }
            }
            catch (Exception ex)
            {
                AddLog(ex);
                return null;
            }
        }


        private static void AddLog(Exception ex)
        {
            _logger?.LogError(ex, $"ApiError-{(new StackFrame(1, true).GetMethod()?.Name ?? "")}");
        }

    }
}
