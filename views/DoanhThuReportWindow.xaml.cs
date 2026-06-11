using System;
using System.IO;
using System.Windows;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Windows.Forms;

namespace QLKhachSan.views
{
    public partial class DoanhThuReportWindow : Window
    {
        private readonly DateTime _tuNgay;
        private readonly DateTime _denNgay;
        private readonly string _nguoiIn;
        private ReportDocument _reportDocument;

        public DoanhThuReportWindow(DateTime tuNgay, DateTime denNgay, string nguoiIn)
        {
            InitializeComponent();

            _tuNgay = tuNgay;
            _denNgay = denNgay;
            _nguoiIn = nguoiIn;

            LoadReport();
        }

        private void LoadReport()
        {
            try
            {
                _reportDocument = new ReportDocument();

                string reportPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Reports",
                    "DoanhThuNangCaoReport.rpt");

                if (!File.Exists(reportPath))
                {
                    MessageBox.Show("Không tìm thấy file báo cáo: " + reportPath);
                    return;
                }

                _reportDocument.Load(reportPath);

                _reportDocument.SetParameterValue("TuNgay", _tuNgay.Date);
                _reportDocument.SetParameterValue("DenNgay", _denNgay.Date);

                // Truyền người in vào Crystal Report
                _reportDocument.SetParameterValue("NguoiIn", _nguoiIn);

                // Lọc dữ liệu theo ngày thanh toán
                _reportDocument.RecordSelectionFormula =
    "{v_BaoCaoDoanhThuNangCao.NgayThanhToan} >= Date(" +
    _tuNgay.Year + "," + _tuNgay.Month + "," + _tuNgay.Day + ") AND " +
    "{v_BaoCaoDoanhThuNangCao.NgayThanhToan} <= Date(" +
    _denNgay.Year + "," + _denNgay.Month + "," + _denNgay.Day + ")";

                crystalReportViewer.ReportSource = _reportDocument;
                crystalReportViewer.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải báo cáo doanh thu: " + ex.Message);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_reportDocument != null)
            {
                _reportDocument.Close();
                _reportDocument.Dispose();
            }

            base.OnClosed(e);
        }
    }
}