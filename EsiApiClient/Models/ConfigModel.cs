using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsiApiClient.Models
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
        public bool CheckOnline { get; set; }
        public bool IsConfirmed { get; set; }

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
                !string.IsNullOrWhiteSpace(WebServiceUrl);
        }
    }
}
