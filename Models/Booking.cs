using System;

namespace QLKhachsan.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string RoomNumber { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; }

        public string Summary
        {
            get
            {
                var nights = Math.Max(1, (CheckOutDate.Date - CheckInDate.Date).Days);
                return "Phong " + RoomNumber + " - " + nights + " dem";
            }
        }
    }
}
