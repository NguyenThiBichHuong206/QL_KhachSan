using System;

namespace QLKhachsan.Models
{
    public class UserAccount
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public int RoleValue { get; set; }
        public DateTime CreatedAt { get; set; }

        public string RoleName
        {
            get { return RoleValue == 1 ? "Admin" : "Le tan"; }
        }
    }
}
