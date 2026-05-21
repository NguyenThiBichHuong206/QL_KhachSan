using System;

namespace QLKhachsan.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public string RoomType { get; set; }
        public string Status { get; set; }
        public decimal PricePerNight { get; set; }

        public string PriceText
        {
            get { return PricePerNight.ToString("N0"); }
        }

        public bool IsAvailable
        {
            get { return string.Equals(Status, "Trong", StringComparison.OrdinalIgnoreCase); }
        }

        public bool IsOccupied
        {
            get { return string.Equals(Status, "Dang o", StringComparison.OrdinalIgnoreCase); }
        }

        public bool IsWaitingCheckIn
        {
            get
            {
                return string.Equals(Status, "Cho don", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(Status, "Cho nhan phong", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
