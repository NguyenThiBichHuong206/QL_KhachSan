using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using QLKhachSan.Models;

namespace QLKhachSan.Data
{
    public class CheckoutRepository
    {
        // Lấy danh sách phòng đang thuê
        // Quy ước: hóa đơn chưa thanh toán = khách còn đang thuê phòng
        public List<HoaDon> GetDanhSachPhongDangThue()
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                return db.HoaDons
                         .Include(hd => hd.KhachHang)
                         .Include(hd => hd.Phong)
                         .Include(hd => hd.Phong.LoaiPhong)
                         .Where(hd => hd.TrangThaiThanhToan == 0)
                         .OrderBy(hd => hd.MaPhong)
                         .ToList();
            }
        }
        // HIỆN  danh sách hóa đơn đã thanh toán
        public List<HoaDon> GetDanhSachHoaDon()
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                return db.HoaDons
                         .Include(hd => hd.KhachHang)
                         .Include(hd => hd.Phong)
                         .Include(hd => hd.Phong.LoaiPhong)
                         .Where(hd => hd.TrangThaiThanhToan == 1)
                         .OrderBy(hd => hd.MaHD)
                         .ToList();
            }
        }

        // Lấy danh sách dịch vụ
        // CHỈ LẤY dịch vụ đang bán (TrangThai == true) để dùng khi lập hóa đơn mới
        public List<DichVu> GetDanhSachDichVu()
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                return db.DichVus
                         .Include(dv => dv.HangHoa)
                         .Where(dv => dv.TrangThai == true)
                         .OrderBy(dv => dv.TenDV)
                         .ToList();
            }
        }

        // Lấy chi tiết dịch vụ theo hóa đơn
        public List<ChiTietHD> GetChiTietDichVu(int maHD)
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                return db.ChiTietHDs
                         .Include(ct => ct.DichVu)
                         .Where(ct => ct.MaHD == maHD)
                         .OrderBy(ct => ct.MaCTHD)
                         .ToList();
            }
        }

        // Lấy lại hóa đơn theo mã
        public HoaDon GetHoaDonById(int maHD)
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                return db.HoaDons
                         .Include(hd => hd.KhachHang)
                         .Include(hd => hd.Phong)
                         .Include(hd => hd.Phong.LoaiPhong)
                         .FirstOrDefault(hd => hd.MaHD == maHD);
            }
        }

        // Thêm dịch vụ vào hóa đơn
        public void ThemDichVuVaoHoaDon(int maHD, int maDV, int soLuong)
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                try
                {
                    HoaDon hoaDon = db.HoaDons.FirstOrDefault(hd => hd.MaHD == maHD);

                    if (hoaDon == null)
                        throw new Exception("Không tìm thấy hóa đơn.");

                    if (hoaDon.TrangThaiThanhToan == 1)
                        throw new Exception("Hóa đơn đã thanh toán, không thể thêm dịch vụ.");

                    DichVu dichVu = db.DichVus.FirstOrDefault(dv => dv.MaDV == maDV);

                    if (dichVu == null)
                        throw new Exception("Không tìm thấy dịch vụ.");

                    if (soLuong <= 0)
                        throw new Exception("Số lượng phải lớn hơn 0.");

                    // =========================
                    // CHỈ KIỂM TRA TỒN KHO
                    // KHÔNG TRỪ KHO Ở C#
                    // Trigger SQL sẽ tự trừ kho sau khi SaveChanges()
                    // =========================
                    if (dichVu.MaHang != null)
                    {
                        HangHoa hangHoa = db.HangHoas
                                            .FirstOrDefault(hh => hh.MaHang == dichVu.MaHang.Value);

                        if (hangHoa == null)
                            throw new Exception("Dịch vụ có liên kết hàng hóa nhưng không tìm thấy hàng hóa trong kho.");

                        int soLuongTieuHao = dichVu.SoLuongTieuHao;

                        if (soLuongTieuHao <= 0)
                            soLuongTieuHao = 1;

                        int soLuongCanDung = soLuong * soLuongTieuHao;

                        if (hangHoa.SoLuongTon < soLuongCanDung)
                        {
                            throw new Exception(
                                "Số lượng hàng tồn kho không đủ để cung cấp dịch vụ." +
                                "\nHàng hóa: " + hangHoa.TenHang +
                                "\nTồn hiện tại: " + hangHoa.SoLuongTon +
                                "\nCần dùng: " + soLuongCanDung);
                        }
                    }

                    // =========================
                    // THÊM HOẶC CỘNG DỒN DỊCH VỤ
                    // =========================
                    ChiTietHD chiTiet = db.ChiTietHDs
                                          .FirstOrDefault(ct => ct.MaHD == maHD && ct.MaDV == maDV);

                    if (chiTiet == null)
                    {
                        chiTiet = new ChiTietHD
                        {
                            MaHD = maHD,
                            MaDV = maDV,
                            SoLuong = soLuong,
                            DonGia = dichVu.DonGia
                        };

                        db.ChiTietHDs.Add(chiTiet);
                    }
                    else
                    {
                        chiTiet.SoLuong = (chiTiet.SoLuong ?? 0) + soLuong;
                        chiTiet.DonGia = dichVu.DonGia;
                    }

                    db.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    string loiChiTiet = LayNoiDungLoi(ex);
                    throw new Exception("Lỗi cập nhật database khi thêm dịch vụ: " + loiChiTiet);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        // Xóa dịch vụ khỏi hóa đơn
        public void XoaDichVuKhoiHoaDon(int maCTHD)
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                try
                {
                    ChiTietHD chiTiet = db.ChiTietHDs
                                          .FirstOrDefault(ct => ct.MaCTHD == maCTHD);

                    if (chiTiet == null)
                        throw new Exception("Không tìm thấy chi tiết dịch vụ.");

                    int maHD = chiTiet.MaHD ?? 0;

                    HoaDon hoaDon = db.HoaDons.FirstOrDefault(hd => hd.MaHD == maHD);

                    if (hoaDon == null)
                        throw new Exception("Không tìm thấy hóa đơn.");

                    if (hoaDon.TrangThaiThanhToan == 1)
                        throw new Exception("Hóa đơn đã thanh toán, không thể xóa dịch vụ.");

                    db.ChiTietHDs.Remove(chiTiet);
                    db.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    string loiChiTiet = LayNoiDungLoi(ex);
                    throw new Exception("Lỗi cập nhật database khi xóa dịch vụ: " + loiChiTiet);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        // Thanh toán hóa đơn
        public void ThanhToanHoaDon(int maHD)
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                try
                {
                    HoaDon hoaDon = db.HoaDons.FirstOrDefault(hd => hd.MaHD == maHD);

                    if (hoaDon == null)
                        throw new Exception("Không tìm thấy hóa đơn.");

                    if (hoaDon.TrangThaiThanhToan == 1)
                        throw new Exception("Hóa đơn này đã thanh toán.");

                    db.Database.ExecuteSqlCommand(
                        "EXEC sp_ThanhToan @MaHD",
                        new SqlParameter("@MaHD", maHD)
                    );
                }
                catch (DbUpdateException ex)
                {
                    string loiChiTiet = LayNoiDungLoi(ex);
                    throw new Exception("Lỗi cập nhật database khi thanh toán: " + loiChiTiet);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        // Tính lại tổng tiền hóa đơn
        private void CapNhatTongTienHoaDon(QuanLyKhachSanEntities db, int maHD)
        {
            HoaDon hoaDon = db.HoaDons.FirstOrDefault(hd => hd.MaHD == maHD);

            if (hoaDon == null)
                return;

            decimal tienPhong = hoaDon.TienPhong ?? 0;

            decimal tienDichVu = db.ChiTietHDs
                                   .Where(ct => ct.MaHD == maHD)
                                   .Select(ct => (ct.SoLuong ?? 0) * (ct.DonGia ?? 0))
                                   .DefaultIfEmpty(0)
                                   .Sum();

            hoaDon.TienDichVu = tienDichVu;
            hoaDon.TongTien = tienPhong + tienDichVu;
        }

        // Lấy lỗi thật bên trong EF
        private string LayNoiDungLoi(Exception ex)
        {
            Exception inner = ex;

            while (inner.InnerException != null)
            {
                inner = inner.InnerException;
            }

            return inner.Message;
        }
    }
}