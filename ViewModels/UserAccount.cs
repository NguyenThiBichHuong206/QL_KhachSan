using System;

namespace QLKhachSan.Models
{
    public class UserAccount
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }

        // Numeric role value mapped from DB (e.g. 1 = Admin, 0 = Lễ tân)
        public int RoleValue { get; set; }

        public DateTime CreatedAt { get; set; }

        // Human readable role name
        public string RoleName
        {
            get { return RoleValue == 1 ? "Admin" : "Le tan"; }
        }

        // Optional alias used in some places as "ChucVu"
        public string ChucVu
        {
            get { return RoleName; }
        }
    }
}