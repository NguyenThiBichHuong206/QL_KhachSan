using System;
using System.Collections.ObjectModel;
using System.Windows;
using QLKhachSan.Data;
using QLKhachSan.Models;

namespace QLKhachSan.ViewModels
{
    public class BookingViewModel : ViewModelBase
    {
        private BookingRepository _repo = new BookingRepository();
        private UserAccount _currentUser;

        private ObservableCollection<KhachHang> _danhSachKhachHang;
        private ObservableCollection<Phong> _danhSachPhongTrong;
        private ObservableCollection<HoaDon> _danhSachHoaDonDangO;

        private KhachHang _selectedKhachHang;
        private Phong _selectedPhong;
        private HoaDon _selectedHoaDon;
        private DateTime _ngayNhan;

        public ObservableCollection<KhachHang> DanhSachKhachHang
        {
            get => _danhSachKhachHang;
            set
            {
                _danhSachKhachHang = value;
                OnPropertyChanged(nameof(DanhSachKhachHang));
            }
        }

        public ObservableCollection<Phong> DanhSachPhongTrong
        {
            get => _danhSachPhongTrong;
            set
            {
                _danhSachPhongTrong = value;
                OnPropertyChanged(nameof(DanhSachPhongTrong));
            }
        }

        public ObservableCollection<HoaDon> DanhSachHoaDonDangO
        {
            get => _danhSachHoaDonDangO;
            set
            {
                _danhSachHoaDonDangO = value;
                OnPropertyChanged(nameof(DanhSachHoaDonDangO));
            }
        }

        public KhachHang SelectedKhachHang
        {
            get => _selectedKhachHang;
            set
            {
                _selectedKhachHang = value;
                OnPropertyChanged(nameof(SelectedKhachHang));
            }
        }

        public Phong SelectedPhong
        {
            get => _selectedPhong;
            set
            {
                _selectedPhong = value;
                OnPropertyChanged(nameof(SelectedPhong));
                OnPropertyChanged(nameof(ThongTinPhongText));
            }
        }

        public HoaDon SelectedHoaDon
        {
            get => _selectedHoaDon;
            set
            {
                _selectedHoaDon = value;
                OnPropertyChanged(nameof(SelectedHoaDon));
            }
        }

        public DateTime NgayNhan
        {
            get => _ngayNhan;
            set
            {
                _ngayNhan = value;
                OnPropertyChanged(nameof(NgayNhan));
            }
        }

        public string ThongTinPhongText
        {
            get
            {
                if (SelectedPhong == null)
                    return "Bạn chưa lựa chọn phòng!";

                string tenLoai = "Chưa có loại";
                string donGia = "0";

                if (SelectedPhong.LoaiPhong != null)
                {
                    tenLoai = SelectedPhong.LoaiPhong.TenLoai;
                    donGia = SelectedPhong.LoaiPhong.DonGia.ToString("N0");
                }

                return "Loại phòng: " + tenLoai + " | Giá: " + donGia + " VNĐ";
            }
        }

        public RelayCommand TaoHoaDonCommand { get; set; }
        public RelayCommand TaiLaiCommand { get; set; }

        public BookingViewModel(UserAccount currentUser)
        {
            _currentUser = currentUser;

            NgayNhan = DateTime.Now;

            TaoHoaDonCommand = new RelayCommand(TaoHoaDon);
            TaiLaiCommand = new RelayCommand(LoadData);

            LoadData();
        }

        public BookingViewModel()
            : this(null)
        {
        }

        private void LoadData()
        {
            try
            {
                DanhSachKhachHang = new ObservableCollection<KhachHang>(_repo.GetAllCustomers());
                DanhSachPhongTrong = new ObservableCollection<Phong>(_repo.GetEmptyRooms());
                DanhSachHoaDonDangO = new ObservableCollection<HoaDon>(_repo.GetActiveInvoices());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu đặt phòng: " + ex.Message);
            }
        }

        private bool KiemTraDuLieu()
        {
            if (SelectedKhachHang == null)
            {
                MessageBox.Show("Vui lòng chọn khách hàng!");
                return false;
            }

            if (SelectedPhong == null)
            {
                MessageBox.Show("Vui lòng chọn phòng trống!");
                return false;
            }

            if (NgayNhan == DateTime.MinValue)
            {
                MessageBox.Show("Vui lòng chọn ngày nhận phòng!");
                return false;
            }

            return true;
        }

        private void TaoHoaDon()
        {
            if (!KiemTraDuLieu())
                return;

            MessageBoxResult result = MessageBox.Show(
                "Bạn có chắc muốn tạo hóa đơn nhận phòng này không?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;

            try
            {
                int maNhanVien = 1;

                // Tạm thời dùng MaNV = 1.
                // Repository sẽ tự kiểm tra, nếu MaNV = 1 không tồn tại thì lấy nhân viên đầu tiên.

                _repo.CreateBooking(
                    SelectedKhachHang.MaKH,
                    SelectedPhong.MaPhong,
                    maNhanVien,
                    NgayNhan);

                MessageBox.Show("Nhận phòng thành công!");

                SelectedKhachHang = null;
                SelectedPhong = null;
                NgayNhan = DateTime.Now;

                LoadData();
            }
            catch (Exception ex)
            {
                string loi = ex.Message;

                Exception inner = ex.InnerException;
                while (inner != null)
                {
                    loi += "\n\nChi tiết: " + inner.Message;
                    inner = inner.InnerException;
                }

                MessageBox.Show("Lỗi nhận phòng: " + loi);
            }
        }
    }
}