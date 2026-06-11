using System;
using System.Collections.Generic;
using QLKhachSan.Models;
using System.Data.Entity;
using System.Linq;

namespace QLKhachSan.Data
{
    public class ServiceRepository
    {
        // 1. Lấy toàn bộ dịch vụ để quản lý (chỉ trả về dịch vụ đang bán: TrangThai == true)
        public List<DichVu> GetAllServices()
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                return db.DichVus
                         .Include("HangHoa")
                         .Where(dv => dv.TrangThai == true)
                         .OrderBy(dv => dv.TenDV)
                         .ToList();
            }
        }

        // 2. Lấy danh sách hàng hóa để mapping với dịch vụ
        public List<HangHoa> GetAllHangHoa()
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                return db.HangHoas
                         .OrderBy(hh => hh.TenHang)
                         .ToList();
            }
        }

        // 3. Thêm mới dịch vụ
        public bool AddService(DichVu dv)
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                if (string.IsNullOrWhiteSpace(dv.TenDV))
                    throw new Exception("Tên dịch vụ không được để trống.");

                if (dv.DonGia < 0)
                    throw new Exception("Đơn giá không được âm.");

                // CHỈ coi trùng tên khi dịch vụ đang bán (TrangThai == true)
                bool daTonTai = db.DichVus.Any(x => x.TenDV == dv.TenDV && x.TrangThai == true);

                if (daTonTai)
                    throw new Exception("Tên dịch vụ này đã tồn tại.");

                // MaHang = 0 là dòng giả trên giao diện: Không dùng kho
                if (dv.MaHang == 0)
                    dv.MaHang = null;

                if (dv.MaHang == null)
                {
                    dv.SoLuongTieuHao = 1;
                }
                else
                {
                    if (dv.SoLuongTieuHao <= 0)
                        throw new Exception("Số lượng tiêu hao phải lớn hơn 0.");
                }

                // Dịch vụ mới mặc định là đang bán
                dv.TrangThai = true;

                db.DichVus.Add(dv);
                return db.SaveChanges() > 0;
            }
        }

        // 4. Sửa thông tin dịch vụ và mapping hàng hóa
        public bool UpdateService(DichVu dv)
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                var editDV = db.DichVus.FirstOrDefault(x => x.MaDV == dv.MaDV);

                if (editDV == null)
                    return false;

                if (string.IsNullOrWhiteSpace(dv.TenDV))
                    throw new Exception("Tên dịch vụ không được để trống.");

                if (dv.DonGia < 0)
                    throw new Exception("Đơn giá không được âm.");

                // CHỈ coi trùng tên khi dịch vụ đang bán (TrangThai == true)
                bool trungTen = db.DichVus.Any(x => x.MaDV != dv.MaDV &&
                                                    x.TenDV == dv.TenDV &&
                                                    x.TrangThai == true);

                if (trungTen)
                    throw new Exception("Tên dịch vụ này đã tồn tại.");

                editDV.TenDV = dv.TenDV;
                editDV.DonGia = dv.DonGia;

                // MaHang = 0 là dòng giả trên giao diện: Không dùng kho
                if (dv.MaHang == 0)
                    editDV.MaHang = null;
                else
                    editDV.MaHang = dv.MaHang;

                if (editDV.MaHang == null)
                {
                    editDV.SoLuongTieuHao = 1;
                }
                else
                {
                    if (dv.SoLuongTieuHao <= 0)
                        throw new Exception("Số lượng tiêu hao phải lớn hơn 0.");

                    editDV.SoLuongTieuHao = dv.SoLuongTieuHao;
                }

                return db.SaveChanges() > 0;
            }
        }

        // 5. Ngừng bán dịch vụ (legacy method still usable)
        public bool StopService(int maDV)
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                var dichVu = db.DichVus.FirstOrDefault(x => x.MaDV == maDV);

                if (dichVu == null)
                    throw new Exception("Không tìm thấy dịch vụ.");

                dichVu.TrangThai = false;

                return db.SaveChanges() > 0;
            }
        }

        // 6. Bán lại dịch vụ (legacy)
        public bool RestoreService(int maDV)
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                var dichVu = db.DichVus.FirstOrDefault(x => x.MaDV == maDV);

                if (dichVu == null)
                    throw new Exception("Không tìm thấy dịch vụ.");

                dichVu.TrangThai = true;

                return db.SaveChanges() > 0;
            }
        }

        // 7. Xóa dịch vụ: chuyển sang xóa mềm (set TrangThai = false)
        public bool DeleteService(int maDV)
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                var dv = db.DichVus.FirstOrDefault(x => x.MaDV == maDV);

                if (dv == null)
                    return false;

                dv.TrangThai = false;
                return db.SaveChanges() > 0;
            }
        }

        // 8. Tìm dịch vụ đã bị ngừng bán theo tên (dùng để tái kích hoạt)
        public DichVu GetDeletedServiceByName(string tenDV)
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                return db.DichVus
                         .FirstOrDefault(x => x.TenDV == tenDV && x.TrangThai == false);
            }
        }

        // 9. Khôi phục dịch vụ và reset tồn kho của hàng hóa liên kết về 0 (nếu có)
        public bool RestoreAndResetInventory(DichVu dv)
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                if (dv == null)
                    throw new Exception("Dữ liệu dịch vụ không hợp lệ.");

                var existing = db.DichVus.FirstOrDefault(x => x.MaDV == dv.MaDV);

                if (existing == null)
                    throw new Exception("Không tìm thấy dịch vụ cần khôi phục.");

                // Cập nhật các trường theo yêu cầu
                existing.DonGia = dv.DonGia;

                if (dv.MaHang == 0)
                    existing.MaHang = null;
                else
                    existing.MaHang = dv.MaHang;

                if (existing.MaHang == null)
                {
                    existing.SoLuongTieuHao = 1;
                }
                else
                {
                    if (dv.SoLuongTieuHao <= 0)
                        throw new Exception("Số lượng tiêu hao phải lớn hơn 0.");

                    existing.SoLuongTieuHao = dv.SoLuongTieuHao;
                }

                existing.TrangThai = true;

                // Nếu có hàng hóa liên kết, reset tồn kho về 0 (Yêu cầu đặc biệt)
                if (existing.MaHang != null)
                {
                    var hang = db.HangHoas.FirstOrDefault(h => h.MaHang == existing.MaHang);
                    if (hang != null)
                    {
                        hang.SoLuongTon = 0;
                    }
                }

                return db.SaveChanges() > 0;
            }
        }
    }
}