using ApiWrapper.Dto;
using DataLayer.Entities;
using IPAClient.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IPAClient.Models
{
    public class RemainFoodModel
    {
        public string Title { get; set; }
        public int Remain { get; set; }
        public int Total { get; set; }

        public RemainFoodModel()
        {

        }
        public RemainFoodModel(string title, int remain, int total)
        {
            Title = title;
            Remain = remain;
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

            MainFoods = reservation.Main_Course;
            MainFoods = reservation.Appetizer_Dessert;
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
        public string WRPN { get; set; }
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
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
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
                    item.Remain = food.Remain;
                }

            }
        }

        public void SetCommand(string command)
        {
            Command = command;
        }
    }
}
