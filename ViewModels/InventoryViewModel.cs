using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using QLKhachSan.Data;
using QLKhachSan.Models;

namespace QLKhachSan.ViewModels
{
    public class InventoryViewModel : ViewModelBase
    {
        private InventoryRepository _repository = new InventoryRepository();

        private ObservableCollection<NhaCungCap> _danhSachNhaCungCap;
        private ObservableCollection<HangHoa> _danhSachHangHoa;
        private ObservableCollection<ChiTietNhapKho> _danhSachChiTietTam;
        private ObservableCollection<PhieuNhapKho> _danhSachPhieuNhap;
        private ObservableCollection<ChiTietNhapKho> _danhSachChiTietPhieuNhap;

        private NhaCungCap _selectedNhaCungCap;
        private HangHoa _selectedHangHoa;
        private ChiTietNhapKho _selectedChiTietTam;
        private PhieuNhapKho _selectedPhieuNhap;

        private DateTime _ngayNhap;
        private DateTime _tuNgay;
        private DateTime _denNgay;

        private int _soLuong;
        private decimal _donGia;
        private bool _dangTaoPhieu;
        private string _message;
        private string _tuKhoaNhaCungCap;
        private string _tuKhoaMaPhieu;

        // Thông tin nhà cung cấp
        private string _tenNCCMoi;
        private string _diaChiNCCMoi;
        private string _sdtNCCMoi;

        // Thông tin hàng hóa
        private string _tenHangMoi;
        private string _donViTinhMoi;
        private string _giaNhapMoi;

        public InventoryViewModel()
        {
            NgayNhap = DateTime.Now;
            TuNgay = DateTime.Now.AddMonths(-1);
            DenNgay = DateTime.Now;

            SoLuong = 1;
            DonGia = 0;
            DangTaoPhieu = false;

            DanhSachChiTietTam = new ObservableCollection<ChiTietNhapKho>();
            DanhSachChiTietPhieuNhap = new ObservableCollection<ChiTietNhapKho>();

            TaoPhieuCommand = new RelayCommand(TaoPhieu);
            ThemHangCommand = new RelayCommand(ThemHang);
            XoaHangCommand = new RelayCommand(XoaHang);
            LuuPhieuNhapCommand = new RelayCommand(LuuPhieuNhap);
            HuyPhieuCommand = new RelayCommand(HuyPhieu);
            TimKiemCommand = new RelayCommand(TimKiemPhieuNhap);

            ThemNhaCungCapCommand = new RelayCommand(ThemNhaCungCap);
            SuaNhaCungCapCommand = new RelayCommand(SuaNhaCungCap);
            XoaNhaCungCapCommand = new RelayCommand(XoaNhaCungCap);

            ThemHangHoaMoiCommand = new RelayCommand(ThemHangHoaMoi);
            SuaHangHoaCommand = new RelayCommand(SuaHangHoa);
            XoaHangHoaCommand = new RelayCommand(XoaHangHoa);

            LoadData();
        }

        // =========================
        // DANH SÁCH DỮ LIỆU
        // =========================

        public ObservableCollection<NhaCungCap> DanhSachNhaCungCap
        {
            get { return _danhSachNhaCungCap; }
            set
            {
                _danhSachNhaCungCap = value;
                OnPropertyChanged(nameof(DanhSachNhaCungCap));
            }
        }

        public ObservableCollection<HangHoa> DanhSachHangHoa
        {
            get { return _danhSachHangHoa; }
            set
            {
                _danhSachHangHoa = value;
                OnPropertyChanged(nameof(DanhSachHangHoa));
            }
        }

        public ObservableCollection<ChiTietNhapKho> DanhSachChiTietTam
        {
            get { return _danhSachChiTietTam; }
            set
            {
                _danhSachChiTietTam = value;
                OnPropertyChanged(nameof(DanhSachChiTietTam));
                OnPropertyChanged(nameof(TongTienText));
            }
        }

