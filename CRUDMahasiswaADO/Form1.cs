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

        private void Form1_Load(object sender, EventArgs e) => LoadData();

        private void LoadData()
        {
            try
            {
                DataTable dt = dbLogic.GetMhs();
                bindingSource1.DataSource = dt;
                dataGridView1.DataSource = bindingSource1;
                if (dataGridView1.Columns["Foto"] is DataGridViewImageColumn f) f.ImageLayout = DataGridViewImageCellLayout.Stretch;
                HitungTotal();
            }
            catch (Exception ex) { MessageBox.Show("Gagal load data: " + ex.Message); }
        }

        private void HitungTotal() { try { lblCountMhs.Text = "Total Mahasiswa : " + dbLogic.CountMhs(); } catch { } }

        private void ClearForm()
        {
            txtNIM.Enabled = true; txtNIM.Clear(); txtNama.Clear(); cmbJK.SelectedIndex = -1;
            txtAlamat.Clear(); txtKodeProdi.Clear(); dtpTanggalLahir.Value = DateTime.Now;
            fotohMs.Image = null; txtNIM.Focus();
        }

        // --- FIX GDI+ ERROR ---
        private byte[] ConvertImageToBytes(PictureBox pb)
        {
            if (pb.Image == null) return null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (Bitmap bmp = new Bitmap(pb.Image)) { bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg); }
                return ms.ToArray();
            }
        }

        // --- CRUD UTAMA ---
        private void btnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                // Pengecekan NIM Duplikat
                DataTable dt = dbLogic.CariMahasiswa(txtNIM.Text.Trim());
                if (dt != null && dt.Rows.Count > 0)
                {
                    MessageBox.Show("NIM sudah ada di database!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Proses simpan ke database
                dbLogic.InsertMhs(txtNIM.Text, txtNama.Text, txtAlamat.Text, cmbJK.Text, dtpTanggalLahir.Value.Date, txtKodeProdi.Text, ConvertImageToBytes(fotohMs));

                // --- INI PESAN SUKSESNYA ---
                MessageBox.Show("Data berhasil ditambahkan!", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Refresh tabel dan bersihkan form
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                // Pesan jika terjadi error
                MessageBox.Show("Gagal menyimpan data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // Proses update ke database
                dbLogic.UpdateMhs(txtNIM.Text, txtNama.Text, txtAlamat.Text, cmbJK.Text, dtpTanggalLahir.Value.Date, txtKodeProdi.Text, ConvertImageToBytes(fotohMs));

                // Memunculkan pesan sukses
                MessageBox.Show("Data berhasil diupdate!", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Refresh tabel dan bersihkan form
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                // Memunculkan pesan jika terjadi error
                MessageBox.Show("Gagal mengupdate data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Yakin hapus?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes) { dbLogic.DeleteMhs(txtNIM.Text.Trim()); LoadData(); ClearForm(); }
        }

        private void btnUploadGambar_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Images|*.jpg;*.png;*.bmp" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using (FileStream fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read)) fotohMs.Image = Image.FromStream(fs);
                fotohMs.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }

        // --- FITUR PENCARIAN (FIX CS1061 & Foto) ---
        private void btnCari_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNIM.Text)) { MessageBox.Show("Masukkan NIM untuk mencari!"); return; }
                DataTable dt = dbLogic.CariMahasiswa(txtNIM.Text.Trim());
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    txtNama.Text = row["Nama"].ToString();
                    cmbJK.Text = row["JenisKelamin"].ToString();
                    txtAlamat.Text = row["Alamat"].ToString();
                    txtKodeProdi.Text = row["KodeProdi"].ToString();
                    if (row["TanggalLahir"] != DBNull.Value) dtpTanggalLahir.Value = Convert.ToDateTime(row["TanggalLahir"]);

                    // Tarik Foto
                    if (row["Foto"] != DBNull.Value && row["Foto"] is byte[])
                    {
                        using (MemoryStream ms = new MemoryStream((byte[])row["Foto"])) { fotohMs.Image = Image.FromStream(ms); }
                    }
                    else { fotohMs.Image = null; }

                    txtNIM.Enabled = false;
                }
                else { MessageBox.Show("Data tidak ditemukan."); }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        // --- FIX DATA CLICKED (Tarik Data & Foto dari Tabel) ---
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                // Ambil data teks menggunakan indeks
                txtNIM.Text = row.Cells[0].Value?.ToString();
                txtNama.Text = row.Cells[1].Value?.ToString();
                cmbJK.Text = row.Cells[2].Value?.ToString();

                if (row.Cells[3].Value != DBNull.Value)
                    dtpTanggalLahir.Value = Convert.ToDateTime(row.Cells[3].Value);

                txtAlamat.Text = row.Cells[4].Value?.ToString();
                txtKodeProdi.Text = row.Cells[5].Value?.ToString();

                // --- PERBAIKAN LOGIKA FOTO ---
                try
                {
                    // Gunakan nama kolom secara eksplisit (pastikan bernama "Foto") agar tidak salah urutan
                    if (row.Cells["Foto"].Value != DBNull.Value && row.Cells["Foto"].Value is byte[])
                    {
                        byte[] imgBytes = (byte[])row.Cells["Foto"].Value;

                        // PENTING: Jangan gunakan 'using' di sini. 
                        // PictureBox butuh MemoryStream tetap terbuka untuk menampilkan gambar.
                        MemoryStream ms = new MemoryStream(imgBytes);
                        fotohMs.Image = Image.FromStream(ms);
                    }
                    else
                    {
                        // Jika datanya memang tidak punya foto di database
                        fotohMs.Image = null;
                    }
                }
                catch
                {
                    // Jika nama kolom "Foto" tidak ditemukan atau error, kosongkan foto dengan aman
                    fotohMs.Image = null;
                }

                txtNIM.Enabled = false; // Kunci NIM saat mode update
            }
        }

        // --- FITUR IMPORT ---
        private void btnImpExcel_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog { Filter = "Excel Files|*.xlsx;*.xls" };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    using (var stream = File.Open(ofd.FileName, FileMode.Open, FileAccess.Read))
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet(new ExcelDataSetConfiguration() { ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true } });
                        dataGridView1.DataSource = result.Tables[0];
                    }
                    // Pesan sukses ditambahkan di sini
                    MessageBox.Show("Data Excel berhasil dimuat ke tabel!\nSilakan klik 'Import Form Database' untuk menyimpan ke sistem.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat file Excel: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                int gagal = 0; // Tambahan untuk menghitung data yang gagal (duplikat)

                foreach (DataRow row in dt.Rows)
                {
                    string nim = row["NIM"].ToString().Trim();
                    string nama = row["Nama"].ToString().Trim();
                    string jk = row["JenisKelamin"].ToString().Trim();
                    string alamat = row["Alamat"].ToString().Trim();
                    string kodeProdi = row["Nama Prodi"].ToString().Trim();

                    string fotoPath = row.Table.Columns.Contains("FotoPath")
                        ? row["FotoPath"].ToString().Trim()
                        : string.Empty;

                    if (string.IsNullOrEmpty(nim) || string.IsNullOrEmpty(nama))
                        continue;

                    // --- SISTEM KEAMANAN ANTI ERROR (CEK DUPLIKAT) ---
                    DataTable cekDt = dbLogic.CariMahasiswa(nim);
                    if (cekDt != null && cekDt.Rows.Count > 0)
                    {
                        gagal++; // Hitung sebagai gagal
                        continue; // Lompati data ini, jangan di-insert agar tidak crash
                    }
                    // --------------------------------------------------

                    DateTime tglLahir;
                    if (!DateTime.TryParse(row["TanggalLahir"].ToString(), out tglLahir))
                        continue;

                    byte[] ConvertImageFromPath(string path)
                    {
                        if (string.IsNullOrWhiteSpace(path)) return null;
                        if (!File.Exists(path)) return null;
                        return File.ReadAllBytes(path);
                    }

                    byte[] fotoBytes = ConvertImageFromPath(fotoPath);

                    // Eksekusi insert
                    dbLogic.InsertMhs(nim, nama, alamat, jk, tglLahir, kodeProdi, fotoBytes);
                    sukses++;
                }

                // Tampilkan laporan yang rapi
                MessageBox.Show($"Import Selesai!\n\nBerhasil masuk: {sukses} data\nDilewati (NIM Duplikat): {gagal} data", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearForm();
                LoadData();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("General Error: " + ex.Message);
            }
        }

        // --- EVENT JEMBATAN LAINNYA ---
        private void btnRefresh_Click(object sender, EventArgs e) { LoadData(); ClearForm(); }
        private void btnConnect_Click_1(object sender, EventArgs e) { MessageBox.Show("Koneksi OK"); }
        private void btnLoad_Click_1(object sender, EventArgs e) { LoadData(); }
        private void btnResetData_Click_1(object sender, EventArgs e) { dbLogic.resetData(); LoadData(); }
        private void btnTestInjection_Click_1(object sender, EventArgs e) { dbLogic.testInject(txtNIM.Text); LoadData(); }
        private void btnRekapData_Click(object sender, EventArgs e) { new Form2().ShowDialog(); }
        private void lblCountMhs_Click(object sender, EventArgs e) { HitungTotal(); }

        private void fotohMs_Click(object sender, EventArgs e)
        {
            // Membuka jendela dialog untuk memilih file gambar
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Images|*.jpg;*.png;*.bmp" };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                // Membaca file gambar dengan aman (menghindari lock file)
                using (FileStream fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read))
                {
                    fotohMs.Image = Image.FromStream(fs);
                }

                // Memastikan gambar pas dengan ukuran kotak
                fotohMs.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }
    }
}