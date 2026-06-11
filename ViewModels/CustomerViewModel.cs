using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using QLKhachSan.Data;
using QLKhachSan.Models;

namespace QLKhachSan.ViewModels
{
    public class CustomerViewModel : ViewModelBase
    {
        private CustomerRepository _customerRepo = new CustomerRepository();

        public ObservableCollection<KhachHang> DanhSachKhachHang { get; set; }

        private KhachHang _selectedCustomer;
        public KhachHang SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged(nameof(SelectedCustomer));
                if (_selectedCustomer != null)
                {
                    HoTen = _selectedCustomer.HoTen;
                    SDT = _selectedCustomer.SDT;
                    CCCD = _selectedCustomer.CCCD;
                }
            }
        }

        private string _hoTen;
        public string HoTen
        {
            get => _hoTen;
            set { _hoTen = value; OnPropertyChanged(nameof(HoTen)); }
        }

        private string _sdt;
        public string SDT
        {
            get => _sdt;
            set { _sdt = value; OnPropertyChanged(nameof(SDT)); }
        }

        private string _cccd;
        public string CCCD
        {
            get => _cccd;
            set { _cccd = value; OnPropertyChanged(nameof(CCCD)); }
        }

        public ICommand AddCommand { get; set; }
        public ICommand UpdateCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        // RBAC: control delete button visibility
        private Visibility _deleteButtonVisibility = Visibility.Visible;
        public Visibility DeleteButtonVisibility
        {
            get => _deleteButtonVisibility;
            set { _deleteButtonVisibility = value; OnPropertyChanged(nameof(DeleteButtonVisibility)); }
        }

        // Constructor now accepts current user to determine permissions
        public CustomerViewModel(UserAccount currentUser)
        {
            // Default visibility
            if (currentUser != null && currentUser.RoleValue == 1)
            {
                DeleteButtonVisibility = Visibility.Visible;
            }
            else
            {
                // For non-admin (e.g., Lễ tân) hide the delete control
                DeleteButtonVisibility = Visibility.Collapsed;
            }

            LoadData();
            AddCommand = new RelayCommand(ExecuteAdd, CanExecuteAdd);
            UpdateCommand = new RelayCommand(ExecuteUpdate, CanExecuteUpdate);
            DeleteCommand = new RelayCommand(ExecuteDelete, CanExecuteDelete);
        }

        private void LoadData()
        {
            DanhSachKhachHang = new ObservableCollection<KhachHang>(_customerRepo.GetAllCustomers());
            OnPropertyChanged(nameof(DanhSachKhachHang));
        }

        private bool CanExecuteAdd() => !string.IsNullOrWhiteSpace(HoTen) && !string.IsNullOrWhiteSpace(SDT) && !string.IsNullOrWhiteSpace(CCCD);
        private bool CanExecuteUpdate() => SelectedCustomer != null && CanExecuteAdd();
        private bool CanExecuteDelete() => SelectedCustomer != null;

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(SDT) || !Regex.IsMatch(SDT, @"^(0|\+84)\d{9}$"))
            {
                MessageBox.Show("Số điện thoại không đúng định dạng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(CCCD) || !Regex.IsMatch(CCCD, @"^\d{12}$"))
            {
                MessageBox.Show("Số CCCD / CMND phải gồm 12 chữ số!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void ExecuteAdd()
        {
            if (!ValidateInput()) return;
            try
            {
                // Kiểm tra xem có khách hàng đã bị ẩn với cùng CCCD không
                var deleted = _customerRepo.GetDeletedCustomerByCCCD(this.CCCD);
                if (deleted != null)
                {
                    var result = MessageBox.Show("Khách hàng có CCCD này từng tồn tại nhưng đã bị ẩn. Bạn có muốn khôi phục và cập nhật thông tin mới không?",
                                                 "Khôi phục khách hàng", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        var khMoi = new KhachHang { HoTen = this.HoTen, SDT = this.SDT, CCCD = this.CCCD };
                        if (_customerRepo.RestoreCustomer(khMoi))
                        {
                            MessageBox.Show("Khôi phục khách hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadData(); ResetForm();
                        }
                        else
                        {
                            MessageBox.Show("Không thể khôi phục khách hàng. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        return;
                    }
                }

                // Nếu không có bản ghi bị ẩn, thực hiện thêm mới như bình thường
                var newCustomer = new KhachHang { HoTen = this.HoTen, SDT = this.SDT, CCCD = this.CCCD };
                if (_customerRepo.AddCustomer(newCustomer))
                {
                    MessageBox.Show("Thêm khách hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData(); ResetForm();
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: Thông tin bị trùng hoặc CSDL từ chối!\n" + ex.Message); }
        }

        private void ExecuteUpdate()
        {
            if (!ValidateInput()) return;
            try
            {
                SelectedCustomer.HoTen = this.HoTen;
                SelectedCustomer.SDT = this.SDT;
                SelectedCustomer.CCCD = this.CCCD;

                if (_customerRepo.UpdateCustomer(SelectedCustomer))
                {
                    MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData(); ResetForm();
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void ExecuteDelete()
        {
            if (SelectedCustomer == null) return;

            if (MessageBox.Show($"Xóa (Ẩn) khách hàng {SelectedCustomer.HoTen}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    if (_customerRepo.DeleteCustomer(SelectedCustomer.MaKH))
                    {
                        MessageBox.Show("Đã xóa (ẩn) khách hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData(); ResetForm();
                    }
                    else
                    {
                        MessageBox.Show("Không thể xóa (ẩn) khách hàng. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ResetForm() { HoTen = ""; SDT = ""; CCCD = ""; SelectedCustomer = null; }
    }
}