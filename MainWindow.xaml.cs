using System;
using System.Windows;
using QLKhachSan.Models;
using QLKhachSan.ViewModels;

namespace QLKhachSan
{
    public partial class MainWindow : Window
    {
        public MainWindow(UserAccount currentUser)
        {
            InitializeComponent();
            DataContext = new MainViewModel(currentUser);
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(null);
        }
    }
}