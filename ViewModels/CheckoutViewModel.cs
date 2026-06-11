using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using QLKhachSan.Data;
using QLKhachSan.Models;
using QLKhachSan.views;
namespace QLKhachSan.ViewModels
{
    public class CheckoutViewModel : ViewModelBase
    {
        private CheckoutRepository _repo = new CheckoutRepository();

        private ObservableCollection<HoaDon> _danhSachPhongDangThue;
        private ObservableCollection<HoaDon> _danhSachHoaDon;
        private ObservableCollection<ChiTietHD> _danhSachChiTiet;
        private ObservableCollection<DichVu> _danhSachDichVu;

        private HoaDon _selectedHoaDon;
        private HoaDon _selectedHoaDonTrongDanhSach;
        private ChiTietHD _selectedChiTiet;
        private DichVu _selectedDichVu;
        private int _soLuong;

        public ObservableCollection<HoaDon> DanhSachPhongDangThue
        {
            get => _danhSachPhongDangThue;
            set
            {
                _danhSachPhongDangThue = value;
                OnPropertyChanged(nameof(DanhSachPhongDangThue));
            }
        }

        public ObservableCollection<HoaDon> DanhSachHoaDon
        {
            get => _danhSachHoaDon;
            set
            {
                _danhSachHoaDon = value;
                OnPropertyChanged(nameof(DanhSachHoaDon));
            }
        }

        public ObservableCollection<ChiTietHD> DanhSachChiTiet
        {
            get => _danhSachChiTiet;
            set
            {
                _danhSachChiTiet = value;
                OnPropertyChanged(nameof(DanhSachChiTiet));
                OnPropertyChanged(nameof(TongTienDichVuText));
            }
        }

        public ObservableCollection<DichVu> DanhSachDichVu
        {
            get => _danhSachDichVu;
            set
            {
                _danhSachDichVu = value;
                OnPropertyChanged(nameof(DanhSachDichVu));
            }
        }

        // Hóa đơn đang chọn để thao tác dịch vụ/thanh toán
        public HoaDon SelectedHoaDon
        {
            get => _selectedHoaDon;
            set
            {
                _selectedHoaDon = value;
                OnPropertyChanged(nameof(SelectedHoaDon));

                CapNhatThongTinHoaDon();
                LoadChiTietDichVu();
            }
        }

        // Hóa đơn đang chọn ở bảng danh sách hóa đơn bên dưới
        public HoaDon SelectedHoaDonTrongDanhSach
        {
            get => _selectedHoaDonTrongDanhSach;
            set
            {
                _selectedHoaDonTrongDanhSach = value;
                OnPropertyChanged(nameof(SelectedHoaDonTrongDanhSach));

                if (_selectedHoaDonTrongDanhSach != null)
                {
                    SelectedHoaDon = _selectedHoaDonTrongDanhSach;
                }
            }
        }

        public ChiTietHD SelectedChiTiet
        {
            get => _selectedChiTiet;
            set
            {
                _selectedChiTiet = value;
                OnPropertyChanged(nameof(SelectedChiTiet));
            }
        }

        public DichVu SelectedDichVu
        {
            get => _selectedDichVu;
            set
            {
                _selectedDichVu = value;
                OnPropertyChanged(nameof(SelectedDichVu));
                OnPropertyChanged(nameof(DonGiaDichVuText));
            }
        }

        public int SoLuong
        {
            get => _soLuong;
            set
            {
                _soLuong = value;
                OnPropertyChanged(nameof(SoLuong));
            }
        }

        public string KhachHangText
        {
            get
            {
                if (SelectedHoaDon == null || SelectedHoaDon.KhachHang == null)
                    return "";

                return SelectedHoaDon.KhachHang.HoTen;
            }
        }

        public string PhongText
        {
            get
            {
                if (SelectedHoaDon == null || SelectedHoaDon.Phong == null)
                    return "";

                return SelectedHoaDon.Phong.TenPhong;
            }
        }

