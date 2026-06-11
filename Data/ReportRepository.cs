using System;
using System.Collections.Generic;
using System.Linq;
using QLKhachSan.Models;

namespace QLKhachSan.Data
{
    public class ReportRepository
    {
        // Lấy danh sách hóa đơn đã thanh toán theo khoảng thời gian
        public List<v_BaoCaoDoanhThuNangCao> GetDoanhThu(DateTime tuNgay, DateTime denNgay)
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                DateTime ngayBatDau = tuNgay.Date;
                DateTime ngayKetThuc = denNgay.Date;

                return db.v_BaoCaoDoanhThuNangCao
                         .Where(hd => hd.NgayThanhToan >= ngayBatDau &&
                                      hd.NgayThanhToan <= ngayKetThuc)
                         .OrderByDescending(hd => hd.NgayThanhToan)
                         .ThenByDescending(hd => hd.MaHD)
                         .ToList();
            }
        }
    }
}