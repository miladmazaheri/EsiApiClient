﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.Dtos;
using DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace DataLayer.Services
{
    public class ReservationService
    {
        private readonly EsiDbContext _context;

        public ReservationService()
        {
            _context = new EsiDbContext();
            _context.Database.Migrate();
        }

        public async Task<Reservation> FindReservationAsync(string personnelNumber, string currentMealCode, string date)
        {
            var reservationExist = await _context.Reservations.Include(x => x.Foods).FirstOrDefaultAsync(x =>
                  x.Num_Ide == personnelNumber
                  && x.Cod_Meal == currentMealCode
                  && x.Dat_Day_Mepdy == date
            //&& string.IsNullOrWhiteSpace(x.Status)
            );
            if (reservationExist != null)
            {
                var time = DateTime.Now.TimeOfDay.ToString("hhmmss");
                await _context.Reservations.Where(x => x.Id == reservationExist.Id && x.Status == null).UpdateAsync(x => new Reservation()
                {
                    Status = ReservationStatusEnum.USED.ToString(),
                    Date_Use = date,
                    Time_Use = time
                });
            }
            return reservationExist;
        }

        public async Task<List<(string Title, int Total, int Remain)>> GetMealFoodRemain(string date, string mealCode)
        {
            var data = await _context.Reservations
                .Where(x => x.Dat_Day_Mepdy == date && x.Cod_Meal == mealCode && x.Foods.Any())
                .Select(x => new { Title = x.Foods.First(food => food.IsMain).Des_Food, x.Status }).ToListAsync();
            return data.GroupBy(x => x.Title)
                .Select(x => (x.Key, x.Count(), x.Count(w => string.IsNullOrWhiteSpace(w.Status)))).ToList();
        }

        public async Task InsertAsync(List<Reservation> reservations)
        {
            foreach (var reservation in reservations) await InsertIfNotExistAsync(reservation);
            await _context.SaveChangesAsync();
        }

        private async Task InsertIfNotExistAsync(Reservation reservation)
        {
            var reservationExist = await _context.Reservations.FirstOrDefaultAsync(x =>
                x.Reciver_Coupon_Id == reservation.Reciver_Coupon_Id && x.Num_Ide == reservation.Num_Ide);
            if (reservationExist == null) await _context.Reservations.AddAsync(reservation);
        }

        public async Task<List<(Guid Id, string Reciver_Coupon_Id, string Status, string Date_Use, string Time_Use)>> GetDeliveredReservesToSendAsync()
        {
            return (await _context.Reservations.AsNoTracking()
                .Where(x => !string.IsNullOrWhiteSpace(x.Status) && !x.DateTime_SentToWebService.HasValue)
                .Select(x => new { x.Id, x.Reciver_Coupon_Id, x.Status, x.Date_Use, x.Time_Use })
                .Skip(0).Take(100)
                .ToListAsync()).Select(x => (x.Id, x.Reciver_Coupon_Id, x.Status, x.Date_Use, x.Time_Use)).ToList();
        }

        public async Task<bool> HasAnyNotSentToServer()
        {
            return await _context.Reservations.AnyAsync(x => !string.IsNullOrWhiteSpace(x.Status) && !x.DateTime_SentToWebService.HasValue);
        }

        public async Task SetSentToWebServiceDateTimeAsync(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                var reserve = _context.Reservations.FirstOrDefault(x => x.Id == id);
                if (reserve != null)
                {
                    reserve.DateTime_SentToWebService = DateTime.Now;
                }
            }
            await _context.SaveChangesAsync();
        }


        public async Task<Reservation> FindReservationByCouponIdAsync(string reciverCouponId, string date)
        {
            var data = await _context.Reservations.FirstOrDefaultAsync(x => x.Reciver_Coupon_Id == reciverCouponId);
            if (data != null)
            {
                data.Status = ReservationStatusEnum.USED.ToString();
                data.Date_Use = date;
                data.Time_Use = DateTime.Now.TimeOfDay.ToString("hhmmss");
                _context.Reservations.Update(data);
                await _context.SaveChangesAsync();
            }

            return data;
        }

        public async Task<List<ReportDto>> GetReportAsync()
        {
            return await _context.Reservations.OrderByDescending(x => x.Dat_Day_Mepdy).GroupBy(x => new { x.Dat_Day_Mepdy, x.Des_Nam_Meal })
                .Select(x => new ReportDto()
                {
                    Date = x.Key.Dat_Day_Mepdy,
                    Meal = x.Key.Des_Nam_Meal,
                    ReserveCount = x.Count(),
                    DeliveredCount = x.Count(c => c.Status != null),
                    SentCount = x.Count(c => c.DateTime_SentToWebService.HasValue)
                }).ToListAsync();
        }

        public async Task DeleteAllSendAsync()
        {
            await _context.Reservations.Where(x => x.DateTime_SentToWebService.HasValue).DeleteAsync();
        }
        public async Task DeleteAllSendByDayAsync(int days)
        {
            var date = DateTime.Now.AddDays(-1 * days);
            await _context.Reservations.Where(x => x.DateTime_SentToWebService < date).DeleteAsync();
        }

    }
}