        public ObservableCollection<PhieuNhapKho> DanhSachPhieuNhap
        {
            get { return _danhSachPhieuNhap; }
            set
            {
                _danhSachPhieuNhap = value;
                OnPropertyChanged(nameof(DanhSachPhieuNhap));
                OnPropertyChanged(nameof(SoKetQuaText));
            }
        }

        public ObservableCollection<ChiTietNhapKho> DanhSachChiTietPhieuNhap
        {
            get { return _danhSachChiTietPhieuNhap; }
            set
            {
                _danhSachChiTietPhieuNhap = value;
                OnPropertyChanged(nameof(DanhSachChiTietPhieuNhap));
            }
        }

        // =========================
        // SELECTED ITEM
        // =========================

        public NhaCungCap SelectedNhaCungCap
        {
            get { return _selectedNhaCungCap; }
            set
            {
                _selectedNhaCungCap = value;
                OnPropertyChanged(nameof(SelectedNhaCungCap));
                CapNhatThongTinNhaCungCap();
            }
        }

        public HangHoa SelectedHangHoa
        {
            get { return _selectedHangHoa; }
            set
            {
                _selectedHangHoa = value;
                OnPropertyChanged(nameof(SelectedHangHoa));
                CapNhatThongTinHangHoa();
            }
        }

        public ChiTietNhapKho SelectedChiTietTam
        {
            get { return _selectedChiTietTam; }
            set
            {
                _selectedChiTietTam = value;
                OnPropertyChanged(nameof(SelectedChiTietTam));
            }
        }

        public PhieuNhapKho SelectedPhieuNhap
        {
            get { return _selectedPhieuNhap; }
            set
            {
                _selectedPhieuNhap = value;
                OnPropertyChanged(nameof(SelectedPhieuNhap));
                LoadChiTietPhieuNhap();
            }
        }

        // =========================
        // THUỘC TÍNH NHẬP PHIẾU
        // =========================

        public DateTime NgayNhap
        {
            get { return _ngayNhap; }
            set
            {
                _ngayNhap = value;
                OnPropertyChanged(nameof(NgayNhap));
            }
        }

        public DateTime TuNgay
        {
            get { return _tuNgay; }
            set
            {
                _tuNgay = value;
                OnPropertyChanged(nameof(TuNgay));
            }
        }

        public DateTime DenNgay
        {
            get { return _denNgay; }
            set
            {
                _denNgay = value;
                OnPropertyChanged(nameof(DenNgay));
            }
        }

        public int SoLuong
        {
            get { return _soLuong; }
            set
            {
                _soLuong = value;
                OnPropertyChanged(nameof(SoLuong));
                OnPropertyChanged(nameof(ThanhTienTamText));
            }
        }

        public decimal DonGia
        {
            get { return _donGia; }
            set
            {
                _donGia = value;
                OnPropertyChanged(nameof(DonGia));
                OnPropertyChanged(nameof(ThanhTienTamText));
            }
        }

