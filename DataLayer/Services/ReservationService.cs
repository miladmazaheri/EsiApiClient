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

    }
}
