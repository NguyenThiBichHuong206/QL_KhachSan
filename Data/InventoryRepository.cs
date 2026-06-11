using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using QLKhachSan.Models;

namespace QLKhachSan.Data
{
    public class InventoryRepository
    {
        // =========================
        // NHÀ CUNG CẤP
        // =========================

        // Lấy danh sách nhà cung cấp
        public List<NhaCungCap> GetAllSuppliers()
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                return db.NhaCungCaps
                         .OrderBy(ncc => ncc.TenNCC)
                         .ToList();
            }
        }

        // Thêm nhà cung cấp mới
        public void AddSupplier(NhaCungCap nhaCungCap)
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                try
                {
                    if (nhaCungCap == null)
                        throw new Exception("Dữ liệu nhà cung cấp không hợp lệ.");

                    if (string.IsNullOrWhiteSpace(nhaCungCap.TenNCC))
                        throw new Exception("Tên nhà cung cấp không được để trống.");

                    bool daTonTai = db.NhaCungCaps
                                      .Any(ncc => ncc.TenNCC == nhaCungCap.TenNCC);

                    if (daTonTai)
                        throw new Exception("Tên nhà cung cấp này đã tồn tại.");

                    db.NhaCungCaps.Add(nhaCungCap);
                    db.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    string loiChiTiet = LayNoiDungLoi(ex);
                    throw new Exception("Lỗi cập nhật database khi thêm nhà cung cấp: " + loiChiTiet);
                }
            }
        }

        // Sửa nhà cung cấp
        public void UpdateSupplier(NhaCungCap nhaCungCap)
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                try
                {
                    if (nhaCungCap == null)
                        throw new Exception("Dữ liệu nhà cung cấp không hợp lệ.");

                    if (string.IsNullOrWhiteSpace(nhaCungCap.TenNCC))
                        throw new Exception("Tên nhà cung cấp không được để trống.");

                    NhaCungCap editNCC = db.NhaCungCaps
                                           .FirstOrDefault(ncc => ncc.MaNCC == nhaCungCap.MaNCC);

                    if (editNCC == null)
                        throw new Exception("Không tìm thấy nhà cung cấp cần sửa.");

                    bool trungTen = db.NhaCungCaps
                                      .Any(ncc => ncc.MaNCC != nhaCungCap.MaNCC &&
                                                  ncc.TenNCC == nhaCungCap.TenNCC);

                    if (trungTen)
                        throw new Exception("Tên nhà cung cấp này đã tồn tại.");

                    editNCC.TenNCC = nhaCungCap.TenNCC;
                    editNCC.DiaChi = nhaCungCap.DiaChi;
                    editNCC.SDT = nhaCungCap.SDT;

                    db.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    string loiChiTiet = LayNoiDungLoi(ex);
                    throw new Exception("Lỗi cập nhật database khi sửa nhà cung cấp: " + loiChiTiet);
                }
            }
        }

        // Xóa nhà cung cấp
        // Chỉ cho xóa nếu nhà cung cấp chưa có phiếu nhập
        public void DeleteSupplier(int maNCC)
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                try
                {
                    bool daCoPhieuNhap = db.PhieuNhapKhoes.Any(pn => pn.MaNCC == maNCC);

                    if (daCoPhieuNhap)
                        throw new Exception("Nhà cung cấp đã có phiếu nhập, không thể xóa. Chỉ nên sửa thông tin.");

                    NhaCungCap nhaCungCap = db.NhaCungCaps
                                               .FirstOrDefault(ncc => ncc.MaNCC == maNCC);

                    if (nhaCungCap == null)
                        throw new Exception("Không tìm thấy nhà cung cấp cần xóa.");

                    db.NhaCungCaps.Remove(nhaCungCap);
                    db.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    string loiChiTiet = LayNoiDungLoi(ex);
                    throw new Exception("Lỗi cập nhật database khi xóa nhà cung cấp: " + loiChiTiet);
                }
            }
        }

        // =========================
        // HÀNG HÓA
        // =========================

        // Lấy danh sách hàng hóa
        public List<HangHoa> GetAllGoods()
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                return db.HangHoas
                         .OrderBy(hh => hh.TenHang)
                         .ToList();
            }
        }

        // Thêm hàng hóa mới
        // Tồn ban đầu luôn là 0, tồn tăng qua phiếu nhập
        public void AddGoods(HangHoa hangHoa)
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                try
                {
                    if (hangHoa == null)
                        throw new Exception("Dữ liệu hàng hóa không hợp lệ.");

                    if (string.IsNullOrWhiteSpace(hangHoa.TenHang))
                        throw new Exception("Tên hàng hóa không được để trống.");

                    if (string.IsNullOrWhiteSpace(hangHoa.DonViTinh))
                        throw new Exception("Đơn vị tính không được để trống.");

                    if (hangHoa.GiaNhap < 0)
                        throw new Exception("Giá nhập không được âm.");

                    bool daTonTai = db.HangHoas
                                      .Any(hh => hh.TenHang == hangHoa.TenHang);

                    if (daTonTai)
                        throw new Exception("Hàng hóa này đã tồn tại.");

                    hangHoa.SoLuongTon = 0;

                    db.HangHoas.Add(hangHoa);
                    db.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    string loiChiTiet = LayNoiDungLoi(ex);
                    throw new Exception("Lỗi cập nhật database khi thêm hàng hóa: " + loiChiTiet);
                }
            }
        }

        // Sửa hàng hóa
        // Không sửa SoLuongTon ở đây, tồn kho chỉ thay đổi qua nhập kho hoặc dùng dịch vụ
        public void UpdateGoods(HangHoa hangHoa)
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                try
                {
                    if (hangHoa == null)
                        throw new Exception("Dữ liệu hàng hóa không hợp lệ.");

                    if (string.IsNullOrWhiteSpace(hangHoa.TenHang))
                        throw new Exception("Tên hàng hóa không được để trống.");

                    if (string.IsNullOrWhiteSpace(hangHoa.DonViTinh))
                        throw new Exception("Đơn vị tính không được để trống.");

                    if (hangHoa.GiaNhap < 0)
                        throw new Exception("Giá nhập không được âm.");

                    HangHoa editHangHoa = db.HangHoas
                                            .FirstOrDefault(hh => hh.MaHang == hangHoa.MaHang);

                    if (editHangHoa == null)
                        throw new Exception("Không tìm thấy hàng hóa cần sửa.");

                    bool trungTen = db.HangHoas
                                      .Any(hh => hh.MaHang != hangHoa.MaHang &&
                                                 hh.TenHang == hangHoa.TenHang);

                    if (trungTen)
                        throw new Exception("Tên hàng hóa này đã tồn tại.");

                    editHangHoa.TenHang = hangHoa.TenHang;
                    editHangHoa.DonViTinh = hangHoa.DonViTinh;
                    editHangHoa.GiaNhap = hangHoa.GiaNhap;

                    db.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    string loiChiTiet = LayNoiDungLoi(ex);
                    throw new Exception("Lỗi cập nhật database khi sửa hàng hóa: " + loiChiTiet);
                }
            }
        }

        // Xóa hàng hóa
        // Chỉ cho xóa nếu hàng hóa chưa từng nhập kho và chưa mapping với dịch vụ
        public void DeleteGoods(int maHang)
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                try
                {
                    bool daCoChiTietNhap = db.ChiTietNhapKhoes.Any(ct => ct.MaHang == maHang);

                    if (daCoChiTietNhap)
                        throw new Exception("Hàng hóa đã có lịch sử nhập kho, không thể xóa. Chỉ nên sửa thông tin.");

                    bool daMappingDichVu = db.DichVus.Any(dv => dv.MaHang == maHang);

                    if (daMappingDichVu)
                        throw new Exception("Hàng hóa đang được liên kết với dịch vụ, không thể xóa. Hãy bỏ mapping trong màn Dịch vụ trước.");

                    HangHoa hangHoa = db.HangHoas.FirstOrDefault(hh => hh.MaHang == maHang);

                    if (hangHoa == null)
                        throw new Exception("Không tìm thấy hàng hóa cần xóa.");

                    db.HangHoas.Remove(hangHoa);
                    db.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    string loiChiTiet = LayNoiDungLoi(ex);
                    throw new Exception("Lỗi cập nhật database khi xóa hàng hóa: " + loiChiTiet);
                }
            }
        }

        // =========================
        // PHIẾU NHẬP KHO
        // =========================

        // Lấy danh sách phiếu nhập đã lưu
        public List<PhieuNhapKho> GetAllReceipts()
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                return db.PhieuNhapKhoes
                         .Include(pn => pn.NhaCungCap)
                         .Include(pn => pn.NhanVien)
                         .OrderByDescending(pn => pn.MaPhieuNhap)
                         .ToList();
            }
        }

        // Tìm phiếu nhập theo khoảng ngày
        public List<PhieuNhapKho> SearchReceiptsByDate(DateTime tuNgay, DateTime denNgay)
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                return db.PhieuNhapKhoes
                         .Include(pn => pn.NhaCungCap)
                         .Include(pn => pn.NhanVien)
                         .Where(pn => pn.NgayNhap != null &&
                                      pn.NgayNhap.Value >= tuNgay &&
                                      pn.NgayNhap.Value <= denNgay)
                         .OrderByDescending(pn => pn.MaPhieuNhap)
                         .ToList();
            }
        }

        // Tìm kiếm phiếu nhập theo ngày, tên nhà cung cấp và mã phiếu nhập
        public List<PhieuNhapKho> SearchReceipts(DateTime? tuNgay, DateTime? denNgay, string tenNCC, int? maPhieuNhap)
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                var query = db.PhieuNhapKhoes
                              .Include(pn => pn.NhaCungCap)
                              .Include(pn => pn.NhanVien)
                              .AsQueryable();

                if (maPhieuNhap != null)
                {
                    query = query.Where(pn => pn.MaPhieuNhap == maPhieuNhap.Value);
                }

                if (tuNgay != null)
                {
                    DateTime ngayBatDau = tuNgay.Value.Date;

                    query = query.Where(pn => pn.NgayNhap != null &&
                                              pn.NgayNhap.Value >= ngayBatDau);
                }

                if (denNgay != null)
                {
                    DateTime ngayKetThuc = denNgay.Value.Date.AddDays(1).AddSeconds(-1);

                    query = query.Where(pn => pn.NgayNhap != null &&
                                              pn.NgayNhap.Value <= ngayKetThuc);
                }

                if (!string.IsNullOrWhiteSpace(tenNCC))
                {
                    string keyword = tenNCC.Trim().ToLower();

                    query = query.Where(pn => pn.NhaCungCap.TenNCC.ToLower().Contains(keyword));
                }

                return query.OrderByDescending(pn => pn.MaPhieuNhap)
                            .ToList();
            }
        }

        // Lấy chi tiết phiếu nhập theo mã phiếu
        public List<ChiTietNhapKho> GetReceiptDetails(int maPhieuNhap)
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                return db.ChiTietNhapKhoes
                         .Include(ct => ct.HangHoa)
                         .Where(ct => ct.MaPhieuNhap == maPhieuNhap)
                         .OrderBy(ct => ct.MaHang)
                         .ToList();
            }
        }

        // Kiểm tra nhân viên có tồn tại không
        // Nếu mã nhân viên truyền vào không tồn tại thì lấy nhân viên đầu tiên
        private int GetMaNhanVienHopLe(QuanLyKhachSanEntities db, int maNhanVien)
        {
            bool tonTaiNhanVien = db.NhanViens.Any(nv => nv.MaNV == maNhanVien);

            if (tonTaiNhanVien)
                return maNhanVien;

            NhanVien nhanVienDauTien = db.NhanViens
                                         .OrderBy(nv => nv.MaNV)
                                         .FirstOrDefault();

            if (nhanVienDauTien == null)
                throw new Exception("Chưa có nhân viên trong bảng NhanVien. Không thể lập phiếu nhập.");

            return nhanVienDauTien.MaNV;
        }

        // Lưu phiếu nhập kho
        public void SaveReceipt(int maNCC, int maNV, DateTime ngayNhap, List<ChiTietNhapKho> danhSachChiTiet)
        {
            using (QuanLyKhachSanEntities db = new QuanLyKhachSanEntities())
            {
                using (DbContextTransaction transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (danhSachChiTiet == null || danhSachChiTiet.Count == 0)
                            throw new Exception("Phiếu nhập chưa có chi tiết hàng hóa.");

                        NhaCungCap nhaCungCap = db.NhaCungCaps
                                                   .FirstOrDefault(ncc => ncc.MaNCC == maNCC);

                        if (nhaCungCap == null)
                            throw new Exception("Không tìm thấy nhà cung cấp.");

                        int maNhanVienHopLe = GetMaNhanVienHopLe(db, maNV);

                        PhieuNhapKho phieuNhap = new PhieuNhapKho
                        {
                            NgayNhap = ngayNhap,
                            MaNCC = maNCC,
                            MaNV = maNhanVienHopLe,
                            TongTien = 0
                        };

                        db.PhieuNhapKhoes.Add(phieuNhap);
                        db.SaveChanges();

                        foreach (ChiTietNhapKho item in danhSachChiTiet)
                        {
                            if (item.SoLuong <= 0)
                                throw new Exception("Số lượng nhập phải lớn hơn 0.");

                            if (item.DonGia <= 0)
                                throw new Exception("Đơn giá nhập phải lớn hơn 0.");

                            HangHoa hangHoa = db.HangHoas
                                                .FirstOrDefault(hh => hh.MaHang == item.MaHang);

                            if (hangHoa == null)
                                throw new Exception("Không tìm thấy hàng hóa có mã: " + item.MaHang);

                            ChiTietNhapKho chiTiet = new ChiTietNhapKho
                            {
                                MaPhieuNhap = phieuNhap.MaPhieuNhap,
                                MaHang = item.MaHang,
                                SoLuong = item.SoLuong,
                                DonGia = item.DonGia
                            };

                            db.ChiTietNhapKhoes.Add(chiTiet);

                            // Nhập kho thì tồn kho phải tăng.
                            // Nếu SoLuongTon đang NULL thì xem như 0.
                            hangHoa.SoLuongTon = (hangHoa.SoLuongTon ?? 0) + item.SoLuong;

                            // Cập nhật giá nhập gần nhất.
                            hangHoa.GiaNhap = item.DonGia;
                        }

                        db.SaveChanges();

                        transaction.Commit();
                    }
                    catch (DbUpdateException ex)
                    {
                        transaction.Rollback();

                        string loiChiTiet = LayNoiDungLoi(ex);
                        throw new Exception("Lỗi cập nhật database khi lưu phiếu nhập: " + loiChiTiet);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception(ex.Message);
                    }
                }
            }
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