using System;
using System.Collections.Generic;
using System.Linq;
using QLKhachSan.Models;

namespace QLKhachSan.Data
{
    public class CustomerRepository
    {
        // 1. Lấy danh sách Khách Hàng (chỉ những khách đang hoạt động)
        public List<KhachHang> GetAllCustomers()
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                return db.KhachHangs
                         .Where(kh => kh.TrangThai == true)
                         .ToList();
            }
        }

        // 1b. Tìm kiếm khách hàng (chỉ khách đang hoạt động)
        public List<KhachHang> SearchCustomer(string keyword)
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return GetAllCustomers();
                }

                keyword = keyword.Trim();
                return db.KhachHangs
                         .Where(kh => kh.TrangThai == true &&
                                      (kh.HoTen.Contains(keyword) ||
                                       kh.SDT.Contains(keyword) ||
                                       kh.CCCD.Contains(keyword)))
                         .ToList();
            }
        }

        // 2. Thêm mới (đảm bảo TrangThai = true)
        public bool AddCustomer(KhachHang kh)
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                kh.TrangThai = true;
                db.KhachHangs.Add(kh);
                return db.SaveChanges() > 0;
            }
        }

        // 3. Sửa thông tin (đảm bảo TrangThai = true)
        public bool UpdateCustomer(KhachHang kh)
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                var editKH = db.KhachHangs.FirstOrDefault(x => x.MaKH == kh.MaKH);
                if (editKH != null)
                {
                    editKH.HoTen = kh.HoTen;
                    editKH.SDT = kh.SDT;
                    editKH.CCCD = kh.CCCD;
                    editKH.TrangThai = true;
                    return db.SaveChanges() > 0;
                }
                return false;
            }
        }

        // 4. Xóa mềm: set TrangThai = false
        public bool DeleteCustomer(int maKH)
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                var kh = db.KhachHangs.FirstOrDefault(x => x.MaKH == maKH);
                if (kh != null)
                {
                    kh.TrangThai = false;
                    return db.SaveChanges() > 0;
                }
                return false;
            }
        }

        // 5. Tìm khách đã bị ẩn theo CCCD
        public KhachHang GetDeletedCustomerByCCCD(string cccd)
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                if (string.IsNullOrWhiteSpace(cccd)) return null;
                return db.KhachHangs.FirstOrDefault(x => x.CCCD == cccd && x.TrangThai == false);
            }
        }

        // 6. Khôi phục khách hàng đã ẩn: cập nhật thông tin và set TrangThai = true
        public bool RestoreCustomer(KhachHang khMoi)
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                if (khMoi == null || string.IsNullOrWhiteSpace(khMoi.CCCD)) return false;

                var exist = db.KhachHangs.FirstOrDefault(x => x.CCCD == khMoi.CCCD && x.TrangThai == false);
                if (exist != null)
                {
                    exist.HoTen = khMoi.HoTen;
                    exist.SDT = khMoi.SDT;
                    exist.TrangThai = true;
                    return db.SaveChanges() > 0;
                }
                return false;
            }
        }
    }
}