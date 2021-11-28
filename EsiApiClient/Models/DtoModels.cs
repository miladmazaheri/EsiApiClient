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
        public delegate void QueueEventHandler(MonitorDto dto, string json);
        public event QueueEventHandler QueueChange;

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
            QueueChange?.Invoke(this, ToJson());
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
    }
}
