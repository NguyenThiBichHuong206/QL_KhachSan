using System;
using System.IO;
using System.Windows;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.Windows.Forms;
using System.Data.SqlClient;
namespace QLKhachSan.views
{
    public partial class HoaDonReportWindow : Window
    {
        private int _maHD;
        private ReportDocument _reportDocument;
        private CrystalReportViewer _crystalReportViewer;

        public HoaDonReportWindow(int maHD)
        {
            InitializeComponent();

            _maHD = maHD;

            LoadReport();
        }
        private bool KiemTraHoaDonCoDuLieu()
        {
            string connectionString = "Data Source=LAPTOP-NNM4EHHN;Initial Catalog=QuanLyKhachSan;User ID=sa;Password=123";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = @"
            SELECT COUNT(*)
            FROM v_HoaDonReport
            WHERE MaHD = @MaHD";

                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@MaHD", _maHD);

                int soDong = Convert.ToInt32(command.ExecuteScalar());

                return soDong > 0;
            }
        }
        private void LoadReport()
        {
            try
            {
                // Kiểm tra hóa đơn có trong view report chưa
                if (!KiemTraHoaDonCoDuLieu())
                {
                    MessageBox.Show(
                        "Không tìm thấy dữ liệu hóa đơn trong v_HoaDonReport.\n" +
                        "Mã hóa đơn: " + _maHD + "\n\n" +
                        "Vui lòng kiểm tra hóa đơn đã được lưu chi tiết và đã thanh toán chưa.");

                    return;
                }

                _reportDocument = new ReportDocument();

                // Đường dẫn file report khi chạy chương trình
                string reportPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Reports",
                    "HoaDonReport.rpt");

                if (!File.Exists(reportPath))
                {
                    MessageBox.Show("Không tìm thấy file report: " + reportPath);
                    return;
                }

                _reportDocument.Load(reportPath);

                // Thông tin kết nối SQL Server
                ConnectionInfo connectionInfo = new ConnectionInfo();
                connectionInfo.ServerName = "LAPTOP-NNM4EHHN";
                connectionInfo.DatabaseName = "QuanLyKhachSan";
                connectionInfo.UserID = "sa";
                connectionInfo.Password = "123";

                foreach (Table table in _reportDocument.Database.Tables)
                {
                    TableLogOnInfo logOnInfo = table.LogOnInfo;
                    logOnInfo.ConnectionInfo = connectionInfo;
                    table.ApplyLogOnInfo(logOnInfo);
                }

                // Đảm bảo report không dùng dữ liệu cũ đã lưu trong file rpt
                _reportDocument.Refresh();

                // Lọc đúng hóa đơn đang chọn
                _reportDocument.RecordSelectionFormula = "{v_HoaDonReport.MaHD} = " + _maHD;

                // Tạo CrystalReportViewer WinForms rồi nhúng vào WPF
                _crystalReportViewer = new CrystalReportViewer();
                _crystalReportViewer.Dock = System.Windows.Forms.DockStyle.Fill;
                _crystalReportViewer.ReportSource = _reportDocument;
                _crystalReportViewer.ToolPanelView = ToolPanelViewType.None;
                _crystalReportViewer.RefreshReport();

                windowsFormsHost.Child = _crystalReportViewer;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi mở hóa đơn Crystal Report: " + ex.Message);
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            if (_crystalReportViewer != null)
            {
                _crystalReportViewer.Dispose();
            }

            if (_reportDocument != null)
            {
                _reportDocument.Close();
                _reportDocument.Dispose();
            }

            base.OnClosed(e);
        }
    }
}