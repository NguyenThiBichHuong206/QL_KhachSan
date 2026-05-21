using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using QLKhachsan.Models;

namespace QLKhachsan.Data
{
    public class HotelRepository
    {
        private readonly string _connectionString;

        public HotelRepository()
        {
            var setting = ConfigurationManager.ConnectionStrings["HotelDb"];
            _connectionString = setting == null ? string.Empty : setting.ConnectionString;
        }

        public List<Room> GetRooms()
        {
            if (string.IsNullOrWhiteSpace(_connectionString)
                || _connectionString.Contains("TEN_SERVER_CUA_BAN"))
            {
                return GetSampleRooms();
            }

            var rooms = new List<Room>();

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
SELECT
    MaPhong AS Id,
    TenPhong AS RoomNumber,
    LoaiPhong AS RoomType,
    TrangThai AS Status,
    GiaPhong AS PricePerNight
FROM dbo.Phong
ORDER BY TenPhong;", connection))
            {
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rooms.Add(new Room
                        {
                            Id = ReadInt(reader, "Id"),
                            RoomNumber = ReadString(reader, "RoomNumber"),
                            RoomType = ReadString(reader, "RoomType"),
                            Status = ReadString(reader, "Status"),
                            PricePerNight = ReadDecimal(reader, "PricePerNight")
                        });
                    }
                }
            }

            return rooms;
        }

        public List<Booking> GetRecentBookings()
        {
            if (string.IsNullOrWhiteSpace(_connectionString)
                || _connectionString.Contains("TEN_SERVER_CUA_BAN"))
            {
                return GetSampleBookings();
            }

            var bookings = new List<Booking>();

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
SELECT TOP 5
    dp.MaDatPhong AS Id,
    kh.HoTen AS CustomerName,
    p.TenPhong AS RoomNumber,
    dp.NgayNhanPhong AS CheckInDate,
    dp.NgayTraPhong AS CheckOutDate,
    dp.TrangThai AS Status
FROM dbo.DatPhong dp
INNER JOIN dbo.KhachHang kh ON kh.MaKhachHang = dp.MaKhachHang
INNER JOIN dbo.Phong p ON p.MaPhong = dp.MaPhong
ORDER BY dp.NgayNhanPhong DESC;", connection))
            {
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bookings.Add(new Booking
                        {
                            Id = ReadInt(reader, "Id"),
                            CustomerName = ReadString(reader, "CustomerName"),
                            RoomNumber = ReadString(reader, "RoomNumber"),
                            CheckInDate = ReadDateTime(reader, "CheckInDate"),
                            CheckOutDate = ReadDateTime(reader, "CheckOutDate"),
                            Status = ReadString(reader, "Status")
                        });
                    }
                }
            }

            return bookings;
        }

        private static int ReadInt(IDataRecord reader, string name)
        {
            var value = reader[name];
            return value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        private static string ReadString(IDataRecord reader, string name)
        {
            var value = reader[name];
            return value == DBNull.Value ? string.Empty : Convert.ToString(value);
        }

        private static decimal ReadDecimal(IDataRecord reader, string name)
        {
            var value = reader[name];
            return value == DBNull.Value ? 0 : Convert.ToDecimal(value);
        }

        private static DateTime ReadDateTime(IDataRecord reader, string name)
        {
            var value = reader[name];
            return value == DBNull.Value ? DateTime.Today : Convert.ToDateTime(value);
        }

        private static List<Room> GetSampleRooms()
        {
            return new List<Room>
            {
                new Room { Id = 1, RoomNumber = "101", RoomType = "Standard", Status = "Trong", PricePerNight = 650000 },
                new Room { Id = 2, RoomNumber = "203", RoomType = "Deluxe", Status = "Dang o", PricePerNight = 950000 },
                new Room { Id = 3, RoomNumber = "305", RoomType = "Suite", Status = "Cho don", PricePerNight = 1800000 },
                new Room { Id = 4, RoomNumber = "407", RoomType = "Family", Status = "Dang don dep", PricePerNight = 1250000 },
                new Room { Id = 5, RoomNumber = "512", RoomType = "Premium", Status = "Dat truoc", PricePerNight = 1450000 }
            };
        }

        private static List<Booking> GetSampleBookings()
        {
            return new List<Booking>
            {
                new Booking { Id = 1, CustomerName = "Tran Quoc Bao", RoomNumber = "203", CheckInDate = DateTime.Today, CheckOutDate = DateTime.Today.AddDays(2), Status = "Da nhan" },
                new Booking { Id = 2, CustomerName = "Le Hoai Nam", RoomNumber = "305", CheckInDate = DateTime.Today, CheckOutDate = DateTime.Today.AddDays(1), Status = "Cho don" },
                new Booking { Id = 3, CustomerName = "Pham Linh Chi", RoomNumber = "512", CheckInDate = DateTime.Today.AddDays(1), CheckOutDate = DateTime.Today.AddDays(3), Status = "Dat truoc" }
            };
        }
    }
}
