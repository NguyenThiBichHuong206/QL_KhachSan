using System.Windows;
using System.Windows.Input;
using QLKhachSan.Models;
using QLKhachSan.views;

namespace QLKhachSan.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private object _currentViewModel;
        private string _pageTitle;
        private string _pageDescription;
        private UserAccount _currentUser;
        private Visibility _adminMenuVisibility = Visibility.Visible;

        public MainViewModel(UserAccount currentUser)
        {
            // assign user first so commands / view models that need it can use it
            CurrentUser = currentUser;

            // Initialize commands
            ShowDashboardCommand = new RelayCommand(ShowDashboard);
            ShowRoomCommand = new RelayCommand(ShowRoom);
            ShowBookingCommand = new RelayCommand(ShowBooking);
            ShowCustomerCommand = new RelayCommand(ShowCustomer);
            ShowInvoiceCommand = new RelayCommand(ShowInvoice);
            ShowServiceCommand = new RelayCommand(ShowService);
            ShowInventoryCommand = new RelayCommand(ShowInventory);
            ShowReportCommand = new RelayCommand(ShowReport);

            LogoutCommand = new RelayCommand(ExecuteLogout);

            // Role-based menu visibility and default view
            // Treat RoleValue == 1 as Admin, otherwise as Lễ tân (receptionist)
            if (CurrentUser != null && CurrentUser.RoleValue == 1)
            {
                AdminMenuVisibility = Visibility.Visible;
                // Admin default view is Dashboard
                ShowDashboard();
            }
            else
            {
                AdminMenuVisibility = Visibility.Collapsed;
                // Receptionist default view should be Booking
                ShowBooking();
            }
        }

        public UserAccount CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
                OnPropertyChanged(nameof(CurrentUserName));
                OnPropertyChanged(nameof(CurrentUserRole));
                OnPropertyChanged(nameof(GreetingText));
            }
        }

        // New property for binding admin-only menu visibility
        public Visibility AdminMenuVisibility
        {
            get => _adminMenuVisibility;
            set
            {
                if (_adminMenuVisibility != value)
                {
                    _adminMenuVisibility = value;
                    OnPropertyChanged(nameof(AdminMenuVisibility));
                }
            }
        }

        public string CurrentUserName
        {
            get
            {
                if (CurrentUser != null)
                    return CurrentUser.FullName;

                return "Nhân viên";
            }
        }

        public string CurrentUserRole
        {
            get
            {
                if (CurrentUser != null)
                    return CurrentUser.RoleName;

                return "Lễ tân";
            }
        }

        public string GreetingText
        {
            get
            {
                if (CurrentUser == null)
                    return "Xin chào!";

                return "Xin chào " + CurrentUser.FullName + "!";
            }
        }

        public object CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }

        public string PageTitle
        {
            get => _pageTitle;
            set
            {
                _pageTitle = value;
                OnPropertyChanged(nameof(PageTitle));
            }
        }

        public string PageDescription
        {
            get => _pageDescription;
            set
            {
                _pageDescription = value;
                OnPropertyChanged(nameof(PageDescription));
            }
        }

        public ICommand ShowDashboardCommand { get; private set; }
        public ICommand ShowRoomCommand { get; private set; }
        public ICommand ShowBookingCommand { get; private set; }
        public ICommand ShowCustomerCommand { get; private set; }
        public ICommand ShowInvoiceCommand { get; private set; }
        public ICommand ShowServiceCommand { get; private set; }
        public ICommand ShowInventoryCommand { get; private set; }
        public ICommand ShowReportCommand { get; private set; }

        // Added logout command
        public ICommand LogoutCommand { get; private set; }

        private void ShowDashboard()
        {
            PageTitle = "Tổng quan vận hành";
            PageDescription = "Hệ thống hỗ trợ theo dõi phòng và công suất hoạt động thời gian thực.";
            CurrentViewModel = new DashboardViewModel();
        }

        private void ShowRoom()
        {
            PageTitle = "Quản lý phòng";
            PageDescription = "Quản lý danh sách phòng, loại phòng, giá phòng và trạng thái sử dụng.";
            CurrentViewModel = new RoomViewModel();
        }

        private void ShowBooking()
        {
            PageTitle = "Đặt phòng";
            PageDescription = "Tạo phiếu đặt phòng, nhận phòng và theo dõi lịch lưu trú của khách.";
            CurrentViewModel = new BookingViewModel(CurrentUser);
        }

        private void ShowCustomer()
        {
            PageTitle = "Khách hàng";
            PageDescription = "Quản lý hồ sơ, CCCD và thông tin liên lạc của khách hàng.";
            CurrentViewModel = new CustomerViewModel(CurrentUser);
        }

        private void ShowInvoice()
        {
            PageTitle = "Trả phòng & Hóa đơn";
            PageDescription = "Gọi thêm dịch vụ, in hóa đơn và tiến hành check-out cho khách.";
            CurrentViewModel = new CheckoutViewModel();
        }

        private void ShowService()
        {
            PageTitle = "Dịch vụ";
            PageDescription = "Quản lý các dịch vụ khách sạn, đơn giá và hàng hóa liên quan trong kho.";
            CurrentViewModel = new ServiceViewModel();
        }

        private void ShowInventory()
        {
            PageTitle = "Nhập kho";
            PageDescription = "Lập phiếu nhập kho, nhập chi tiết hàng hóa và cập nhật tồn kho.";
            CurrentViewModel = new InventoryViewModel();
        }

        private void ShowReport()
        {
            PageTitle = "Báo cáo tài chính";
            PageDescription = "Quản lý dữ liệu doanh thu chi tiết.";

            string nguoiIn = "Người dùng";

            if (CurrentUser != null)
            {
                string tenNguoiDung = CurrentUser.FullName;
                string vaiTro = CurrentUser.RoleName;

                if (!string.IsNullOrWhiteSpace(tenNguoiDung) &&
                    !string.IsNullOrWhiteSpace(vaiTro))
                {
                    nguoiIn = tenNguoiDung + " (" + vaiTro + ")";
                }
                else if (!string.IsNullOrWhiteSpace(tenNguoiDung))
                {
                    nguoiIn = tenNguoiDung;
                }
                else if (!string.IsNullOrWhiteSpace(vaiTro))
                {
                    nguoiIn = vaiTro;
                }
            }

            CurrentViewModel = new ReportViewModel(nguoiIn);
        }

        // ExecuteLogout: shows confirmation, resets current user, opens LoginWindow, closes MainWindow
        private void ExecuteLogout()
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất khỏi hệ thống không?",
                                         "Xác nhận đăng xuất",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Clear current session user
                CurrentUser = null;

                // Show login window
                var loginWindow = new LoginWindow();
                loginWindow.Show();

                // Close the current MainWindow safely
                foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                {
                    if (window is MainWindow)
                    {
                        window.Close();
                        break;
                    }
                }
            }
        }
    }
}