        public string NgayNhanText
        {
            get
            {
                if (SelectedHoaDon == null || SelectedHoaDon.NgayCheckIn == null)
                    return "";

                return SelectedHoaDon.NgayCheckIn.Value.ToString("dd/MM/yyyy HH:mm");
            }
        }

        public string NgayTraText
        {
            get
            {
                if (SelectedHoaDon == null)
                    return DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                if (SelectedHoaDon.NgayCheckOut == null)
                    return DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                return SelectedHoaDon.NgayCheckOut.Value.ToString("dd/MM/yyyy HH:mm");
            }
        }

        public string TienPhongText
        {
            get
            {
                decimal tienPhong = 0;

                if (SelectedHoaDon != null)
                    tienPhong = SelectedHoaDon.TienPhong ?? 0;

                return tienPhong.ToString("N0");
            }
        }

        public string TienDichVuText
        {
            get
            {
                decimal tienDichVu = 0;

                if (SelectedHoaDon != null)
                    tienDichVu = SelectedHoaDon.TienDichVu ?? 0;

                return tienDichVu.ToString("N0");
            }
        }

        public string TongTienText
        {
            get
            {
                decimal tongTien = 0;

                if (SelectedHoaDon != null)
                    tongTien = SelectedHoaDon.TongTien ?? 0;

                return tongTien.ToString("N0");
            }
        }

        public string TongThanhToanText
        {
            get
            {
                decimal tongTien = 0;

                if (SelectedHoaDon != null)
                    tongTien = SelectedHoaDon.TongTien ?? 0;

                return tongTien.ToString("N0");
            }
        }

        public string TongTienDichVuText
        {
            get
            {
                if (DanhSachChiTiet == null)
                    return "0";

                decimal tong = DanhSachChiTiet
                               .Select(ct => (ct.SoLuong ?? 0) * (ct.DonGia ?? 0))
                               .DefaultIfEmpty(0)
                               .Sum();

                return tong.ToString("N0");
            }
        }

        public string DonGiaDichVuText
        {
            get
            {
                if (SelectedDichVu == null)
                    return "0";

                return SelectedDichVu.DonGia.ToString("N0");
            }
        }

        public RelayCommand ThemDichVuCommand { get; set; }
        public RelayCommand XoaDichVuCommand { get; set; }
        public RelayCommand ThanhToanCommand { get; set; }
        public RelayCommand InHoaDonCommand { get; set; }
        public RelayCommand TaiLaiCommand { get; set; }

        public CheckoutViewModel()
        {
            SoLuong = 1;

            ThemDichVuCommand = new RelayCommand(ThemDichVu);
            XoaDichVuCommand = new RelayCommand(XoaDichVu);
            ThanhToanCommand = new RelayCommand(ThanhToan);
            InHoaDonCommand = new RelayCommand(InHoaDon);
            TaiLaiCommand = new RelayCommand(LoadData);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                int maHDHienTai = 0;

                if (SelectedHoaDon != null)
                    maHDHienTai = SelectedHoaDon.MaHD;

                DanhSachPhongDangThue = new ObservableCollection<HoaDon>(_repo.GetDanhSachPhongDangThue());
                DanhSachHoaDon = new ObservableCollection<HoaDon>(_repo.GetDanhSachHoaDon());
                DanhSachDichVu = new ObservableCollection<DichVu>(_repo.GetDanhSachDichVu());

                if (DanhSachChiTiet == null)
                    DanhSachChiTiet = new ObservableCollection<ChiTietHD>();

                if (maHDHienTai > 0)
                {
                    SelectedHoaDon = _repo.GetHoaDonById(maHDHienTai);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu hóa đơn: " + ex.Message);
            }
        }

