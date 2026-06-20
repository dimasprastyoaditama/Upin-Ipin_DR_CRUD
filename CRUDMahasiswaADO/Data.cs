using System;

namespace CRUDMahasiswaADO
{
    // Ingat, kata "public" di bawah ini sangat wajib agar terbaca oleh Form3
    public class Mahasiswa
    {
        public string Nama { get; set; }
        public string JenisKelamin { get; set; }
        public string Alamat { get; set; }
        public string NamaProdi { get; set; }
        public DateTime TanggalDaftar { get; set; }
    }
}