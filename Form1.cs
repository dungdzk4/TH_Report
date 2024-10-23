using BTL;
using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TH_Repost
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            CrystalReport1 report = new CrystalReport1();
            report.SetDataSource(DBConnection.Instance.SelectDB("NHANVIEN"));
            crystalReportViewer1.ReportSource = report;
            crystalReportViewer1.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            CrystalReport1 report = new CrystalReport1();
            report.SetDataSource(DBConnection.Instance.SelectDB("NHANVIEN", $"MONTH(dNgaySinh)={textBox1.Text}"));
            crystalReportViewer1.ReportSource = report;
            crystalReportViewer1.Refresh();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CrystalReport1 report = new CrystalReport1();
            report.SetDataSource(DBConnection.Instance.SelectDB("NHANVIEN", $"fLuong > {textBox2.Text}"));
            crystalReportViewer1.ReportSource = report;
            crystalReportViewer1.Refresh();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CrystalReport1 report = new CrystalReport1();
            report.SetDataSource(DBConnection.Instance.SelectDB("NHANVIEN", $" (sGioitinh = 'Nam' AND DATEDIFF(year, dNgaySinh,'{dateTimePicker1.Value.ToString("MM/dd/yyyy")}') > 60) OR (sGioitinh='Nu' AND  DATEDIFF(year, dNgaySinh,'{dateTimePicker1.Value.ToString("MM/dd/yyyy")}') > 55)"));
            crystalReportViewer1.ReportSource = report;
            crystalReportViewer1.Refresh();
        }
    }
}