        public bool DangTaoPhieu
        {
            get { return _dangTaoPhieu; }
            set
            {
                _dangTaoPhieu = value;
                OnPropertyChanged(nameof(DangTaoPhieu));
                OnPropertyChanged(nameof(ChoPhepChonNhaCungCap));
                OnPropertyChanged(nameof(ChoPhepNhapChiTiet));
                OnPropertyChanged(nameof(ChoPhepLuuPhieu));
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        // =========================
        // NHÀ CUNG CẤP
        // =========================

        public string TenNCCMoi
        {
            get { return _tenNCCMoi; }
            set
            {
                _tenNCCMoi = value;
                OnPropertyChanged(nameof(TenNCCMoi));
            }
        }

        public string DiaChiNCCMoi
        {
            get { return _diaChiNCCMoi; }
            set
            {
                _diaChiNCCMoi = value;
                OnPropertyChanged(nameof(DiaChiNCCMoi));
            }
        }

        public string SDTNCCMoi
        {
            get { return _sdtNCCMoi; }
            set
            {
                _sdtNCCMoi = value;
                OnPropertyChanged(nameof(SDTNCCMoi));
            }
        }

        // =========================
        // HÀNG HÓA
        // =========================

        public string TenHangMoi
        {
            get { return _tenHangMoi; }
            set
            {
                _tenHangMoi = value;
                OnPropertyChanged(nameof(TenHangMoi));
            }
        }

        public string DonViTinhMoi
        {
            get { return _donViTinhMoi; }
            set
            {
                _donViTinhMoi = value;
                OnPropertyChanged(nameof(DonViTinhMoi));
            }
        }

        public string GiaNhapMoi
        {
            get { return _giaNhapMoi; }
            set
            {
                _giaNhapMoi = value;
                OnPropertyChanged(nameof(GiaNhapMoi));
            }
        }

        // =========================
        // TÌM KIẾM
        // =========================

        public string TuKhoaNhaCungCap
        {
            get { return _tuKhoaNhaCungCap; }
            set
            {
                _tuKhoaNhaCungCap = value;
                OnPropertyChanged(nameof(TuKhoaNhaCungCap));
            }
        }

        public string TuKhoaMaPhieu
        {
            get { return _tuKhoaMaPhieu; }
            set
            {
                _tuKhoaMaPhieu = value;
                OnPropertyChanged(nameof(TuKhoaMaPhieu));
            }
        }

        public string SoKetQuaText
        {
            get
            {
                int soLuong = 0;

                if (DanhSachPhieuNhap != null)
                    soLuong = DanhSachPhieuNhap.Count;

                return "Tìm thấy " + soLuong + " phiếu nhập";
            }
        }

        // =========================
        // THUỘC TÍNH HIỂN THỊ
        // =========================

        public bool ChoPhepChonNhaCungCap
        {
            get { return !DangTaoPhieu; }
        }

        public bool ChoPhepNhapChiTiet
        {
            get { return DangTaoPhieu; }
        }

        public bool ChoPhepLuuPhieu
        {
            get
            {
                return DangTaoPhieu &&
                       DanhSachChiTietTam != null &&
                       DanhSachChiTietTam.Count > 0;
            }
        }

        public string DonViTinhText
        {
            get
            {
                if (SelectedHangHoa == null)
                    return "";

                return SelectedHangHoa.DonViTinh;
            }
        }

        public string TonKhoText
        {
            get
            {
                if (SelectedHangHoa == null)
                    return "0";

                return (SelectedHangHoa.SoLuongTon ?? 0).ToString();
            }
        }

        public string ThanhTienTamText
        {
            get
            {
                decimal thanhTien = SoLuong * DonGia;
                return thanhTien.ToString("N0");
            }
        }

        public decimal TongTien
        {
            get
            {
                if (DanhSachChiTietTam == null)
                    return 0;

                return DanhSachChiTietTam.Sum(ct => ct.SoLuong * ct.DonGia);
            }
        }

        public string TongTienText
        {
            get { return TongTien.ToString("N0"); }
        }

        // =========================
        // COMMAND
        // =========================

        public RelayCommand TaoPhieuCommand { get; set; }
        public RelayCommand ThemHangCommand { get; set; }
        public RelayCommand XoaHangCommand { get; set; }
        public RelayCommand LuuPhieuNhapCommand { get; set; }
        public RelayCommand HuyPhieuCommand { get; set; }
        public RelayCommand TimKiemCommand { get; set; }

        public RelayCommand ThemNhaCungCapCommand { get; set; }
        public RelayCommand SuaNhaCungCapCommand { get; set; }
        public RelayCommand XoaNhaCungCapCommand { get; set; }

        public RelayCommand ThemHangHoaMoiCommand { get; set; }
        public RelayCommand SuaHangHoaCommand { get; set; }
        public RelayCommand XoaHangHoaCommand { get; set; }

        // =========================
        // LOAD DATA
        // =========================

        private void LoadData()
        {
            try
            {
                DanhSachNhaCungCap = new ObservableCollection<NhaCungCap>(
                    _repository.GetAllSuppliers());

                DanhSachHangHoa = new ObservableCollection<HangHoa>(
                    _repository.GetAllGoods());

                DanhSachPhieuNhap = new ObservableCollection<PhieuNhapKho>(
                    _repository.GetAllReceipts());

                if (DanhSachNhaCungCap.Count > 0 && SelectedNhaCungCap == null)
                    SelectedNhaCungCap = DanhSachNhaCungCap.FirstOrDefault();

                if (DanhSachHangHoa.Count > 0 && SelectedHangHoa == null)
                    SelectedHangHoa = DanhSachHangHoa.FirstOrDefault();

                Message = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu nhập kho: " + ex.Message);
            }
        }

        private void CapNhatThongTinNhaCungCap()
        {
            if (SelectedNhaCungCap != null)
            {
                TenNCCMoi = SelectedNhaCungCap.TenNCC;
                DiaChiNCCMoi = SelectedNhaCungCap.DiaChi;
                SDTNCCMoi = SelectedNhaCungCap.SDT;
            }
        }

        private void CapNhatThongTinHangHoa()
        {
            if (SelectedHangHoa != null)
            {
                DonGia = SelectedHangHoa.GiaNhap ?? 0;

                TenHangMoi = SelectedHangHoa.TenHang;
                DonViTinhMoi = SelectedHangHoa.DonViTinh;
                GiaNhapMoi = (SelectedHangHoa.GiaNhap ?? 0).ToString("G29");
            }

            OnPropertyChanged(nameof(DonViTinhText));
            OnPropertyChanged(nameof(TonKhoText));
            OnPropertyChanged(nameof(ThanhTienTamText));
        }

        // =========================
        // THÊM / SỬA / XÓA NHÀ CUNG CẤP
        // =========================

        private bool KiemTraThongTinNhaCungCap()
        {
            if (string.IsNullOrWhiteSpace(TenNCCMoi))
            {
                MessageBox.Show("Vui lòng nhập tên nhà cung cấp!");
                return false;
            }

            return true;
        }

        private void ThemNhaCungCap()
        {
            if (!KiemTraThongTinNhaCungCap())
                return;

            try
            {
                NhaCungCap nhaCungCap = new NhaCungCap
                {
                    TenNCC = TenNCCMoi.Trim(),
                    DiaChi = string.IsNullOrWhiteSpace(DiaChiNCCMoi) ? "" : DiaChiNCCMoi.Trim(),
                    SDT = string.IsNullOrWhiteSpace(SDTNCCMoi) ? "" : SDTNCCMoi.Trim()
                };

                _repository.AddSupplier(nhaCungCap);

                MessageBox.Show("Thêm nhà cung cấp thành công!");

                LoadData();

                SelectedNhaCungCap = DanhSachNhaCungCap
                    .FirstOrDefault(ncc => ncc.TenNCC == nhaCungCap.TenNCC);

                Message = "Đã thêm nhà cung cấp mới.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm nhà cung cấp: " + ex.Message);
            }
        }

        private void SuaNhaCungCap()
        {
            if (SelectedNhaCungCap == null)
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp cần sửa!");
                return;
            }

            if (!KiemTraThongTinNhaCungCap())
                return;

            try
            {
                NhaCungCap nhaCungCap = new NhaCungCap
                {
                    MaNCC = SelectedNhaCungCap.MaNCC,
                    TenNCC = TenNCCMoi.Trim(),
                    DiaChi = string.IsNullOrWhiteSpace(DiaChiNCCMoi) ? "" : DiaChiNCCMoi.Trim(),
                    SDT = string.IsNullOrWhiteSpace(SDTNCCMoi) ? "" : SDTNCCMoi.Trim()
                };

                _repository.UpdateSupplier(nhaCungCap);

                MessageBox.Show("Sửa nhà cung cấp thành công!");

                SelectedNhaCungCap = null;
                LoadData();

                Message = "Đã sửa nhà cung cấp.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi sửa nhà cung cấp: " + ex.Message);
            }
        }

