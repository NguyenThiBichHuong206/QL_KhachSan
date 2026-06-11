using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using QLKhachSan.Data;
using QLKhachSan.Models;

namespace QLKhachSan.ViewModels
{
    public class RoomViewModel : ViewModelBase
    {
        private RoomRepository _repo = new RoomRepository();

        // Use display item to carry both relative and absolute paths for UI
        public class RoomDisplayItem
        {
            public int MaPhong { get; set; }
            public string TenPhong { get; set; }
            public int? MaLoai { get; set; }
            public int? TrangThai { get; set; }
            // Relative path stored in DB, e.g. "Images/Rooms/room_....jpg"
            public string HinhAnh { get; set; }
            // Absolute file path computed for UI preview/thumbnail, null if not exists
            public string HinhAnhFullPath { get; set; }
        }

        private ObservableCollection<RoomDisplayItem> _danhSachPhong;
        private ObservableCollection<LoaiPhong> _danhSachLoaiPhong;
        private RoomDisplayItem _selectedRoom;
        private Phong _newPhong;
        private string _tuKhoaTimKiem;
        private string _hinhAnhPreviewPath;

        public ObservableCollection<RoomDisplayItem> DanhSachPhong
        {
            get => _danhSachPhong;
            set
            {
                _danhSachPhong = value;
                OnPropertyChanged(nameof(DanhSachPhong));
            }
        }

        public ObservableCollection<LoaiPhong> DanhSachLoaiPhong
        {
            get => _danhSachLoaiPhong;
            set
            {
                _danhSachLoaiPhong = value;
                OnPropertyChanged(nameof(DanhSachLoaiPhong));
            }
        }

        // Selected item in DataGrid (display item). When selection changes, populate NewPhong and preview path.
        public RoomDisplayItem SelectedRoom
        {
            get => _selectedRoom;
            set
            {
                _selectedRoom = value;
                OnPropertyChanged(nameof(SelectedRoom));

                if (_selectedRoom != null)
                {
                    NewPhong = new Phong
                    {
                        MaPhong = _selectedRoom.MaPhong,
                        TenPhong = _selectedRoom.TenPhong,
                        MaLoai = _selectedRoom.MaLoai,
                        TrangThai = _selectedRoom.TrangThai,
                        HinhAnh = _selectedRoom.HinhAnh
                    };

                    UpdatePreviewPath();
                }
            }
        }

        public Phong NewPhong
        {
            get => _newPhong;
            set
            {
                _newPhong = value;
                OnPropertyChanged(nameof(NewPhong));
                UpdatePreviewPath();
            }
        }

        public string HinhAnhPreviewPath
        {
            get => _hinhAnhPreviewPath;
            set
            {
                _hinhAnhPreviewPath = value;
                OnPropertyChanged(nameof(HinhAnhPreviewPath));
            }
        }

        public string TuKhoaTimKiem
        {
            get => _tuKhoaTimKiem;
            set
            {
                _tuKhoaTimKiem = value;
                OnPropertyChanged(nameof(TuKhoaTimKiem));
            }
        }

        public RelayCommand AddCommand { get; set; }
        public RelayCommand UpdateCommand { get; set; }
        public RelayCommand DeleteCommand { get; set; }
        public RelayCommand SearchCommand { get; set; }
        public RelayCommand ChooseImageCommand { get; set; }

        public RoomViewModel()
        {
            NewPhong = new Phong { TrangThai = 0 };

            AddCommand = new RelayCommand(Add);
            UpdateCommand = new RelayCommand(Update);
            DeleteCommand = new RelayCommand(Delete);
            SearchCommand = new RelayCommand(Search);
            ChooseImageCommand = new RelayCommand(ChooseImage);

            LoadData();
            LoadLoaiPhong();
        }

        private void LoadData()
        {
            var list = _repo.GetAllRooms();
            DanhSachPhong = new ObservableCollection<RoomDisplayItem>(
                list.Select(p => MapToDisplayItem(p))
            );
        }

        private void LoadLoaiPhong()
        {
            DanhSachLoaiPhong = new ObservableCollection<LoaiPhong>(_repo.GetAllRoomTypes());
        }

        private RoomDisplayItem MapToDisplayItem(Phong p)
        {
            string rel = p.HinhAnh;
            string full = null;
            if (!string.IsNullOrWhiteSpace(rel))
            {
                // Normalize separators then combine
                string normalized = rel.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
                string candidate = Path.IsPathRooted(normalized) ? normalized : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, normalized);
                if (File.Exists(candidate))
                    full = candidate;
            }

            return new RoomDisplayItem
            {
                MaPhong = p.MaPhong,
                TenPhong = p.TenPhong,
                MaLoai = p.MaLoai,
                TrangThai = p.TrangThai,
                HinhAnh = p.HinhAnh,
                HinhAnhFullPath = full
            };
        }

        private void LamMoiForm()
        {
            NewPhong = new Phong { TrangThai = 0 };
            SelectedRoom = null;
            HinhAnhPreviewPath = null;
        }

