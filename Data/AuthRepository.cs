using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using QLKhachsan.Models;

namespace QLKhachsan.Data
{
    public class AuthRepository
    {
        private static readonly List<UserAccount> MemoryUsers = new List<UserAccount>
        {
            new UserAccount { Id = 1, FullName = "Nguyen Van Admin", Username = "admin", Password = "123", RoleValue = 1, CreatedAt = DateTime.Now },
            new UserAccount { Id = 2, FullName = "Le Thi Le Tan", Username = "reception01", Password = "123", RoleValue = 0, CreatedAt = DateTime.Now }
        };

        private readonly string _connectionString;

        public AuthRepository()
        {
            var setting = ConfigurationManager.ConnectionStrings["HotelDb"];
            _connectionString = setting == null ? string.Empty : setting.ConnectionString;
        }

        public bool Register(string username, string password, out string message)
        {
            username = NormalizeUsername(username);

            if (string.IsNullOrWhiteSpace(username))
            {
                message = "Vui long nhap ten dang nhap.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                message = "Vui long nhap mat khau.";
                return false;
            }

            if (UseMemoryStore)
            {
                if (MemoryUsers.Any(user => string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase)))
                {
                    message = "Ten dang nhap da ton tai.";
                    return false;
                }

                MemoryUsers.Add(new UserAccount
                {
                    Id = MemoryUsers.Count + 1,
                    FullName = username,
                    Username = username,
                    Password = password,
                    RoleValue = 0,
                    CreatedAt = DateTime.Now
                });

                message = "Dang ky thanh cong. Ban co the dang nhap.";
                return true;
            }

            using (var connection = new SqlConnection(_connectionString))
            using (var existsCommand = new SqlCommand("SELECT COUNT(1) FROM dbo.NhanVien WHERE TaiKhoan = @Username;", connection))
            using (var insertCommand = new SqlCommand(@"
INSERT INTO dbo.NhanVien (HoTen, TaiKhoan, MatKhau, VaiTro)
VALUES (@FullName, @Username, @Password, @Role);", connection))
            {
                existsCommand.Parameters.AddWithValue("@Username", username);
                insertCommand.Parameters.AddWithValue("@FullName", username);
                insertCommand.Parameters.AddWithValue("@Username", username);
                insertCommand.Parameters.AddWithValue("@Password", password);
                insertCommand.Parameters.AddWithValue("@Role", 0);

                connection.Open();

                var exists = Convert.ToInt32(existsCommand.ExecuteScalar()) > 0;
                if (exists)
                {
                    message = "Ten dang nhap da ton tai.";
                    return false;
                }

                insertCommand.ExecuteNonQuery();
                message = "Dang ky thanh cong. Ban co the dang nhap.";
                return true;
            }
        }

        public bool Login(string username, string password, out UserAccount account, out string message)
        {
            account = null;
            username = NormalizeUsername(username);

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                message = "Vui long nhap ten dang nhap va mat khau.";
                return false;
            }

            if (UseMemoryStore)
            {
                account = MemoryUsers.FirstOrDefault(user =>
                    string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase)
                    && user.Password == password);

                if (account == null)
                {
                    message = "Ten dang nhap hoac mat khau khong dung.";
                    return false;
                }

                message = "Dang nhap thanh cong.";
                return true;
            }

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
SELECT TOP 1 MaNV, HoTen, TaiKhoan, MatKhau, VaiTro
FROM dbo.NhanVien
WHERE TaiKhoan = @Username AND MatKhau = @Password;", connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Password", password);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        message = "Ten dang nhap hoac mat khau khong dung.";
                        return false;
                    }

                    account = new UserAccount
                    {
                        Id = Convert.ToInt32(reader["MaNV"]),
                        FullName = Convert.ToString(reader["HoTen"]),
                        Username = Convert.ToString(reader["TaiKhoan"]),
                        Password = Convert.ToString(reader["MatKhau"]),
                        RoleValue = Convert.ToInt32(reader["VaiTro"]),
                        CreatedAt = DateTime.Now
                    };

                    message = "Dang nhap thanh cong.";
                    return true;
                }
            }
        }

        private bool UseMemoryStore
        {
            get
            {
                return string.IsNullOrWhiteSpace(_connectionString)
                    || _connectionString.Contains("TEN_SERVER_CUA_BAN");
            }
        }

        private static string NormalizeUsername(string username)
        {
            return username == null ? string.Empty : username.Trim();
        }
    }
}
