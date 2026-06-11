using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using QLKhachSan.Models;

namespace QLKhachSan.Data
{
    public class BookingRepository
    {
        // Lấy danh sách khách hàng
        public List<KhachHang> GetAllCustomers()
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                return db.KhachHangs
                         .OrderBy(kh => kh.HoTen)
                         .ToList();
            }
        }

        // Lấy danh sách phòng đang trống
        public List<Phong> GetEmptyRooms()
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                return db.Phongs
                         .Include(p => p.LoaiPhong)
                         .Where(p => p.TrangThai == 0)
                         .OrderBy(p => p.TenPhong)
                         .ToList();
            }
        }

        // Lấy danh sách hóa đơn chưa thanh toán / đang ở
        public List<HoaDon> GetActiveInvoices()
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                return db.HoaDons
                         .Include(hd => hd.KhachHang)
                         .Include(hd => hd.Phong)
                         .Include(hd => hd.NhanVien)
                         .Where(hd => hd.TrangThaiThanhToan == 0)
                         .OrderBy(hd => hd.NgayCheckIn)
                         .ToList();
            }
        }

        // Kiểm tra mã nhân viên có tồn tại không
        // Nếu mã nhân viên truyền vào không tồn tại thì lấy nhân viên đầu tiên trong bảng NhanVien
        private int GetMaNhanVienHopLe(QuanLyKhachSanEntities db, int maNhanVien)
        {
            bool tonTaiNhanVien = db.NhanViens.Any(nv => nv.MaNV == maNhanVien);

            if (tonTaiNhanVien)
                return maNhanVien;

            NhanVien nhanVienDauTien = db.NhanViens
                                         .OrderBy(nv => nv.MaNV)
                                         .FirstOrDefault();

            if (nhanVienDauTien == null)
                throw new Exception("Chưa có nhân viên trong bảng NhanVien. Không thể tạo hóa đơn.");

            return nhanVienDauTien.MaNV;
        }

        // Tạo hóa đơn nhận phòng
        public void CreateBooking(int maKhachHang, int maPhong, int maNhanVien, DateTime ngayNhan)
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                try
                {
                    // Kiểm tra khách hàng
                    KhachHang khachHang = db.KhachHangs
                                            .FirstOrDefault(kh => kh.MaKH == maKhachHang);

                    if (khachHang == null)
                        throw new Exception("Không tìm thấy khách hàng.");

                    // Kiểm tra phòng
                    Phong phong = db.Phongs
                                    .Include(p => p.LoaiPhong)
                                    .FirstOrDefault(p => p.MaPhong == maPhong);

                    if (phong == null)
                        throw new Exception("Không tìm thấy phòng.");

                    if (phong.TrangThai == 1)
                        throw new Exception("Phòng này đang thuê, không thể nhận phòng.");

                    // Kiểm tra mã nhân viên hợp lệ
                    int maNhanVienHopLe = GetMaNhanVienHopLe(db, maNhanVien);

                    decimal tienPhong = 0;

                    if (phong.LoaiPhong != null)
                        tienPhong = phong.LoaiPhong.DonGia;

                    HoaDon hoaDon = new HoaDon
                    {
                        MaKH = maKhachHang,
                        MaPhong = maPhong,
                        MaNV = maNhanVienHopLe,
                        NgayCheckIn = ngayNhan,
                        NgayCheckOut = null,
                        TienPhong = tienPhong,
                        TienDichVu = 0,
                        TongTien = tienPhong,
                        TrangThaiThanhToan = 0
                    };

                    /*
                        Lưu ý:
                        Không cập nhật phong.TrangThai = 1 ở đây.

                        Lý do:
                        Database đã có trigger kiểm tra/cập nhật trạng thái phòng.
                        Nếu EF đổi phòng sang Đang thuê trước khi lưu hóa đơn,
                        trigger sẽ hiểu phòng đang bận và chặn thao tác nhận phòng.
                    */

                    db.HoaDons.Add(hoaDon);
                    db.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    string loiChiTiet = LayNoiDungLoi(ex);
                    throw new Exception("Lỗi cập nhật database khi nhận phòng: " + loiChiTiet);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        // Lấy lỗi thật bên trong EF để dễ biết lỗi SQL Server
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