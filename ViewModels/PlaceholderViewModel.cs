using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using QLKhachSan.ViewModels;

namespace QLKhachSan.ViewModels
{
    public class PlaceholderViewModel : ViewModelBase
    {
        public PlaceholderViewModel(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }
    }
}