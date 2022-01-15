namespace IPAClient.Models
{
    public class ConfigModel
    {
        public string Restaurant_Cod { get; set; }
        public string Device_Category { get; set; }
        public string Device_Type { get; set; }
        public string Device_Name { get; set; }
        public string Device_Cod { get; set; }
        public string IP { get; set; }
        public string Num_Queue { get; set; }
        public string WebServiceUrl { get; set; }
        public string WebServiceAuthToken { get; set; }
        public bool CheckOnline { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsDemo { get; set; }
        public bool Logging { get; set; }
        public bool CheckMealTime { get; set; }

        public bool IsValid()
        {
            return
                !string.IsNullOrWhiteSpace(Restaurant_Cod) &&
                !string.IsNullOrWhiteSpace(Device_Category) &&
                !string.IsNullOrWhiteSpace(Device_Type) &&
                !string.IsNullOrWhiteSpace(Device_Name) &&
                !string.IsNullOrWhiteSpace(Device_Cod) &&
                !string.IsNullOrWhiteSpace(IP) &&
                !string.IsNullOrWhiteSpace(Num_Queue) &&
                !string.IsNullOrWhiteSpace(WebServiceAuthToken) &&
                !string.IsNullOrWhiteSpace(WebServiceUrl);
        }
    }
}
