using System.Globalization;
using System.Threading;
using System.Windows;

namespace QLKhachSan
{
    public partial class App : Application
    {
        public App()
        {
            CultureInfo culture = (CultureInfo)CultureInfo.GetCultureInfo("vi-VN").Clone();
            culture.DateTimeFormat.ShortDatePattern = "d/M/yyyy";
            culture.DateTimeFormat.LongDatePattern = "d/M/yyyy";

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}