using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using QLKhachSan.Data;
using QLKhachSan.Models;

namespace QLKhachSan.ViewModels
{
    public class ServiceViewModel : ViewModelBase
    {
        private ServiceRepository _repo = new ServiceRepository();

        private ObservableCollection<DichVu> _danhSachDV;
        private ObservableCollection<HangHoa> _danhSachHangHoaLuaChon;

        private DichVu _selectedService;

        private string _tenDV;
        private string _donGia;
        private int _selectedMaHang;
        private string _soLuongTieuHao;

        public ObservableCollection<DichVu> DanhSachDV
        {
            get { return _danhSachDV; }
            set
            {
                _danhSachDV = value;
                OnPropertyChanged(nameof(DanhSachDV));
            }
        }

        public ObservableCollection<HangHoa> DanhSachHangHoaLuaChon
        {
            get { return _danhSachHangHoaLuaChon; }
            set
            {
                _danhSachHangHoaLuaChon = value;
                OnPropertyChanged(nameof(DanhSachHangHoaLuaChon));
            }
        }

        public DichVu SelectedService
        {
            get { return _selectedService; }
            set
            {
                _selectedService = value;
                OnPropertyChanged(nameof(SelectedService));

                if (_selectedService != null)
                {
                    TenDV = _selectedService.TenDV;
                    DonGia = _selectedService.DonGia.ToString("G29");

                    if (_selectedService.MaHang == null)
                        SelectedMaHang = 0;
                    else
                        SelectedMaHang = _selectedService.MaHang.Value;

                    SoLuongTieuHao = _selectedService.SoLuongTieuHao.ToString();
                }
            }
        }

        public string TenDV
        {
            get { return _tenDV; }
            set
            {
                _tenDV = value;
                OnPropertyChanged(nameof(TenDV));
            }
        }

        public string DonGia
        {
            get { return _donGia; }
            set
            {
                _donGia = value;
                OnPropertyChanged(nameof(DonGia));
            }
        }

        public int SelectedMaHang
        {
            get { return _selectedMaHang; }
            set
            {
                _selectedMaHang = value;
                OnPropertyChanged(nameof(SelectedMaHang));
                OnPropertyChanged(nameof(LoaiDichVuDangChonText));
            }
        }

        public string SoLuongTieuHao
        {
            get { return _soLuongTieuHao; }
            set
            {
                _soLuongTieuHao = value;
                OnPropertyChanged(nameof(SoLuongTieuHao));
            }
        }

        public string LoaiDichVuDangChonText
        {
            get
            {
                if (SelectedMaHang == 0)
                    return "Loại dịch vụ: Không dùng kho";

                return "Loại dịch vụ: Có dùng kho";
            }
        }

        public ICommand AddCommand { get; private set; }
        public ICommand UpdateCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

        public ServiceViewModel()
        {
            LoadData();

            AddCommand = new RelayCommand(ExecuteAdd, CanExecuteAdd);
            UpdateCommand = new RelayCommand(ExecuteUpdate, CanExecuteUpdate);
            DeleteCommand = new RelayCommand(ExecuteDelete, CanExecuteDelete);

            SoLuongTieuHao = "1";
        }

        private void LoadData()
        {
            DanhSachDV = new ObservableCollection<DichVu>(_repo.GetAllServices());

            DanhSachHangHoaLuaChon = new ObservableCollection<HangHoa>();

            // Dòng giả dùng cho ComboBox, nghĩa là dịch vụ không dùng kho
            DanhSachHangHoaLuaChon.Add(new HangHoa
            {
                MaHang = 0,
                TenHang = "Không dùng kho"
            });

            foreach (HangHoa hangHoa in _repo.GetAllHangHoa())
            {
                DanhSachHangHoaLuaChon.Add(hangHoa);
            }
        }

        private bool CanExecuteAdd()
        {
            return !string.IsNullOrWhiteSpace(TenDV) &&
                   !string.IsNullOrWhiteSpace(DonGia);
        }

        private bool CanExecuteUpdate()
        {
            return SelectedService != null && CanExecuteAdd();
        }

        private bool CanExecuteDelete()
        {
            return SelectedService != null;
        }

