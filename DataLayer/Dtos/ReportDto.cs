using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Dtos
{
    public class ReportDto
    {
        public string Date { get; set; }
        public string Meal { get; set; }
        public int ReserveCount { get; set; }
        public int DeliveredCount { get; set; }
        public int SentCount { get; set; }

    }
}
