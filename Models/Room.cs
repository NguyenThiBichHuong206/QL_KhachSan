using System;

namespace QLKhachsan.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public string RoomType { get; set; }
        public string Status { get; set; }
        public int StatusValue { get; set; }
        public decimal PricePerNight { get; set; }
        public string ImagePath { get; set; }

        public string PriceText
        {
            get { return PricePerNight.ToString("N0"); }
        }

        public bool IsAvailable
        {
            get { return StatusValue == 0 || string.Equals(Status, "Trong", StringComparison.OrdinalIgnoreCase); }
        }

        public bool IsOccupied
        {
            get { return StatusValue == 1 || string.Equals(Status, "Dang thue", StringComparison.OrdinalIgnoreCase); }
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
