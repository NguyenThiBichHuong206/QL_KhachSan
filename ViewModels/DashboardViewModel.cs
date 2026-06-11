using System;
using System.Windows;
using QLKhachSan.Data;

namespace QLKhachSan.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private DashboardRepository _repo = new DashboardRepository();

        private int _tongSoPhong;
        private int _soPhongTrong;
        private int _soPhongDangThue;
        private int _tongSoKhachHang;
        private int _tongSoDichVu;
        private int _tongSoHoaDon;
        private decimal _tongDoanhThu;
        private string _thoiGianCapNhat;

        public int TongSoPhong
        {
            get => _tongSoPhong;
            set
            {
                _tongSoPhong = value;
                OnPropertyChanged(nameof(TongSoPhong));
            }
        }

        public int SoPhongTrong
        {
            get => _soPhongTrong;
            set
            {
                _soPhongTrong = value;
                OnPropertyChanged(nameof(SoPhongTrong));
            }
        }

        public int SoPhongDangThue
        {
            get => _soPhongDangThue;
            set
            {
                _soPhongDangThue = value;
                OnPropertyChanged(nameof(SoPhongDangThue));
            }
        }

        public int TongSoKhachHang
        {
            get => _tongSoKhachHang;
            set
            {
                _tongSoKhachHang = value;
                OnPropertyChanged(nameof(TongSoKhachHang));
            }
        }

        public int TongSoDichVu
        {
            get => _tongSoDichVu;
            set
            {
                _tongSoDichVu = value;
                OnPropertyChanged(nameof(TongSoDichVu));
            }
        }

        public int TongSoHoaDon
        {
            get => _tongSoHoaDon;
            set
            {
                _tongSoHoaDon = value;
                OnPropertyChanged(nameof(TongSoHoaDon));
            }
        }

        public decimal TongDoanhThu
        {
            get => _tongDoanhThu;
            set
            {
                _tongDoanhThu = value;
                OnPropertyChanged(nameof(TongDoanhThu));
                OnPropertyChanged(nameof(TongDoanhThuText));
            }
        }

        public string TongDoanhThuText
        {
            get
            {
                return TongDoanhThu.ToString("N0") + " VNĐ";
            }
        }

        public string ThoiGianCapNhat
        {
            get => _thoiGianCapNhat;
            set
            {
                _thoiGianCapNhat = value;
                OnPropertyChanged(nameof(ThoiGianCapNhat));
            }
        }

        public RelayCommand TaiLaiCommand { get; set; }

        public DashboardViewModel()
        {
            TaiLaiCommand = new RelayCommand(LoadData);
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                TongSoPhong = _repo.GetTongSoPhong();
                SoPhongTrong = _repo.GetSoPhongTrong();
                SoPhongDangThue = _repo.GetSoPhongDangThue();
                TongSoKhachHang = _repo.GetTongSoKhachHang();
                TongSoDichVu = _repo.GetTongSoDichVu();
                TongSoHoaDon = _repo.GetTongSoHoaDon();
                TongDoanhThu = _repo.GetTongDoanhThu();

                ThoiGianCapNhat = "Cập nhật lúc: " + DateTime.Now.ToString("HH:mm dd/MM/yyyy");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu tổng quan: " + ex.Message);
            }
        }
    }
}