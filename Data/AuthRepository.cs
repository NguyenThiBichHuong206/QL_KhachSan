using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QLKhachSan.Models;

namespace QLKhachSan.Data
{
    public class AuthRepository
    {
        public bool Register(string username, string password, out string message)
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                // Kiểm tra tài khoản đã tồn tại chưa bằng LINQ
                // Lưu ý: Nếu db.NhanViens báo đỏ, hãy sửa thành db.NhanVien (Bỏ chữ s)
                if (db.NhanViens.Any(nv => nv.TaiKhoan == username))
                {
                    message = "Tên đăng nhập đã tồn tại trong hệ thống.";
                    return false;
                }

                // Thêm mới không cần viết câu lệnh INSERT INTO
                var nhanVienMoi = new NhanVien
                {
                    HoTen = username,
                    TaiKhoan = username,
                    MatKhau = password,
                    VaiTro = 0
                };

                db.NhanViens.Add(nhanVienMoi);
                db.SaveChanges(); // Lưu xuống SQL

                message = "Đăng ký thành công. Bạn có thể đăng nhập ngay.";
                return true;
            }
        }

        public bool Login(string username, string password, out UserAccount account, out string message)
        {
            account = null;
            using (var db = new QuanLyKhachSanEntities())
            {
                // Tìm nhân viên khớp tài khoản & mật khẩu
                var nv = db.NhanViens.FirstOrDefault(x => x.TaiKhoan == username && x.MatKhau == password);

                if (nv != null)
                {
                    // Map dữ liệu sang UserAccount để đưa lên giao diện
                    account = new UserAccount
                    {
                        Id = nv.MaNV,
                        FullName = nv.HoTen,
                        Username = nv.TaiKhoan,
                        Password = nv.MatKhau,
                        RoleValue = nv.VaiTro ?? 0,
                        CreatedAt = DateTime.Now
                    };
                    message = "Đăng nhập thành công!";
                    return true;
                }

                message = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return false;
            }
        }
    }
}
