using System.Windows;
using QLKhachsan.ViewModels;

namespace QLKhachsan.views
{
    public partial class LoginWindow : Window
    {
        private readonly AuthViewModel _viewModel;

        public LoginWindow()
        {
            InitializeComponent();

            _viewModel = new AuthViewModel();
            _viewModel.LoginSucceeded += ViewModel_LoginSucceeded;
            DataContext = _viewModel;
        }

        private void LoginPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.LoginPassword = LoginPasswordBox.Password;
        }

        private void RegisterPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.RegisterPassword = RegisterPasswordBox.Password;
        }

        private void ViewModel_LoginSucceeded(object sender, System.EventArgs e)
        {
            var dashboard = new Window1();
            dashboard.Show();
            Close();
        }
    }
}
