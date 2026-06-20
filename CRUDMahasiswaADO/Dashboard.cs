using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CRUDMahasiswaADO
{
    public partial class Dashboard : Form
    {
        // Tambahan kode dari Langkah 8:
        DAL dbLogic = new DAL();
        bool isInitializing = true;
        DataTable dt;
        int button = 0;

        public Dashboard()
        {
            InitializeComponent();

            // tambahan langkah 9:
            dtpTanggalMasuk.MinDate = new DateTime(2000, 1, 1);
            dtpTanggalMasuk.Format = DateTimePickerFormat.Custom;
            dtpTanggalMasuk.CustomFormat = "yyyy";
            dtpTanggalMasuk.ShowUpDown = true;
            dtpTanggalMasuk.MaxDate = DateTime.Now;

            cmbTipe.DropDownStyle = ComboBoxStyle.DropDownList;
            var items = new List<KeyValuePair<string, SeriesChartType>>()
            {
                new KeyValuePair<string, SeriesChartType>("Kolom", SeriesChartType.Column),
                new KeyValuePair<string, SeriesChartType>("Pie", SeriesChartType.Pie)
            };

            isInitializing = true;

            cmbTipe.DataSource = items;
            cmbTipe.DisplayMember = "Key";
            cmbTipe.ValueMember = "Value";
            cmbTipe.SelectedIndex = 0;

            isInitializing = false;
            loadDataChart();
        }

        // === LANGKAH 10: Kode loadDataChart() yang sudah diperbaiki tipe datanya ===
        public void loadDataChart()
        {
            chartProdi.Series.Clear();
            chartProdi.Titles.Clear();
            chartProdi.Legends.Clear();
            chartProdi.ChartAreas.Clear();

            ChartArea ca = new ChartArea("MainArea");
            ca.AxisX.Title = "Program Studi";
            ca.AxisY.Title = "Jumlah Mahasiswa";
            ca.AxisX.LabelStyle.Angle = -45;
            ca.BackColor = Color.Transparent;
            chartProdi.ChartAreas.Add(ca);

            try
            {
                if (button == 1)
                {
                    dt = dbLogic.getDataChartByTahun(dtpTanggalMasuk.Value);
                }
                else
                {
                    dt = dbLogic.getAllDataChart();
                }

                SeriesChartType tipe = (SeriesChartType)cmbTipe.SelectedValue;

                if (tipe == SeriesChartType.Column)
                {
                    Series s = new Series("Mahasiswa");
                    s.ChartType = SeriesChartType.Column;
                    foreach (DataRow row in dt.Rows)
                    {
                        string prodi = row["NamaProdi"].ToString();

                        // PERBAIKAN: Membuang (long) agar tidak terjadi bentrok tipe data
                        int jumlah = Convert.ToInt32(row["JmlhMhs"]);

                        s.Points.AddXY(prodi, jumlah);
                    }
                    chartProdi.Series.Add(s);
                }
                else
                {
                    Series s = new Series("Jumlah Mahasiswa");
                    s.ChartType = tipe;

                    s.IsValueShownAsLabel = true;
                    s.Label = "#VAL";
                    s.LegendText = "#VALX";

                    foreach (DataRow row in dt.Rows)
                    {
                        string prodi = row["NamaProdi"].ToString();

                        // PERBAIKAN: Membuang (long) agar tidak terjadi bentrok tipe data
                        int jumlah = Convert.ToInt32(row["JmlhMhs"]);

                        s.Points.AddXY(prodi, jumlah);
                    }
                    chartProdi.Series.Add(s);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data: " + ex.Message);
            }

            Title title = new Title("Jumlah Mahasiswa per Program Studi", Docking.Top, new Font("Arial", 14, FontStyle.Bold), Color.DarkBlue);
            chartProdi.Titles.Add(title);
            Legend legend = new Legend("MainLegend");
            legend.Docking = Docking.Right;
            chartProdi.Legends.Add(legend);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Dashboard_Load(object sender, EventArgs e)
        {

        }

        private void cmbTipe_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isInitializing)
                return;

            // Perbaiki di sini agar saat ganti tipe, grafik tetap memuat data yang benar
            loadDataChart();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            button = 1;
            loadDataChart();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Memanggil class Form1 (bukan nama tombol)
            Form1 frm1 = new Form1();
            frm1.Show();
            this.Hide();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            button = 0;
            loadDataChart(); // Memuat ulang semua data

            // Memunculkan pesan singkat agar tahu proses reset berhasil
            MessageBox.Show("Grafik telah direset ke tampilan semua angkatan.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}