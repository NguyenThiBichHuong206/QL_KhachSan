using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using QLKhachsan.Models;

namespace QLKhachsan.Data
{
    public class AuthRepository
    {
        private static readonly List<UserAccount> MemoryUsers = new List<UserAccount>();
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

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                message = "Mat khau can toi thieu 6 ky tu.";
                return false;
            }

            CreatePasswordHash(password, out var hash, out var salt);

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
                    Username = username,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    CreatedAt = DateTime.Now
                });

                message = "Dang ky thanh cong. Ban co the dang nhap.";
                return true;
            }

            EnsureUserTable();

            using (var connection = new SqlConnection(_connectionString))
            using (var existsCommand = new SqlCommand("SELECT COUNT(1) FROM dbo.NguoiDung WHERE TenDangNhap = @Username;", connection))
            using (var insertCommand = new SqlCommand(@"
INSERT INTO dbo.NguoiDung (TenDangNhap, MatKhauHash, MatKhauSalt, NgayTao)
VALUES (@Username, @PasswordHash, @PasswordSalt, @CreatedAt);", connection))
            {
                existsCommand.Parameters.AddWithValue("@Username", username);
                insertCommand.Parameters.AddWithValue("@Username", username);
                insertCommand.Parameters.AddWithValue("@PasswordHash", hash);
                insertCommand.Parameters.AddWithValue("@PasswordSalt", salt);
                insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

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
                var user = MemoryUsers.FirstOrDefault(item => string.Equals(item.Username, username, StringComparison.OrdinalIgnoreCase));
                if (user == null || !VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
                {
                    message = "Ten dang nhap hoac mat khau khong dung.";
                    return false;
                }

                account = user;
                message = "Dang nhap thanh cong.";
                return true;
            }

            EnsureUserTable();

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
SELECT TOP 1 MaNguoiDung, TenDangNhap, MatKhauHash, MatKhauSalt, NgayTao
FROM dbo.NguoiDung
WHERE TenDangNhap = @Username;", connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        message = "Ten dang nhap hoac mat khau khong dung.";
                        return false;
                    }

                    var user = new UserAccount
                    {
                        Id = Convert.ToInt32(reader["MaNguoiDung"]),
                        Username = Convert.ToString(reader["TenDangNhap"]),
                        PasswordHash = (byte[])reader["MatKhauHash"],
                        PasswordSalt = (byte[])reader["MatKhauSalt"],
                        CreatedAt = Convert.ToDateTime(reader["NgayTao"])
                    };

                    if (!VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
                    {
                        message = "Ten dang nhap hoac mat khau khong dung.";
                        return false;
                    }

                    account = user;
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

        private void EnsureUserTable()
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
IF OBJECT_ID('dbo.NguoiDung', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.NguoiDung
    (
        MaNguoiDung INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenDangNhap NVARCHAR(100) NOT NULL UNIQUE,
        MatKhauHash VARBINARY(64) NOT NULL,
        MatKhauSalt VARBINARY(32) NOT NULL,
        NgayTao DATETIME NOT NULL
    );
END;", connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static string NormalizeUsername(string username)
        {
            return username == null ? string.Empty : username.Trim();
        }

        private static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            salt = new byte[32];

            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(salt);
            }

            hash = ComputeHash(password, salt);
        }

        private static bool VerifyPassword(string password, byte[] expectedHash, byte[] salt)
        {
            var hash = ComputeHash(password, salt);

            if (hash.Length != expectedHash.Length)
            {
                return false;
            }

            var differentBits = 0;
            for (var index = 0; index < hash.Length; index++)
            {
                differentBits |= hash[index] ^ expectedHash[index];
            }

            return differentBits == 0;
        }

        private static byte[] ComputeHash(string password, byte[] salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                var input = new byte[salt.Length + passwordBytes.Length];
                Buffer.BlockCopy(salt, 0, input, 0, salt.Length);
                Buffer.BlockCopy(passwordBytes, 0, input, salt.Length, passwordBytes.Length);
                return sha256.ComputeHash(input);
            }
        }
    }
}