        private void LoadChiTietDichVu()
        {
            try
            {
                if (SelectedHoaDon == null)
                {
                    DanhSachChiTiet = new ObservableCollection<ChiTietHD>();
                    return;
                }

                DanhSachChiTiet = new ObservableCollection<ChiTietHD>(
                    _repo.GetChiTietDichVu(SelectedHoaDon.MaHD)
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết dịch vụ: " + ex.Message);
            }
        }

        private void CapNhatThongTinHoaDon()
        {
            OnPropertyChanged(nameof(KhachHangText));
            OnPropertyChanged(nameof(PhongText));
            OnPropertyChanged(nameof(NgayNhanText));
            OnPropertyChanged(nameof(NgayTraText));
            OnPropertyChanged(nameof(TienPhongText));
            OnPropertyChanged(nameof(TienDichVuText));
            OnPropertyChanged(nameof(TongTienText));
            OnPropertyChanged(nameof(TongThanhToanText));
        }

        private bool KiemTraThemDichVu()
        {
            if (SelectedHoaDon == null)
            {
                MessageBox.Show("Vui lòng chọn hóa đơn/phòng đang thuê!");
                return false;
            }

            if (SelectedDichVu == null)
            {
                MessageBox.Show("Vui lòng chọn dịch vụ!");
                return false;
            }

            if (SoLuong <= 0)
            {
                MessageBox.Show("Số lượng phải lớn hơn 0!");
                return false;
            }

            return true;
        }

        private void ThemDichVu()
        {
            if (!KiemTraThemDichVu())
                return;

            try
            {
                int maHD = SelectedHoaDon.MaHD;

                _repo.ThemDichVuVaoHoaDon(
                    maHD,
                    SelectedDichVu.MaDV,
                    SoLuong);

                SelectedHoaDon = _repo.GetHoaDonById(maHD);
                LoadData();
                LoadChiTietDichVu();

                SelectedDichVu = null;
                SoLuong = 1;

                MessageBox.Show("Thêm dịch vụ vào hóa đơn thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm dịch vụ: " + ex.Message);
            }
        }

        private void XoaDichVu()
        {
            if (SelectedChiTiet == null)
            {
                MessageBox.Show("Vui lòng chọn dịch vụ cần xóa!");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Bạn có chắc muốn xóa dịch vụ này khỏi hóa đơn không?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;

            try
            {
                int maHD = SelectedHoaDon.MaHD;

                _repo.XoaDichVuKhoiHoaDon(SelectedChiTiet.MaCTHD);

                SelectedHoaDon = _repo.GetHoaDonById(maHD);
                LoadData();
                LoadChiTietDichVu();

                MessageBox.Show("Xóa dịch vụ thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa dịch vụ: " + ex.Message);
            }
        }

        private void ThanhToan()
        {
            if (SelectedHoaDon == null)
            {
                MessageBox.Show("Vui lòng chọn hóa đơn cần thanh toán!");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Bạn có chắc muốn thanh toán và trả phòng không?",
                "Xác nhận thanh toán",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;

            try
            {
                _repo.ThanhToanHoaDon(SelectedHoaDon.MaHD);

                MessageBox.Show("Thanh toán hóa đơn thành công!");

                SelectedHoaDon = null;
                SelectedHoaDonTrongDanhSach = null;
                SelectedChiTiet = null;
                SelectedDichVu = null;
                SoLuong = 1;
                DanhSachChiTiet = new ObservableCollection<ChiTietHD>();

                LoadData();
                CapNhatThongTinHoaDon();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thanh toán hóa đơn: " + ex.Message);
            }
        }

        private void InHoaDon()
        {
            if (SelectedHoaDonTrongDanhSach == null)
            {
                MessageBox.Show("Vui lòng chọn hóa đơn đã thanh toán cần in!");
                return;
            }

            if (SelectedHoaDonTrongDanhSach.TrangThaiThanhToan != 1)
            {
                MessageBox.Show("Chỉ được in hóa đơn đã thanh toán!");
                return;
            }

            HoaDonReportWindow window = new HoaDonReportWindow(SelectedHoaDonTrongDanhSach.MaHD);
            window.ShowDialog();
        }
    }
}