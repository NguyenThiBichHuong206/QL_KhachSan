using System;
using System.Linq;
using QLKhachSan.Models;

namespace QLKhachSan.Data
{
    public class DashboardRepository
    {
        // Lấy tổng số phòng trong khách sạn
        public int GetTongSoPhong()
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                return db.Phongs.Count();
            }
        }

        // Lấy số phòng đang trống
        public int GetSoPhongTrong()
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                return db.Phongs.Count(p => p.TrangThai == 0);
            }
        }

        // Lấy số phòng đang thuê
        public int GetSoPhongDangThue()
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                return db.Phongs.Count(p => p.TrangThai == 1);
            }
        }

        // Lấy tổng số khách hàng
        public int GetTongSoKhachHang()
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                return db.KhachHangs.Count();
            }
        }

        // Lấy tổng số dịch vụ đang hoạt động (TrangThai == true)
        public int GetTongSoDichVu()
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                return db.DichVus.Count(dv => dv.TrangThai == true);
            }
        }

        // Lấy tổng số hóa đơn
        public int GetTongSoHoaDon()
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                return db.HoaDons.Count();
            }
        }

        // Lấy tổng doanh thu từ các hóa đơn đã thanh toán
        public decimal GetTongDoanhThu()
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                decimal tongTien = db.HoaDons
                                    .Where(hd => hd.TrangThaiThanhToan == 1)
                                    .Select(hd => hd.TongTien ?? 0)
                                    .DefaultIfEmpty(0)
                                    .Sum();

                return tongTien;
            }
        }
    }
}