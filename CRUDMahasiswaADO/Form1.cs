using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using ExcelDataReader;

namespace CRUDMahasiswaADO
{
    public partial class Form1 : Form
    {
        DAL dbLogic = new DAL();
        BindingSource bindingSource1 = new BindingSource();

        public Form1()
        {
            InitializeComponent();
        }

        private void HitungTotal()
        {
            try
            {
                int total = (dbLogic.CountMhs().Equals(DBNull.Value)) ? 0 : dbLogic.CountMhs();
                lblCountMhs.Text = "Total Mahasiswa : " + total;
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("Gagal hitung total: " + ex.Message);
            }
        }

        private void LoadData()
        {
            try
            {
                bindingSource1.DataSource = dbLogic.GetMhs();
                dataGridView1.DataSource = bindingSource1;

                DataGridViewImageColumn fotoColumn = (DataGridViewImageColumn)dataGridView1.Columns["Foto"];
                if (fotoColumn != null)
                {
                    fotoColumn.ImageLayout = DataGridViewImageCellLayout.Stretch;
                }

                HitungTotal();

                dataGridView1.Enabled = true;
                btnConnect.Enabled = true;
                btnInsert.Enabled = true;
                btnUpdate.Enabled = true;
                btnDelete.Enabled = true;
                btnLoad.Enabled = true;
                btnResetData.Enabled = true;
                btnTestInjection.Enabled = true;
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }

        private void ClearForm()
        {
            txtNIM.Enabled = true;
            txtNIM.Clear();
            txtNama.Clear();
            cmbJK.SelectedIndex = -1;
            txtAlamat.Clear();
            txtKodeProdi.Clear();
            dtpTanggalLahir.Value = DateTime.Now;
            fotohMs.Image = null;
            txtNIM.Focus();
        }

        public void simpanLog(string message)
        {
            dbLogic.InsertLog(message);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection("Data Source=ADIT\\DIMASPRASTYO;Initial Catalog=DBAkademikADO;Integrated Security=True"))
                {
                    conn.Open();
                    MessageBox.Show("Koneksi Berhasil");
                }
            }
            catch (SqlException ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message);
            }
        }

        private void btnCari_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNIM.Text))
                {
                    MessageBox.Show("Masukkan NIM terlebih dahulu untuk mencari!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataTable dt = dbLogic.CariMahasiswa(txtNIM.Text.Trim());

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    txtNama.Text = row["Nama"].ToString();
                    cmbJK.Text = row["JenisKelamin"].ToString();

                    if (row["TanggalLahir"] != DBNull.Value)
                        dtpTanggalLahir.Value = Convert.ToDateTime(row["TanggalLahir"]);

                    txtAlamat.Text = row["Alamat"].ToString();
                    txtKodeProdi.Text = row["KodeProdi"].ToString();

                    if (row["Foto"] != DBNull.Value)
                    {
                        byte[] imgBytes = (byte[])row["Foto"];
                        using (MemoryStream ms = new MemoryStream(imgBytes))
                        {
                            fotohMs.Image = Image.FromStream(ms);
                            fotohMs.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                    }
                    else
                    {
                        fotohMs.Image = null;
                    }

                    txtNIM.Enabled = false;
                }
                else
                {
                    MessageBox.Show("Data dengan NIM tersebut tidak ditemukan.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saat mencari data: " + ex.Message, "Detail Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                simpanLog(ex.Message);
            }
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] imgBytes = ConvertImageToBytes(fotohMs);
                dbLogic.InsertMhs(txtNIM.Text, txtNama.Text, txtAlamat.Text, cmbJK.Text, dtpTanggalLahir.Value.Date, txtKodeProdi.Text, imgBytes);
                MessageBox.Show("Data mahasiswa berhasil ditambahkan");
                ClearForm();
                LoadData();
            }
            catch (SqlException ex)
            {
                simpanLog("Rollback Insert : " + ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                simpanLog("General Error : " + ex.Message);
                MessageBox.Show("General Error : " + ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] imgBytes = ConvertImageToBytes(fotohMs);
                dbLogic.UpdateMhs(txtNIM.Text, txtNama.Text, txtAlamat.Text, cmbJK.Text, dtpTanggalLahir.Value.Date, txtKodeProdi.Text, imgBytes);
                MessageBox.Show("Data mahasiswa berhasil diubah");
                ClearForm();
                LoadData();
            }
            catch (SqlException ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dg = MessageBox.Show("Yakin ingin menghapus data?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dg == DialogResult.Yes)
                {
                    dbLogic.DeleteMhs(txtNIM.Text);
                    MessageBox.Show("Data mahasiswa berhasil dihapus");
                    ClearForm();
                    LoadData();
                }
            }
            catch (SqlException ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message);
            }
        }

        private void btnResetData_Click(object sender, EventArgs e)
        {
            btnResetData_Click_1(sender, e);
        }

        private void btnTestInjection_Click(object sender, EventArgs e)
        {
            btnTestInjection_Click_1(sender, e);
        }

        // =========================================================================
        // INI ADALAH EVENT KLIK TABEL YANG SUDAH DIPERBAIKI (DATA MELOMPAT KE ATAS)
        // =========================================================================
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = this.dataGridView1.Rows[e.RowIndex];

                    txtNIM.Text = row.Cells["NIM"].Value?.ToString();
                    txtNama.Text = row.Cells["Nama"].Value?.ToString();
                    cmbJK.Text = row.Cells["JenisKelamin"].Value?.ToString();

                    if (row.Cells["TanggalLahir"].Value != DBNull.Value && row.Cells["TanggalLahir"].Value != null)
                    {
                        dtpTanggalLahir.Value = Convert.ToDateTime(row.Cells["TanggalLahir"].Value);
                    }

                    txtAlamat.Text = row.Cells["Alamat"].Value?.ToString();

                    if (this.dataGridView1.Columns.Contains("KodeProdi"))
                    {
                        txtKodeProdi.Text = row.Cells["KodeProdi"].Value?.ToString();
                    }
                    else if (this.dataGridView1.Columns.Contains("NamaProdi"))
                    {
                        txtKodeProdi.Text = row.Cells["NamaProdi"].Value?.ToString();
                    }

                    if (this.dataGridView1.Columns.Contains("Foto") && row.Cells["Foto"].Value != DBNull.Value && row.Cells["Foto"].Value != null)
                    {
                        byte[] imgBytes = (byte[])row.Cells["Foto"].Value;
                        using (MemoryStream ms = new MemoryStream(imgBytes))
                        {
                            fotohMs.Image = Image.FromStream(ms);
                            fotohMs.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                    }
                    else
                    {
                        fotohMs.Image = null;
                    }

                    txtNIM.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Peringatan Klik: " + ex.Message);
            }
        }

        // Wadah kosong ini sengaja dibiarkan agar tidak error di Design
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }
        // =========================================================================

        private void btnUpload_Click(object sender, EventArgs e)
        {
            btnUploadGambar_Click(sender, e);
        }

        private void btnImpExcel_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Excel Workbook|*.xlsx;*.xls" })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filePath = openFileDialog.FileName;
                        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                        {
                            using (var reader = ExcelReaderFactory.CreateReader(stream))
                            {
                                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                                {
                                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                                });

                                DataTable dt = result.Tables[0];

                                if (dt.Rows.Count == 0)
                                {
                                    MessageBox.Show("File Excel berhasil dibaca, tetapi isinya KOSONG!\n\nPastikan data Anda diketik di lembar pertama (Sheet1).", "Peringatan Excel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                dataGridView1.DataSource = null;
                                dataGridView1.AutoGenerateColumns = true;
                                dataGridView1.DataSource = dt;
                                dataGridView1.Refresh();

                                MessageBox.Show("Data Excel berhasil ditampilkan di layar!\n\nJangan lupa klik tombol 'Import Form Database' agar data ini tersimpan permanen ke SQL Server.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal memuat file Excel. \n\nKemungkinan Penyebab:\n1. File Excel Anda masih terbuka (Tutup dulu aplikasi Excel-nya).\n2. Belum menginstal NuGet Package 'ExcelDataReader.DataSet'.\n\nDetail Error: " + ex.Message, "Error Import Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnImpDb_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = (DataTable)dataGridView1.DataSource;
                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk diimport.");
                    return;
                }

                int sukses = 0;
                foreach (DataRow row in dt.Rows)
                {
                    string nim = row["NIM"].ToString().Trim();
                    string nama = row["Nama"].ToString().Trim();
                    string jk = row["JenisKelamin"].ToString().Trim();
                    string alamat = row["Alamat"].ToString().Trim();
                    string kodeProdi = row["NamaProdi"].ToString().Trim();
                    string fotoPath = row.Table.Columns.Contains("FotoPath") ? row["FotoPath"].ToString().Trim() : string.Empty;

                    if (string.IsNullOrEmpty(nim) || string.IsNullOrEmpty(nama)) continue;

                    DateTime tglLahir;
                    if (!DateTime.TryParse(row["TanggalLahir"].ToString(), out tglLahir)) continue;

                    byte[] fotoBytes = ConvertImageFromPath(fotoPath);
                    dbLogic.InsertMhs(nim, nama, alamat, jk, tglLahir, kodeProdi, fotoBytes);
                    sukses++;
                }

                MessageBox.Show("Data mahasiswa berhasil ditambahkan: " + sukses + " baris");
                ClearForm();
                LoadData();
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("Gagal import: " + ex.Message);
            }
        }

        private byte[] ConvertImageToBytes(PictureBox pb)
        {
            if (pb.Image == null) return null;
            using (MemoryStream ms = new MemoryStream())
            {
                pb.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        private byte[] ConvertImageFromPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
            return File.ReadAllBytes(path);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            btnLoad_Click_1(sender, e);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                txtNIM.Enabled = true;
                btnInsert.Enabled = true;
                btnUpdate.Enabled = true;
                btnDelete.Enabled = true;
                btnCari.Enabled = true;

                ClearForm();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saat refresh: " + ex.Message);
            }
        }

        private void btnLoad_Click_1(object sender, EventArgs e)
        {
            try
            {
                txtNIM.Enabled = true;
                LoadData();
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("Gagal memuat data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnConnect_Click_1(object sender, EventArgs e)
        {
            btnUploadGambar_Click(sender, e);
        }

        private void btnUploadGambar_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    fotohMs.Image = Image.FromFile(ofd.FileName);
                    fotohMs.SizeMode = PictureBoxSizeMode.StretchImage;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan saat mengunggah gambar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                simpanLog(ex.Message);
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV Files (*.csv)|*.csv";
            ofd.Title = "Pilih File Data Mahasiswa CSV";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    int jumlahBerhasil = 0;

                    using (StreamReader sr = new StreamReader(ofd.FileName))
                    {
                        string baris;
                        bool headerTerlewati = false;

                        while ((baris = sr.ReadLine()) != null)
                        {
                            if (!headerTerlewati)
                            {
                                headerTerlewati = true;
                                continue;
                            }

                            string[] data = baris.Split(';');

                            if (data.Length >= 6)
                            {
                                string nim = data[0].Trim();
                                string nama = data[1].Trim();
                                string alamat = data[2].Trim();
                                string jenisKelamin = data[3].Trim();

                                DateTime tanggalLahir;
                                DateTime.TryParse(data[4].Trim(), out tanggalLahir);

                                string kodeProdi = data[5].Trim();

                                dbLogic.InsertMhs(nim, nama, alamat, jenisKelamin, tanggalLahir, kodeProdi, null);
                                jumlahBerhasil++;
                            }
                        }
                    }

                    MessageBox.Show($"Mantap! {jumlahBerhasil} data mahasiswa berhasil diimpor.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal mengimpor data. Pastikan urutan dan format CSV benar.\n\nDetail: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnResetData_Click_1(object sender, EventArgs e)
        {
            try
            {
                DialogResult dialog = MessageBox.Show("Apakah Anda yakin ingin mereset seluruh data mahasiswa ke kondisi awal?", "Konfirmasi Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dialog == DialogResult.Yes)
                {
                    dbLogic.resetData();
                    MessageBox.Show("Data berhasil direset!", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadData();
                }
            }
            catch (SqlException ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnTestInjection_Click_1(object sender, EventArgs e)
        {
            try
            {
                dbLogic.testInject(txtNIM.Text);
                LoadData();
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("safe"))
                {
                    simpanLog(ex.Message);
                    MessageBox.Show("SQL Error : Unsafe UPDATE operation not allowed", "Peringatan Keamanan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    simpanLog(ex.Message);
                    MessageBox.Show("SQL Error : " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRekapData_Click(object sender, EventArgs e)
        {
            try
            {
                Form2 formRekap = new Form2();
                formRekap.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membuka form rekap: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lblCountMhs_Click(object sender, EventArgs e)
        {
            try
            {
                HitungTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menghitung ulang: " + ex.Message);
            }
        }
    }
}