        private void XoaNhaCungCap()
        {
            if (SelectedNhaCungCap == null)
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp cần xóa!");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Bạn có chắc muốn xóa nhà cung cấp '" + SelectedNhaCungCap.TenNCC + "' không?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;

            try
            {
                _repository.DeleteSupplier(SelectedNhaCungCap.MaNCC);

                MessageBox.Show("Xóa nhà cung cấp thành công!");

                SelectedNhaCungCap = null;
                TenNCCMoi = "";
                DiaChiNCCMoi = "";
                SDTNCCMoi = "";

                LoadData();

                Message = "Đã xóa nhà cung cấp.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa nhà cung cấp: " + ex.Message);
            }
        }

        // =========================
        // THÊM / SỬA / XÓA HÀNG HÓA
        // =========================

        private bool KiemTraThongTinHangHoa(out decimal giaNhap)
        {
            giaNhap = 0;

            if (string.IsNullOrWhiteSpace(TenHangMoi))
            {
                MessageBox.Show("Vui lòng nhập tên hàng hóa!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(DonViTinhMoi))
            {
                MessageBox.Show("Vui lòng nhập đơn vị tính!");
                return false;
            }

            if (!decimal.TryParse(GiaNhapMoi, out giaNhap) || giaNhap < 0)
            {
                MessageBox.Show("Giá nhập phải là số không âm!");
                return false;
            }

            return true;
        }

        private void ThemHangHoaMoi()
        {
            if (!KiemTraThongTinHangHoa(out decimal giaNhap))
                return;

            try
            {
                HangHoa hangHoa = new HangHoa
                {
                    TenHang = TenHangMoi.Trim(),
                    DonViTinh = DonViTinhMoi.Trim(),
                    GiaNhap = giaNhap,
                    SoLuongTon = 0
                };

                _repository.AddGoods(hangHoa);

                MessageBox.Show("Thêm hàng hóa thành công. Tồn kho ban đầu là 0, hãy nhập hàng qua phiếu nhập.");

                LoadData();

                SelectedHangHoa = DanhSachHangHoa
                    .FirstOrDefault(hh => hh.TenHang == hangHoa.TenHang);

                Message = "Đã thêm hàng hóa mới.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm hàng hóa: " + ex.Message);
            }
        }

        private void SuaHangHoa()
        {
            if (SelectedHangHoa == null)
            {
                MessageBox.Show("Vui lòng chọn hàng hóa cần sửa!");
                return;
            }

            if (!KiemTraThongTinHangHoa(out decimal giaNhap))
                return;

            try
            {
                HangHoa hangHoa = new HangHoa
                {
                    MaHang = SelectedHangHoa.MaHang,
                    TenHang = TenHangMoi.Trim(),
                    DonViTinh = DonViTinhMoi.Trim(),
                    GiaNhap = giaNhap
                };

                _repository.UpdateGoods(hangHoa);

                MessageBox.Show("Sửa hàng hóa thành công!");

                SelectedHangHoa = null;
                LoadData();

                Message = "Đã sửa hàng hóa.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi sửa hàng hóa: " + ex.Message);
            }
        }

        private void XoaHangHoa()
        {
            if (SelectedHangHoa == null)
            {
                MessageBox.Show("Vui lòng chọn hàng hóa cần xóa!");
                return;
            }

            if (DanhSachChiTietTam != null &&
                DanhSachChiTietTam.Any(ct => ct.MaHang == SelectedHangHoa.MaHang))
            {
                MessageBox.Show("Hàng hóa này đang nằm trong phiếu nhập đang lập. Hãy xóa dòng chi tiết trước.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Bạn có chắc muốn xóa hàng hóa '" + SelectedHangHoa.TenHang + "' không?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;

            try
            {
                _repository.DeleteGoods(SelectedHangHoa.MaHang);

                MessageBox.Show("Xóa hàng hóa thành công!");

                SelectedHangHoa = null;
                TenHangMoi = "";
                DonViTinhMoi = "";
                GiaNhapMoi = "";

                LoadData();

                Message = "Đã xóa hàng hóa.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa hàng hóa: " + ex.Message);
            }
        }

        // =========================
        // TẠO PHIẾU
        // =========================

        private void TaoPhieu()
        {
            if (SelectedNhaCungCap == null)
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp trước khi tạo phiếu!");
                return;
            }

            DangTaoPhieu = true;
            NgayNhap = DateTime.Now;
            DanhSachChiTietTam.Clear();

            OnPropertyChanged(nameof(TongTienText));
            OnPropertyChanged(nameof(ChoPhepLuuPhieu));

            Message = "Đang tạo phiếu nhập mới.";
        }

        // =========================
        // THÊM HÀNG VÀO PHIẾU TẠM
        // =========================

        private bool KiemTraThemHang()
        {
            if (!DangTaoPhieu)
            {
                MessageBox.Show("Vui lòng bấm Tạo phiếu trước khi thêm hàng!");
                return false;
            }

            if (SelectedHangHoa == null)
            {
                MessageBox.Show("Vui lòng chọn hàng hóa!");
                return false;
            }

            if (SoLuong <= 0)
            {
                MessageBox.Show("Số lượng nhập phải lớn hơn 0!");
                return false;
            }

            if (DonGia <= 0)
            {
                MessageBox.Show("Đơn giá nhập phải lớn hơn 0!");
                return false;
            }

            return true;
        }

        private void ThemHang()
        {
            if (!KiemTraThemHang())
                return;

            try
            {
                ChiTietNhapKho dongCu = DanhSachChiTietTam
                    .FirstOrDefault(ct => ct.MaHang == SelectedHangHoa.MaHang &&
                                          ct.DonGia == DonGia);

                if (dongCu != null)
                {
                    dongCu.SoLuong = dongCu.SoLuong + SoLuong;

                    DanhSachChiTietTam = new ObservableCollection<ChiTietNhapKho>(
                        DanhSachChiTietTam);
                }
                else
                {
                    ChiTietNhapKho chiTiet = new ChiTietNhapKho
                    {
                        MaPhieuNhap = 0,
                        MaHang = SelectedHangHoa.MaHang,
                        SoLuong = SoLuong,
                        DonGia = DonGia,
                        HangHoa = SelectedHangHoa
                    };

                    DanhSachChiTietTam.Add(chiTiet);
                }

                SoLuong = 1;

                OnPropertyChanged(nameof(TongTienText));
                OnPropertyChanged(nameof(ChoPhepLuuPhieu));

                Message = "Đã thêm hàng vào chi tiết phiếu nhập.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm hàng nhập: " + ex.Message);
            }
        }

        // =========================
        // XÓA DÒNG CHI TIẾT TẠM
        // =========================

        private void XoaHang()
        {
            if (SelectedChiTietTam == null)
            {
                MessageBox.Show("Vui lòng chọn dòng hàng cần xóa!");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Bạn có chắc muốn xóa dòng hàng này không?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;

            DanhSachChiTietTam.Remove(SelectedChiTietTam);
            SelectedChiTietTam = null;

            OnPropertyChanged(nameof(TongTienText));
            OnPropertyChanged(nameof(ChoPhepLuuPhieu));

            Message = "Đã xóa dòng hàng khỏi phiếu nhập.";
        }

        // =========================
        // LƯU PHIẾU NHẬP
        // =========================

        private bool KiemTraLuuPhieuNhap()
        {
            if (!DangTaoPhieu)
            {
                MessageBox.Show("Vui lòng tạo phiếu nhập trước!");
                return false;
            }

            if (SelectedNhaCungCap == null)
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp!");
                return false;
            }

            if (DanhSachChiTietTam == null || DanhSachChiTietTam.Count == 0)
            {
                MessageBox.Show("Phiếu nhập chưa có chi tiết hàng hóa!");
                return false;
            }

            foreach (ChiTietNhapKho item in DanhSachChiTietTam)
            {
                if (item.SoLuong <= 0)
                {
                    MessageBox.Show("Số lượng nhập phải lớn hơn 0!");
                    return false;
                }

                if (item.DonGia <= 0)
                {
                    MessageBox.Show("Đơn giá nhập phải lớn hơn 0!");
                    return false;
                }
            }

            return true;
        }

        private void LuuPhieuNhap()
        {
            if (!KiemTraLuuPhieuNhap())
                return;

            MessageBoxResult result = MessageBox.Show(
                "Bạn có chắc muốn lưu phiếu nhập này không?",
                "Xác nhận lưu",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;

            try
            {
                int maNhanVienDangNhap = 1;

                var danhSachChiTietLuu = DanhSachChiTietTam
                    .Select(ct => new ChiTietNhapKho
                    {
                        MaHang = ct.MaHang,
                        SoLuong = ct.SoLuong,
                        DonGia = ct.DonGia
                    })
                    .ToList();

                _repository.SaveReceipt(
                    SelectedNhaCungCap.MaNCC,
                    maNhanVienDangNhap,
                    NgayNhap,
                    danhSachChiTietLuu);

                MessageBox.Show("Lưu phiếu nhập thành công!");

                ResetPhieuNhap();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu phiếu nhập: " + ex.Message);
            }
        }

        // =========================
        // HỦY PHIẾU
        // =========================

        private void HuyPhieu()
        {
            if (DanhSachChiTietTam != null && DanhSachChiTietTam.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Phiếu nhập đang có chi tiết. Bạn có chắc muốn hủy không?",
                    "Xác nhận hủy",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                    return;
            }

            ResetPhieuNhap();
            Message = "Đã hủy phiếu nhập.";
        }

        private void ResetPhieuNhap()
        {
            DangTaoPhieu = false;
            NgayNhap = DateTime.Now;
            SoLuong = 1;
            DonGia = 0;

            DanhSachChiTietTam = new ObservableCollection<ChiTietNhapKho>();
            DanhSachChiTietPhieuNhap = new ObservableCollection<ChiTietNhapKho>();

            SelectedChiTietTam = null;
            SelectedPhieuNhap = null;

            if (DanhSachNhaCungCap != null && DanhSachNhaCungCap.Count > 0)
                SelectedNhaCungCap = DanhSachNhaCungCap.FirstOrDefault();

            if (DanhSachHangHoa != null && DanhSachHangHoa.Count > 0)
                SelectedHangHoa = DanhSachHangHoa.FirstOrDefault();

            OnPropertyChanged(nameof(TongTienText));
            OnPropertyChanged(nameof(ChoPhepChonNhaCungCap));
            OnPropertyChanged(nameof(ChoPhepNhapChiTiet));
            OnPropertyChanged(nameof(ChoPhepLuuPhieu));

            Message = "";
        }

        // =========================
        // TÌM KIẾM PHIẾU NHẬP
        // =========================

        private void TimKiemPhieuNhap()
        {
            try
            {
                int? maPhieuNhap = null;

                if (!string.IsNullOrWhiteSpace(TuKhoaMaPhieu))
                {
                    int maPhieu;

                    if (!int.TryParse(TuKhoaMaPhieu.Trim(), out maPhieu))
                    {
                        MessageBox.Show("Mã phiếu nhập phải là số!");
                        return;
                    }

                    maPhieuNhap = maPhieu;
                }

                if (TuNgay > DenNgay)
                {
                    MessageBox.Show("Từ ngày không được lớn hơn đến ngày!");
                    return;
                }

                DanhSachPhieuNhap = new ObservableCollection<PhieuNhapKho>(
                    _repository.SearchReceipts(TuNgay, DenNgay, TuKhoaNhaCungCap, maPhieuNhap)
                );

                Message = SoKetQuaText;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tìm kiếm phiếu nhập: " + ex.Message);
            }
        }

        // =========================
        // LOAD CHI TIẾT PHIẾU NHẬP ĐÃ LƯU
        // =========================

        private void LoadChiTietPhieuNhap()
        {
            try
            {
                if (SelectedPhieuNhap == null)
                {
                    DanhSachChiTietPhieuNhap = new ObservableCollection<ChiTietNhapKho>();
                    return;
                }

                DanhSachChiTietPhieuNhap = new ObservableCollection<ChiTietNhapKho>(
                    _repository.GetReceiptDetails(SelectedPhieuNhap.MaPhieuNhap));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết phiếu nhập: " + ex.Message);
            }
        }
    }
}