        private bool ValidateDichVuInput(out decimal giaChuan, out int soLuongTieuHaoChuan)
        {
            giaChuan = 0;
            soLuongTieuHaoChuan = 1;

            if (!decimal.TryParse(DonGia, out giaChuan) || giaChuan < 0)
            {
                MessageBox.Show("Đơn giá dịch vụ không hợp lệ. Vui lòng chỉ nhập số.",
                                "Định dạng không hợp lệ",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return false;
            }

            if (SelectedMaHang != 0)
            {
                if (!int.TryParse(SoLuongTieuHao, out soLuongTieuHaoChuan) ||
                    soLuongTieuHaoChuan <= 0)
                {
                    MessageBox.Show("Số lượng tiêu hao phải là số nguyên lớn hơn 0.",
                                    "Định dạng không hợp lệ",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return false;
                }
            }
            else
            {
                soLuongTieuHaoChuan = 1;
            }

            return true;
        }

        private void ExecuteAdd()
        {
            if (!ValidateDichVuInput(out decimal giaChuan, out int soLuongTieuHaoChuan))
                return;

            try
            {
                // Kiểm tra có dịch vụ cùng tên đã từng bị ngừng bán (xóa mềm) không
                var deleted = _repo.GetDeletedServiceByName(TenDV);
                if (deleted != null)
                {
                    var result = MessageBox.Show(
                        "Dịch vụ này từng bị ngừng bán. Bạn có muốn khôi phục lại (Đồng thời reset tồn kho cũ về 0) không?",
                        "Xác nhận khôi phục",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Cập nhật thông tin mới vào bản ghi đã bị ngừng bán rồi gọi restore+reset kho
                        deleted.DonGia = giaChuan;

                        if (SelectedMaHang == 0)
                            deleted.MaHang = null;
                        else
                            deleted.MaHang = SelectedMaHang;

                        deleted.SoLuongTieuHao = soLuongTieuHaoChuan;

                        if (_repo.RestoreAndResetInventory(deleted))
                        {
                            MessageBox.Show("Dịch vụ đã được khôi phục thành công và tồn kho liên kết đã được reset về 0.",
                                            "Thông báo",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Information);

                            LoadData();
                            ClearForm();
                        }

                        return;
                    }
                    else
                    {
                        // Người dùng không muốn khôi phục -> dừng thao tác thêm
                        return;
                    }
                }

                // Nếu không có bản ghi đã xóa mềm trùng tên -> thêm mới
                DichVu newDV = new DichVu
                {
                    TenDV = TenDV,
                    DonGia = giaChuan,
                    SoLuongTieuHao = soLuongTieuHaoChuan
                };

                if (SelectedMaHang == 0)
                    newDV.MaHang = null;
                else
                    newDV.MaHang = SelectedMaHang;

                if (_repo.AddService(newDV))
                {
                    MessageBox.Show("Thêm dịch vụ mới thành công.",
                                    "Thông báo",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    LoadData();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi CSDL: " + ex.Message);
            }
        }

        private void ExecuteUpdate()
        {
            if (!ValidateDichVuInput(out decimal giaChuan, out int soLuongTieuHaoChuan))
                return;

            try
            {
                SelectedService.TenDV = TenDV;
                SelectedService.DonGia = giaChuan;
                SelectedService.SoLuongTieuHao = soLuongTieuHaoChuan;

                if (SelectedMaHang == 0)
                    SelectedService.MaHang = null;
                else
                    SelectedService.MaHang = SelectedMaHang;

                if (_repo.UpdateService(SelectedService))
                {
                    MessageBox.Show("Cập nhật dịch vụ thành công.",
                                    "Thông báo",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    LoadData();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi CSDL: " + ex.Message);
            }
        }

        private void ExecuteDelete()
        {
            if (SelectedService == null)
                return;

            MessageBoxResult result = MessageBox.Show(
                "Xóa dịch vụ '" + SelectedService.TenDV + "' khỏi hệ thống?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                if (_repo.DeleteService(SelectedService.MaDV))
                {
                    MessageBox.Show("Đã xóa (ngừng bán) dịch vụ thành công!",
                                    "Thông báo",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    LoadData();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                                "Không thể xóa dịch vụ",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            TenDV = "";
            DonGia = "";
            SelectedMaHang = 0;
            SoLuongTieuHao = "1";
            SelectedService = null;
        }
    }
}