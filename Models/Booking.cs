using System;

namespace QLKhachsan.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string RoomNumber { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public string Status { get; set; }

        public string Summary
        {
            get
            {
                var checkOutDate = CheckOutDate.HasValue ? CheckOutDate.Value.Date : DateTime.Today;
                var nights = Math.Max(1, (checkOutDate - CheckInDate.Date).Days);
                return "Phong " + RoomNumber + " - " + nights + " dem";
            }
        }
    }
}
