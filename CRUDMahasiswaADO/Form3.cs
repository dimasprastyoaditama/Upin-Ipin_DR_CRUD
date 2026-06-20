using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public partial class Form3 : Form
    {
        // 1. Deklarasikan object DAL sesuai instruksi modul
        DAL dbLogic = new DAL();

        // Object report bawaan Anda
        Praktikum_14 listMahasiswa = new Praktikum_14();

        string prodi { get; set; }
        DateTime tglmasuk { get; set; }

        public Form3(string Prodi, DateTime TglMasuk)
        {
            InitializeComponent();

            prodi = Prodi;
            tglmasuk = TglMasuk;

            try
            {
                // 2. Ambil data mentah dari database menggunakan class DAL
                DataTable dtMahasiswa = dbLogic.getDataRekap(prodi, tglmasuk);

                // 3. PROSES KONVERSI: Mengubah DataTable menjadi List<Mahasiswa> (diambil dari Data.cs)
                List<Mahasiswa> listData = new List<Mahasiswa>();
                foreach (DataRow row in dtMahasiswa.Rows)
                {
                    Mahasiswa mhs = new Mahasiswa();
                    mhs.Nama = row["Nama"].ToString();
                    mhs.JenisKelamin = row["JenisKelamin"].ToString();
                    mhs.Alamat = row["Alamat"].ToString();
                    mhs.NamaProdi = row["NamaProdi"].ToString();
                    mhs.TanggalDaftar = Convert.ToDateTime(row["TanggalDaftar"]);

                    listData.Add(mhs);
                }

                // 4. Masukkan list data ke dalam Crystal Report
                listMahasiswa.SetDataSource(listData);
                crystalReportViewer2.ReportSource = listMahasiswa;
                crystalReportViewer2.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }

        private void crystalReportViewer2_Load(object sender, EventArgs e)
        {
            // Biarkan kosong sesuai bawaan form Anda
        }
    }
}