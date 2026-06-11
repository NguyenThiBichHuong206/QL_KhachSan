using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using QLKhachSan.Data;
using QLKhachSan.Models;
using QLKhachSan.views;
namespace QLKhachSan.ViewModels
{
    public class ReportViewModel : ViewModelBase
    {
        private ReportRepository _repository = new ReportRepository();

        private DateTime _tuNgay;
        private DateTime _denNgay;
        private ObservableCollection<v_BaoCaoDoanhThuNangCao> _danhSachDoanhThu;
        private ICollectionView _danhSachDoanhThuView;
        private string _message;

        public ReportViewModel()
            : this("Người dùng")
        {
        }

        public ReportViewModel(string nguoiIn)
        {
            if (string.IsNullOrWhiteSpace(nguoiIn))
            {
                NguoiIn = "Người dùng";
            }
            else
            {
                NguoiIn = nguoiIn;
            }

            TuNgay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DenNgay = DateTime.Now;

            DanhSachDoanhThu = new ObservableCollection<v_BaoCaoDoanhThuNangCao>();

            ThongKeCommand = new RelayCommand(ThongKeDoanhThu);
            InBaoCaoCommand = new RelayCommand(InBaoCao, CanInBaoCao);

            ThongKeDoanhThu();
        }
        private string _NguoiIn;

        public string NguoiIn
        {
            get { return _NguoiIn; }
            set
            {
                _NguoiIn = value;
                OnPropertyChanged(nameof(NguoiIn));
            }
        }
        // =========================
        // THUỘC TÍNH LỌC
        // =========================

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

        // =========================
        // DANH SÁCH DOANH THU
        // =========================

        public ObservableCollection<v_BaoCaoDoanhThuNangCao> DanhSachDoanhThu
        {
            get { return _danhSachDoanhThu; }
            set
            {
                _danhSachDoanhThu = value;
                OnPropertyChanged(nameof(DanhSachDoanhThu));

                CapNhatViewGomNhom();

                OnPropertyChanged(nameof(TongSoHoaDon));
                OnPropertyChanged(nameof(TongTienPhong));
                OnPropertyChanged(nameof(TongTienDichVu));
                OnPropertyChanged(nameof(TongDoanhThu));
                OnPropertyChanged(nameof(TongTienPhongText));
                OnPropertyChanged(nameof(TongTienDichVuText));
                OnPropertyChanged(nameof(TongDoanhThuText));
                OnPropertyChanged(nameof(SoKetQuaText));
            }
        }

        public ICollectionView DanhSachDoanhThuView
        {
            get { return _danhSachDoanhThuView; }
            set
            {
                _danhSachDoanhThuView = value;
                OnPropertyChanged(nameof(DanhSachDoanhThuView));
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
        // TỔNG HỢP SỐ LIỆU
        // =========================

        public int TongSoHoaDon
        {
            get
            {
                if (DanhSachDoanhThu == null)
                    return 0;

                return DanhSachDoanhThu.Count;
            }
        }

        public decimal TongTienPhong
        {
            get
            {
                if (DanhSachDoanhThu == null)
                    return 0;

                return DanhSachDoanhThu.Sum(hd => Convert.ToDecimal(hd.TienPhong));
            }
        }

        public decimal TongTienDichVu
        {
            get
            {
                if (DanhSachDoanhThu == null)
                    return 0;

                return DanhSachDoanhThu.Sum(hd => Convert.ToDecimal(hd.TienDichVu));
            }
        }

        public decimal TongDoanhThu
        {
            get
            {
                if (DanhSachDoanhThu == null)
                    return 0;

                return DanhSachDoanhThu.Sum(hd => Convert.ToDecimal(hd.TongTien));
            }
        }

        public string TongTienPhongText
        {
            get { return TongTienPhong.ToString("N0"); }
        }

        public string TongTienDichVuText
        {
            get { return TongTienDichVu.ToString("N0"); }
        }

        public string TongDoanhThuText
        {
            get { return TongDoanhThu.ToString("N0"); }
        }

        public string SoKetQuaText
        {
            get
            {
                return "Tìm thấy " + TongSoHoaDon + " hóa đơn đã thanh toán";
            }
        }


        public ICommand ThongKeCommand { get; private set; }
        public ICommand InBaoCaoCommand { get; private set; }

        // XỬ LÝ THỐNG KÊ
       
        private void ThongKeDoanhThu()
        {
            try
            {
                if (TuNgay.Date > DenNgay.Date)
                {
                    MessageBox.Show("Từ ngày không được lớn hơn đến ngày!");
                    return;
                }

                var ketQua = _repository.GetDoanhThu(TuNgay, DenNgay);

                DanhSachDoanhThu = new ObservableCollection<v_BaoCaoDoanhThuNangCao>(ketQua);

                Message = SoKetQuaText;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thống kê doanh thu: " + ex.Message);
            }
        }
        private void InBaoCao()
        {
            DoanhThuReportWindow window = new DoanhThuReportWindow(TuNgay, DenNgay, NguoiIn);
            window.ShowDialog();
        }

        private bool CanInBaoCao()
        {
            return DanhSachDoanhThu != null && DanhSachDoanhThu.Count > 0;
        }
        // Gom nhóm theo ngày thanh toán để DataGrid hiển thị theo từng ngày
        private void CapNhatViewGomNhom()
        {
            DanhSachDoanhThuView = CollectionViewSource.GetDefaultView(DanhSachDoanhThu);

            if (DanhSachDoanhThuView != null)
            {
                DanhSachDoanhThuView.GroupDescriptions.Clear();
                DanhSachDoanhThuView.GroupDescriptions.Add(
                    new PropertyGroupDescription(nameof(v_BaoCaoDoanhThuNangCao.NgayThanhToan)));
            }


        }
    }
}