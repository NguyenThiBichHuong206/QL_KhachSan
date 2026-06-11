using System;
using System.Windows.Input;
using QLKhachSan.Data;
using QLKhachSan.Models;

namespace QLKhachSan.ViewModels
{
    public class AuthViewModel : ViewModelBase
    {
        private readonly AuthRepository _repository;

        private string _loginUsername;
        private string _loginPassword;
        private string _registerUsername;
        private string _registerPassword;
        private string _message;
        private bool _isBusy;

        public AuthViewModel() : this(new AuthRepository()) { }

        public AuthViewModel(AuthRepository repository)
        {
            _repository = repository;
            LoginCommand = new RelayCommand(Login, CanLogin);
            RegisterCommand = new RelayCommand(Register, CanRegister);
        }

        public event EventHandler LoginSucceeded;

        public ICommand LoginCommand { get; private set; }
        public ICommand RegisterCommand { get; private set; }

        public UserAccount CurrentUser { get; private set; }

        public string LoginUsername
        {
            get { return _loginUsername; }
            set { if (SetProperty(ref _loginUsername, value)) CommandManager.InvalidateRequerySuggested(); }
        }

        public string LoginPassword
        {
            get { return _loginPassword; }
            set { if (SetProperty(ref _loginPassword, value)) CommandManager.InvalidateRequerySuggested(); }
        }

        public string RegisterUsername
        {
            get { return _registerUsername; }
            set { if (SetProperty(ref _registerUsername, value)) CommandManager.InvalidateRequerySuggested(); }
        }

        public string RegisterPassword
        {
            get { return _registerPassword; }
            set { if (SetProperty(ref _registerPassword, value)) CommandManager.InvalidateRequerySuggested(); }
        }

        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        private bool CanLogin() => !IsBusy && !string.IsNullOrWhiteSpace(LoginUsername) && !string.IsNullOrWhiteSpace(LoginPassword);
        private bool CanRegister() => !IsBusy && !string.IsNullOrWhiteSpace(RegisterUsername) && !string.IsNullOrWhiteSpace(RegisterPassword);

        private void Login()
        {
            try
            {
                IsBusy = true;
                if (_repository.Login(LoginUsername, LoginPassword, out UserAccount account, out string message))
                {
                    CurrentUser = account;
                    Message = message;
                    LoginSucceeded?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    Message = message;
                }
            }
            catch (Exception ex)
            {
                Message = "Có lỗi kết nối CSDL: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void Register()
        {
            try
            {
                IsBusy = true;
                if (_repository.Register(RegisterUsername, RegisterPassword, out string message))
                {
                    LoginUsername = RegisterUsername;
                    LoginPassword = RegisterPassword;
                }
                Message = message;
            }
            catch (Exception ex)
            {
                Message = "Có lỗi kết nối CSDL: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}