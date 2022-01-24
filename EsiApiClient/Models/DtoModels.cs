using DataLayer.Entities;
using IPAClient.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace IPAClient.Models
{
    public class RemainFoodModel
    {
        public string Title { get; set; }
        public int Delivered { get; set; }
        public int Total { get; set; }

        public RemainFoodModel()
        {

        }
        public RemainFoodModel(string title, int delivered, int total)
        {
            Title = title;
            Delivered = delivered;
            Total = total;
        }


    }

    public class PersonnelFoodDto
    {
        public string FullName { get; set; }
        public string PersonnelNumber { get; set; }
        public string Meal { get; set; }
        public string Shift { get; set; }
        public string Company { get; set; }
        public List<Food> MainFoods { get; set; }
        public List<Food> SubsidiaryFoods { get; set; }
        public TimeSpan DeliveryTime { get; set; }
        public PersonnelFoodDto()
        {

        }

        public PersonnelFoodDto(Reservation reservation)
        {
            FullName = reservation.First_Name_Ide + " " + reservation.Last_Name_Ide;
            PersonnelNumber = reservation.Num_Ide;
            Meal = reservation.Des_Nam_Meal;
            Shift = reservation.Employee_Shift_Name;
            Company = reservation.Des_Contract_Order;
            DeliveryTime = DateTime.Now.TimeOfDay;
            MainFoods = reservation.Foods.Where(x => x.IsMain).ToList();
            SubsidiaryFoods = reservation.Foods.Where(x => !x.IsMain).ToList();
            if (MainFoods.Count < 1)
            {
                MainFoods.Add(new Food()
                {
                    Des_Food = string.Empty,
                    IsMain = true,
                    Num_Amount = string.Empty,
                    Typ_Serv_Unit = string.Empty,
                    Id = 0
                });
            }
            if (SubsidiaryFoods.Count < 6)
            {
                for (var i = 0; i < (6 - SubsidiaryFoods.Count); i++)
                {
                    SubsidiaryFoods.Add(new Food()
                    {
                        Des_Food = string.Empty,
                        IsMain = false,
                        Num_Amount = string.Empty,
                        Typ_Serv_Unit = string.Empty,
                        Id = 0
                    });
                }
            }
        }
        public PersonnelFoodDto(string noReservePersonnelCode, string message)
        {
            PersonnelNumber = noReservePersonnelCode;
            DeliveryTime = DateTime.Now.TimeOfDay;
            MainFoods = new List<Food>(){new Food()
            {
                Des_Food = message,
            }};
        }

    }

    public class MonitorDto
    {
        /// <summary>
        /// 1 = F1 اتمام غذا
        /// 2 = F2 در حال آماده سازی
        /// 3 = F3
        /// 4 = F4
        /// </summary>
        public string Command { get; private set; } = string.Empty;
        /// <summary>
        /// Without Reserve Personnel Number
        /// </summary>
        public string CurrentDateTime => SystemTimeHelper.CurrentPersinaFullDateTime();
        public TimeSpan CurrentMealRemainTime { get; set; }

        public List<RemainFoodModel> RemainFoods { get; set; }
        public List<PersonnelFoodDto> PersonnelFoods { get; private set; }
        public MonitorDto()
        {
            RemainFoods = new List<RemainFoodModel>();
            PersonnelFoods = new List<PersonnelFoodDto>();
        }

        public void AddToQueue(Reservation reservation)
        {
            if (PersonnelFoods.Count == 5)
            {
                PersonnelFoods.Remove(PersonnelFoods[0]);
            }
            PersonnelFoods.Add(new PersonnelFoodDto(reservation));
        }

        public void AddMessageToQueue(string personnelCode, string message)
        {
            if (PersonnelFoods.Count == 5)
            {
                PersonnelFoods.Remove(PersonnelFoods[0]);
            }
            PersonnelFoods.Add(new PersonnelFoodDto(personnelCode, message));
        }
        public string ToJson()
        {
            var options = new JsonSerializerOptions() { WriteIndented = false };//, ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve };
            return JsonSerializer.Serialize(this, options).Replace("\r\n", " ") + "\n";
        }

        public void InsertOrUpdateRemainFood(params RemainFoodModel[] remainFoods)
        {
            foreach (var food in remainFoods)
            {
                var item = RemainFoods.FirstOrDefault(x => x.Title == food.Title);
                if (item == null)
                {
                    RemainFoods.Add(food);
                }
                else
                {
                    item.Total = food.Total;
                    item.Delivered = food.Delivered;
                }

            }
        }

        public void SetCommand(string command)
        {
            Command = command;
        }

        public void Clear()
        {
            RemainFoods = new List<RemainFoodModel>();
            PersonnelFoods = new List<PersonnelFoodDto>();
        }
    }
}
