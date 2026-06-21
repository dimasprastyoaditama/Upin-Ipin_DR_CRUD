using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Data;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public partial class Form3 : Form
    {
        DAL dbLogic = new DAL();
        Praktikum_14 listMahasiswa = new Praktikum_14();

        string prodi;
        DateTime tglmasuk;

        public Form3(string Prodi, DateTime TglMasuk)
        {
            InitializeComponent();
            this.prodi = Prodi;
            this.tglmasuk = TglMasuk;
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            try
            {
                // 1. Ambil data rekap
                DataTable dtMahasiswa = dbLogic.getDataRekap(prodi, tglmasuk);

                // Cadangan: Jika filter kosong, ambil semua data (biar demo video lancar)
                if (dtMahasiswa == null || dtMahasiswa.Rows.Count == 0)
                {
                    dtMahasiswa = dbLogic.GetMhs();
                }

                if (dtMahasiswa != null && dtMahasiswa.Rows.Count > 0)
                {
                    // 2. Set DataSource ke Report
                    listMahasiswa.SetDataSource(dtMahasiswa);

                    // 3. Tembak ke Viewer secara aman (hanya jika viewer tersebut ada)
                    if (this.Controls.ContainsKey("crystalReportViewer1"))
                    {
                        crystalReportViewer1.ReportSource = listMahasiswa;
                        crystalReportViewer1.Refresh();
                    }

                    if (this.Controls.ContainsKey("crystalReportViewer2"))
                    {
                        crystalReportViewer2.ReportSource = listMahasiswa;
                        crystalReportViewer2.Refresh();
                    }
                }
                else
                {
                    MessageBox.Show("Tidak ada data mahasiswa untuk ditampilkan.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat laporan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}