        private void UpdatePreviewPath()
        {
            try
            {
                if (NewPhong == null || string.IsNullOrWhiteSpace(NewPhong.HinhAnh))
                {
                    HinhAnhPreviewPath = null;
                    return;
                }

                string relative = NewPhong.HinhAnh.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
                string abs = Path.IsPathRooted(relative) ? relative : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relative);

                if (File.Exists(abs))
                    HinhAnhPreviewPath = abs;
                else
                    HinhAnhPreviewPath = null;
            }
            catch
            {
                HinhAnhPreviewPath = null;
            }
        }

        private bool KiemTraDuLieu()
        {
            if (NewPhong == null)
            {
                MessageBox.Show("Dữ liệu phòng không hợp lệ!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(NewPhong.TenPhong))
            {
                MessageBox.Show("Tên phòng không được để trống!");
                return false;
            }

            if (NewPhong.MaLoai == null || NewPhong.MaLoai <= 0)
            {
                MessageBox.Show("Vui lòng chọn loại phòng!");
                return false;
            }

            if (NewPhong.TrangThai == null)
            {
                MessageBox.Show("Vui lòng chọn trạng thái phòng!");
                return false;
            }

            return true;
        }

        private void Add()
        {
            if (!KiemTraDuLieu())
                return;

            try
            {
                // 1) Kiểm tra có phòng cùng tên đã bị xóa mềm trước đó không
                var deleted = _repo.GetDeletedRoomByName(NewPhong.TenPhong);
                if (deleted != null)
                {
                    var result = MessageBox.Show(
                        "Phòng này từng tồn tại và đã bị ngừng hoạt động trước đó. Bạn có muốn tái kích hoạt lại phòng này với thông tin mới không?",
                        "Xác nhận tái kích hoạt",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Cập nhật thông tin và tái kích hoạt (đặt lại trạng thái = 0)
                        deleted.MaLoai = NewPhong.MaLoai;
                        deleted.HinhAnh = NewPhong.HinhAnh;
                        deleted.TrangThai = 0;
                        // TenPhong giữ nguyên (trùng tên)
                        _repo.UpdateRoom(deleted);

                        MessageBox.Show("Phòng đã được tái kích hoạt thành công!");
                        LoadData();
                        LamMoiForm();
                        return;
                    }
                    else
                    {
                        // Người dùng chọn không tái kích hoạt -> dừng hành động thêm
                        return;
                    }
                }

                // 2) Nếu không có phòng đã xóa mềm trùng tên -> kiểm tra trùng tên trên các phòng đang hoạt động
                if (_repo.CheckRoomNameExists(NewPhong.TenPhong, 0))
                {
                    MessageBox.Show("Tên phòng đã tồn tại!");
                    return;
                }

                // 3) Thêm mới bình thường
                Phong phong = new Phong
                {
                    TenPhong = NewPhong.TenPhong,
                    MaLoai = NewPhong.MaLoai,
                    TrangThai = NewPhong.TrangThai,
                    HinhAnh = NewPhong.HinhAnh
                };

                _repo.AddRoom(phong);

                MessageBox.Show("Thêm phòng thành công!");
                LoadData();
                LamMoiForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm phòng: " + ex.Message);
            }
        }

        private void Update()
        {
            if (SelectedRoom == null)
            {
                MessageBox.Show("Vui lòng chọn phòng cần sửa!");
                return;
            }

            if (!KiemTraDuLieu())
                return;

            try
            {
                if (_repo.CheckRoomNameExists(NewPhong.TenPhong, NewPhong.MaPhong))
                {
                    MessageBox.Show("Tên phòng đã tồn tại!");
                    return;
                }

                Phong phong = new Phong
                {
                    MaPhong = NewPhong.MaPhong,
                    TenPhong = NewPhong.TenPhong,
                    MaLoai = NewPhong.MaLoai,
                    TrangThai = NewPhong.TrangThai,
                    HinhAnh = NewPhong.HinhAnh
                };

                _repo.UpdateRoom(phong);

                MessageBox.Show("Cập nhật phòng thành công!");
                LoadData();
                LamMoiForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật phòng: " + ex.Message);
            }
        }

        private void Delete()
        {
            if (SelectedRoom == null)
            {
                MessageBox.Show("Vui lòng chọn phòng cần xóa!");
                return;
            }

            if (SelectedRoom.TrangThai == 1)
            {
                MessageBox.Show("Phòng đang thuê, không thể xóa!");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Bạn có chắc muốn ngừng hoạt động và ẩn phòng này không?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;

            try
            {
                _repo.DeleteRoom(SelectedRoom.MaPhong);

                MessageBox.Show("Đã ngừng hoạt động và ẩn phòng thành công!");
                LoadData();
                LamMoiForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi ngừng hoạt động phòng: " + ex.Message);
            }
        }

        private void Search()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TuKhoaTimKiem))
                {
                    LoadData();
                    return;
                }

                var list = _repo.SearchRooms(TuKhoaTimKiem);
                DanhSachPhong = new ObservableCollection<RoomDisplayItem>(list.Select(p => MapToDisplayItem(p)));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tìm kiếm phòng: " + ex.Message);
            }
        }

        private void ChooseImage()
        {
            try
            {
                var dlg = new OpenFileDialog
                {
                    Title = "Chọn ảnh phòng",
                    Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                    Multiselect = false
                };

                if (dlg.ShowDialog() != true)
                    return;

                string sourcePath = dlg.FileName;
                string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Rooms");
                Directory.CreateDirectory(imagesFolder);

                string ext = Path.GetExtension(sourcePath);
                string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string fileName = $"room_{timeStamp}{ext}";
                string destPath = Path.Combine(imagesFolder, fileName);

                File.Copy(sourcePath, destPath);

                // Save relative path: Images/Rooms/<file>
                string relativePath = $"Images/Rooms/{fileName}";
                if (NewPhong == null)
                    NewPhong = new Phong();

                NewPhong.HinhAnh = relativePath;

                // Update preview absolute path
                UpdatePreviewPath();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi chọn ảnh: " + ex.Message);
            }
        }
    }
}