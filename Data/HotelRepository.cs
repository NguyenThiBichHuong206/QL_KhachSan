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
            if (UseSampleData)
            {
                return GetSampleRooms();
            }

            var rooms = new List<Room>();

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
SELECT
    p.MaPhong AS Id,
    p.TenPhong AS RoomNumber,
    lp.TenLoai AS RoomType,
    p.TrangThai AS StatusValue,
    CASE p.TrangThai
        WHEN 1 THEN N'Dang thue'
        ELSE N'Trong'
    END AS Status,
    lp.DonGia AS PricePerNight,
    p.HinhAnh AS ImagePath
FROM dbo.Phong p
LEFT JOIN dbo.LoaiPhong lp ON lp.MaLoai = p.MaLoai
ORDER BY p.TenPhong;", connection))
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
                            StatusValue = ReadInt(reader, "StatusValue"),
                            Status = ReadString(reader, "Status"),
                            PricePerNight = ReadDecimal(reader, "PricePerNight"),
                            ImagePath = ReadString(reader, "ImagePath")
                        });
                    }
                }
            }

            return rooms;
        }

        public List<Booking> GetRecentBookings()
        {
            if (UseSampleData)
            {
                return GetSampleBookings();
            }

            var bookings = new List<Booking>();

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
SELECT TOP 5
    hd.MaHD AS Id,
    kh.HoTen AS CustomerName,
    p.TenPhong AS RoomNumber,
    hd.NgayCheckIn AS CheckInDate,
    hd.NgayCheckOut AS CheckOutDate,
    CASE hd.TrangThaiThanhToan
        WHEN 1 THEN N'Da thanh toan'
        ELSE N'Chua thanh toan'
    END AS Status
FROM dbo.HoaDon hd
LEFT JOIN dbo.KhachHang kh ON kh.MaKH = hd.MaKH
LEFT JOIN dbo.Phong p ON p.MaPhong = hd.MaPhong
ORDER BY hd.NgayCheckIn DESC;", connection))
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
                            CheckOutDate = ReadNullableDateTime(reader, "CheckOutDate"),
                            Status = ReadString(reader, "Status")
                        });
                    }
                }
            }

            return bookings;
        }

        public decimal GetTodayRevenue()
        {
            if (UseSampleData)
            {
                return 38500000;
            }

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
SELECT ISNULL(SUM(TongTien), 0)
FROM dbo.HoaDon
WHERE TrangThaiThanhToan = 1
  AND CONVERT(date, ISNULL(NgayCheckOut, NgayCheckIn)) = CONVERT(date, GETDATE());", connection))
            {
                connection.Open();
                return Convert.ToDecimal(command.ExecuteScalar());
            }
        }

        private bool UseSampleData
        {
            get
            {
                return string.IsNullOrWhiteSpace(_connectionString)
                    || _connectionString.Contains("TEN_SERVER_CUA_BAN");
            }
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

        private static DateTime? ReadNullableDateTime(IDataRecord reader, string name)
        {
            var value = reader[name];
            return value == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(value);
        }

        private static List<Room> GetSampleRooms()
        {
            return new List<Room>
            {
                new Room { Id = 1, RoomNumber = "P101", RoomType = "Phong Don Standard", StatusValue = 0, Status = "Trong", PricePerNight = 300000 },
                new Room { Id = 2, RoomNumber = "P102", RoomType = "Phong Don Standard", StatusValue = 1, Status = "Dang thue", PricePerNight = 300000 },
                new Room { Id = 3, RoomNumber = "P201", RoomType = "Phong Doi Standard", StatusValue = 0, Status = "Trong", PricePerNight = 500000 },
                new Room { Id = 4, RoomNumber = "P202", RoomType = "Phong Doi Standard", StatusValue = 1, Status = "Dang thue", PricePerNight = 500000 },
                new Room { Id = 5, RoomNumber = "P301", RoomType = "Phong Don VIP", StatusValue = 0, Status = "Trong", PricePerNight = 600000 },
                new Room { Id = 6, RoomNumber = "P401", RoomType = "Phong Doi VIP", StatusValue = 1, Status = "Dang thue", PricePerNight = 1000000 }
            };
        }

        private static List<Booking> GetSampleBookings()
        {
            return new List<Booking>
            {
                new Booking { Id = 1, CustomerName = "Nguyen Anh Tuan", RoomNumber = "P102", CheckInDate = DateTime.Today.AddDays(-1), CheckOutDate = null, Status = "Chua thanh toan" },
                new Booking { Id = 2, CustomerName = "Le Thi Hong", RoomNumber = "P202", CheckInDate = DateTime.Today.AddDays(-2), CheckOutDate = null, Status = "Chua thanh toan" }
            };
        }
    }
}
