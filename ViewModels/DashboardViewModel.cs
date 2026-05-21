using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using QLKhachsan.Data;
using QLKhachsan.Models;

namespace QLKhachsan.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly HotelRepository _repository;
        private Room _selectedRoom;
        private string _errorMessage;

        public DashboardViewModel()
            : this(new HotelRepository())
        {
        }

        public DashboardViewModel(HotelRepository repository)
        {
            _repository = repository;
            Rooms = new ObservableCollection<Room>();
            RecentBookings = new ObservableCollection<Booking>();
            RefreshCommand = new RelayCommand(LoadData);

            LoadData();
        }

        public ObservableCollection<Room> Rooms { get; private set; }

        public ObservableCollection<Booking> RecentBookings { get; private set; }

        public ICommand RefreshCommand { get; private set; }

        public Room SelectedRoom
        {
            get { return _selectedRoom; }
            set { SetProperty(ref _selectedRoom, value); }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                if (SetProperty(ref _errorMessage, value))
                {
                    OnPropertyChanged("ErrorVisibility");
                }
            }
        }

        public int AvailableRoomCount
        {
            get { return Rooms.Count(room => room.IsAvailable); }
        }

        public int OccupiedRoomCount
        {
            get { return Rooms.Count(room => room.IsOccupied); }
        }

        public int WaitingCheckInCount
        {
            get { return Rooms.Count(room => room.IsWaitingCheckIn); }
        }

        public string OccupancyRateText
        {
            get
            {
                if (Rooms.Count == 0)
                {
                    return "Ty le lap day 0%";
                }

                var rate = (int)Math.Round(OccupiedRoomCount * 100.0 / Rooms.Count);
                return "Ty le lap day " + rate + "%";
            }
        }

        public string DailyRevenueText
        {
            get
            {
                var revenue = Rooms.Where(room => room.IsOccupied).Sum(room => room.PricePerNight);
                return (revenue / 1000000m).ToString("0.#") + " tr";
            }
        }

        public Visibility ErrorVisibility
        {
            get { return string.IsNullOrWhiteSpace(ErrorMessage) ? Visibility.Collapsed : Visibility.Visible; }
        }

        private void LoadData()
        {
            try
            {
                ErrorMessage = string.Empty;
                ReplaceRooms(_repository.GetRooms());
                ReplaceBookings(_repository.GetRecentBookings());
            }
            catch (Exception ex)
            {
                ErrorMessage = "Khong the tai du lieu: " + ex.Message;
            }
        }

        private void ReplaceRooms(System.Collections.Generic.IEnumerable<Room> rooms)
        {
            Rooms.Clear();

            foreach (var room in rooms)
            {
                Rooms.Add(room);
            }

            SelectedRoom = Rooms.FirstOrDefault();
            OnDashboardChanged();
        }

        private void ReplaceBookings(System.Collections.Generic.IEnumerable<Booking> bookings)
        {
            RecentBookings.Clear();

            foreach (var booking in bookings)
            {
                RecentBookings.Add(booking);
            }
        }

        private void OnDashboardChanged()
        {
            OnPropertyChanged("AvailableRoomCount");
            OnPropertyChanged("OccupiedRoomCount");
            OnPropertyChanged("WaitingCheckInCount");
            OnPropertyChanged("OccupancyRateText");
            OnPropertyChanged("DailyRevenueText");
        }
    }
}
