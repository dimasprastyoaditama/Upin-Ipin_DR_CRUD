using System;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Windows.Forms;
using System.IO;

namespace CRUDMahasiswaADO
{
    internal class DAL
    {
        public static string GetLoacalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        return ip.ToString();
                }
            }
            catch { }
            return "127.0.0.1";
        }

        public string GetConnectionString()
        {
            return $"Data Source={GetLoacalIPAddress()}\\DIMASPRASTYO;Initial Catalog=DBAkademikADO;User ID=sa;Password=sa1980;";
        }

        SqlConnection conn;

        public DAL()
        {
            conn = new SqlConnection(GetConnectionString());
        }

        public int CountMhs()
        {
            if (conn.State == ConnectionState.Closed) conn.Open();
            SqlCommand cmd = new SqlCommand("sp_CountMahasiswa", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlParameter outputParam = new SqlParameter("@Total", SqlDbType.Int);
            outputParam.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(outputParam);
            cmd.ExecuteNonQuery();
            return Convert.ToInt32(outputParam.Value);
        }

        public DataTable GetMhs()
        {
            if (conn.State == ConnectionState.Closed) conn.Open();
            SqlCommand cmd = new SqlCommand("SELECT * FROM Mahasiswa", conn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public void InsertMhs(string nim, string nama, string alamat, string jenisKelamin, DateTime tanggalLahir, string kodeProdi, byte[] foto)
        {
            if (conn.State == ConnectionState.Closed) conn.Open();
            SqlCommand cmd = new SqlCommand("sp_InsertMahasiswa", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@NIM", nim);
            cmd.Parameters.AddWithValue("@Nama", nama);
            cmd.Parameters.AddWithValue("@Alamat", alamat);
            cmd.Parameters.AddWithValue("@TanggalLahir", tanggalLahir);
            cmd.Parameters.AddWithValue("@JenisKelamin", jenisKelamin);
            cmd.Parameters.AddWithValue("@KodeProdi", kodeProdi);

            SqlParameter paramFoto = new SqlParameter("@Foto", SqlDbType.VarBinary);
            paramFoto.Value = (foto != null && foto.Length > 0) ? (object)foto : DBNull.Value;
            cmd.Parameters.Add(paramFoto);

            cmd.ExecuteNonQuery();
        }

        public void UpdateMhs(string nim, string nama, string alamat, string jenisKelamin, DateTime tanggalLahir, string kodeProdi, byte[] foto)
        {
            if (conn.State == ConnectionState.Closed) conn.Open();
            SqlCommand cmd = new SqlCommand("sp_UpdateMahasiswa", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@NIM", nim);
            cmd.Parameters.AddWithValue("@Nama", nama);
            cmd.Parameters.AddWithValue("@Alamat", alamat);
            cmd.Parameters.AddWithValue("@JenisKelamin", jenisKelamin);
            cmd.Parameters.AddWithValue("@TanggalLahir", tanggalLahir);
            cmd.Parameters.AddWithValue("@KodeProdi", kodeProdi);

            SqlParameter paramFoto = new SqlParameter("@Foto", SqlDbType.VarBinary);
            paramFoto.Value = (foto != null && foto.Length > 0) ? (object)foto : DBNull.Value;
            cmd.Parameters.Add(paramFoto);

            cmd.ExecuteNonQuery();
        }

        public void DeleteMhs(string nim)
        {
            if (conn.State == ConnectionState.Closed) conn.Open();
            SqlCommand cmd = new SqlCommand("sp_DeleteMahasiswa", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@NIM", nim);
            cmd.ExecuteNonQuery();
        }

        public void InsertLog(string message)
        {
            try
            {
                if (conn.State == ConnectionState.Closed) conn.Open();
                SqlCommand cmd = new SqlCommand("sp_LogMessage", conn);
                cmd.Parameters.AddWithValue("@psn", message);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        public DataTable CariMahasiswa(string nim)
        {
            if (conn.State == ConnectionState.Closed) conn.Open();
            SqlCommand cmd = new SqlCommand("SELECT * FROM Mahasiswa WHERE NIM = @nim", conn);
            cmd.Parameters.AddWithValue("@nim", nim);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        // --- FUNGSI RESET DATA DIPERBAIKI ---
        public void resetData()
        {
            if (conn.State == ConnectionState.Closed) conn.Open();

            // Kolom FOTO dikecualikan jika tabel backup tidak memilikinya
            string queryReset = @"
                DELETE FROM Mahasiswa; 
                INSERT INTO Mahasiswa (NIM, Nama, Alamat, JenisKelamin, TanggalLahir, KodeProdi) 
                SELECT NIM, Nama, Alamat, JenisKelamin, TanggalLahir, KodeProdi FROM mahasiswa_backup;";

            new SqlCommand(queryReset, conn).ExecuteNonQuery();
        }

        public void testInject(string nim)
        {
            if (conn.State == ConnectionState.Closed) conn.Open();
            new SqlCommand("Update Mahasiswa set Nama = 'HACKED' where NIM = '" + nim + "'", conn).ExecuteNonQuery();
        }

        public DataTable getDataRekap(string prodi, DateTime tanggalMasuk)
        {
            if (conn.State == ConnectionState.Closed) conn.Open();
            SqlCommand cmd = new SqlCommand("sp_Report", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@inProdi", prodi);
            cmd.Parameters.AddWithValue("@inTglMsuk", tanggalMasuk.Year.ToString());
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable getAllDataChart()
        {
            if (conn.State == ConnectionState.Closed) conn.Open();
            SqlCommand cmd = new SqlCommand("sp_DashBoard", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable getDataChartByTahun(DateTime thMasuk)
        {
            if (conn.State == ConnectionState.Closed) conn.Open();
            SqlCommand cmd = new SqlCommand("sp_DashBoardByTahun", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@inTglMsuk", thMasuk.Year);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }
}