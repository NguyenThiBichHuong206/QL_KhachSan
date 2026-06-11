using System.Collections.Generic;
using System.Linq;
using QLKhachSan.Models;

namespace QLKhachSan.Data
{
    public class RoomRepository
    {
        // 1. Lấy toàn bộ danh sách phòng (loại bỏ phòng đã xóa mềm: TrangThai == 2)
        public List<Phong> GetAllRooms()
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                return db.Phongs
                         .Where(p => p.TrangThai != 2)
                         .ToList();
            }
        }

        // 2. Lấy danh sách loại phòng để đổ vào ComboBox
        public List<LoaiPhong> GetAllRoomTypes()
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                return db.LoaiPhongs.ToList();
            }
        }

        // 3. Thêm phòng mới
        public void AddRoom(Phong phong)
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                db.Phongs.Add(phong);
                db.SaveChanges();
            }
        }

        // 4. Cập nhật phòng (bao gồm HinhAnh)
        public void UpdateRoom(Phong phong)
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                var existing = db.Phongs.FirstOrDefault(x => x.MaPhong == phong.MaPhong);
                if (existing != null)
                {
                    existing.TenPhong = phong.TenPhong;
                    existing.MaLoai = phong.MaLoai;
                    existing.TrangThai = phong.TrangThai;
                    existing.HinhAnh = phong.HinhAnh;
                    db.SaveChanges();
                }
            }
        }

        // 5. "Xóa mềm" phòng: đặt TrangThai = 2 (ngừng hoạt động / ẩn)
        public void DeleteRoom(int maPhong)
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                Phong p = db.Phongs.FirstOrDefault(x => x.MaPhong == maPhong);

                if (p != null)
                {
                    p.TrangThai = 2;
                    db.SaveChanges();
                }
            }
        }

        // 6. Tìm kiếm phòng theo tên (không trả về phòng đã xóa mềm)
        public List<Phong> SearchRooms(string tuKhoa)
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                if (string.IsNullOrWhiteSpace(tuKhoa))
                {
                    return db.Phongs
                             .Where(p => p.TrangThai != 2)
                             .ToList();
                }

                return db.Phongs
                         .Where(p => p.TrangThai != 2 && p.TenPhong.Contains(tuKhoa))
                         .ToList();
            }
        }

        // 7. Tìm phòng đã xóa mềm theo tên (dùng để phục hồi / tái kích hoạt)
        public Phong GetDeletedRoomByName(string tenPhong)
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                return db.Phongs.FirstOrDefault(p => p.TenPhong == tenPhong && p.TrangThai == 2);
            }
        }

        // 8. Cập nhật trạng thái phòng
        // Quy ước: 0 = Trống, 1 = Đang thuê, 2 = Đã xóa mềm / ngừng hoạt động
        public void UpdateRoomStatus(int maPhong, int trangThai)
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                Phong p = db.Phongs.FirstOrDefault(x => x.MaPhong == maPhong);

                if (p != null)
                {
                    p.TrangThai = trangThai;
                    db.SaveChanges();
                }
            }
        }

        // 9. Kiểm tra tên phòng đã tồn tại chưa
        // Chỉ kiểm tra trên những phòng đang hoạt động (TrangThai != 2)
        public bool CheckRoomNameExists(string tenPhong, int maPhongLoaiTru)
        {
            using (var db = new QuanLyKhachSanEntities())
            {
                return db.Phongs.Any(p =>
                    p.TenPhong == tenPhong &&
                    p.MaPhong != maPhongLoaiTru &&
                    p.TrangThai != 2);
            }
        }
    }
}