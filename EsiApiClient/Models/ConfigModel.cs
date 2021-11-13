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
        public bool IsConfirmed { get; set; }
    }
}
