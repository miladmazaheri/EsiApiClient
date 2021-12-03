using DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task InsertAsync(List<Reservation> reservations)
        {
            foreach (var reservation in reservations)
            {
                await InsertIfNotExistAsync(reservation);
            }
            await _context.SaveChangesAsync();
        }
        private async Task InsertIfNotExistAsync(Reservation reservation)
        {
            var reservationExist = await _context.Reservations.FirstOrDefaultAsync(x => x.Reciver_Coupon_Id == reservation.Reciver_Coupon_Id && x.Num_Ide == reservation.Num_Ide);
            if (reservationExist == null)
            {
                await _context.Reservations.AddAsync(reservation);
            }
        }

        public async Task<Reservation> FindReservationAsync(string personnelNumber, string currentMealCode, string date)
        {
            var reservationExist = await _context.Reservations.FirstOrDefaultAsync(x =>
            x.Num_Ide == personnelNumber
            && x.Cod_Meal == currentMealCode
            && x.Dat_Day_Mepdy == date
            && string.IsNullOrWhiteSpace(x.Status)
            );
            if (reservationExist != null)
            {
                reservationExist.Status = ReservationStatusEnum.USED.ToString();
                reservationExist.Date_Use = date;
                reservationExist.Time_Use = DateTime.Now.TimeOfDay.ToString(format: "hhmmss");
                _context.Reservations.Update(reservationExist);
                await _context.SaveChangesAsync();
            }
            return reservationExist;
        }

        public async Task<List<(string Title, int Total, int Remain)>> GetMealFoodRemain(string date, string mealCode)
        {
            var data = await _context.Reservations.Where(x => x.Dat_Day_Mepdy == date && x.Cod_Meal == mealCode).Select(x => new { Title = x.Main_Course.First().Des_Food, Status = x.Status }).ToListAsync();
            return data.GroupBy(x => x.Title).Select(x => (x.Key, x.Count(), x.Where(w => string.IsNullOrWhiteSpace(w.Status)).Count())).ToList();
        }
    }
}
