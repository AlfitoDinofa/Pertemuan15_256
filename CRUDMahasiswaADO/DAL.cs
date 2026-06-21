using System;
using System.Data;
using System.Data.SqlClient;

namespace CRUDMahasiswaADO
{
    class DAL
    {
        static string connectionString = @"Data Source=LAPTOP-IF142S7G\ALFITO; Initial Catalog=DBAkademikADO; Integrated Security=True";

        public string GetConnectionString()
        {
            return connectionString;
        }

        SqlConnection conn = new SqlConnection(connectionString);
        SqlDataAdapter da;
        DataTable dtMahasiswa;
        DataTable dtProdi;

        public int CountMhs()
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlCommand cmd = new SqlCommand("sp_CountMahasiswa", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            SqlParameter outputParam = new SqlParameter("@Total", SqlDbType.Int);
            outputParam.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(outputParam);

            cmd.ExecuteNonQuery();
            conn.Close();

            if (outputParam.Value == null || outputParam.Value == DBNull.Value)
                return 0;

            return Convert.ToInt32(outputParam.Value);
        }

        public DataTable GetMhs()
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlCommand cmd = new SqlCommand("sp_GetMahasiswa", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            conn.Close();
            return dt;
        }

        public void InsertMhs(string nim, string nama, string alamat, string jenisKelamin, DateTime tanggalLahir, string kodeProdi, byte[] foto)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlCommand command = new SqlCommand("sp_InsertMahasiswa", conn);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("pNIM", nim);
            command.Parameters.AddWithValue("pNama", nama);
            command.Parameters.AddWithValue("pAlamat", alamat);
            command.Parameters.AddWithValue("pJenisKelamin", jenisKelamin);
            command.Parameters.AddWithValue("pTanggalLahir", tanggalLahir);
            command.Parameters.AddWithValue("pKodeProdi", kodeProdi);

            SqlParameter paramFoto = new SqlParameter("pFoto", SqlDbType.VarBinary, -1);
            paramFoto.Value = (object)foto ?? DBNull.Value;
            command.Parameters.Add(paramFoto);

            command.ExecuteNonQuery();
            conn.Close();
        }

        public void UpdateMhs(string nim, string nama, string alamat, string jenisKelamin, DateTime tanggalLahir, string kodeProdi, byte[] foto, bool updateFoto)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlCommand command = new SqlCommand("sp_UpdateMahasiswa", conn);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("pNIM", nim);
            command.Parameters.AddWithValue("pNama", nama);
            command.Parameters.AddWithValue("pAlamat", alamat);
            command.Parameters.AddWithValue("pJenisKelamin", jenisKelamin);
            command.Parameters.AddWithValue("pTanggalLahir", tanggalLahir);
            command.Parameters.AddWithValue("pKodeProdi", kodeProdi);

            // ========== UPDATE FOTO HANYA JIKA ADA GAMBAR BARU ==========
            if (updateFoto && foto != null)
            {
                SqlParameter paramFoto = new SqlParameter("pFoto", SqlDbType.VarBinary, -1);
                paramFoto.Value = foto;
                command.Parameters.Add(paramFoto);
            }
            else
            {
                // Jika tidak ada gambar baru, kirim NULL agar kolom Foto tidak berubah
                SqlParameter paramFoto = new SqlParameter("pFoto", SqlDbType.VarBinary, -1);
                paramFoto.Value = DBNull.Value;
                command.Parameters.Add(paramFoto);
            }
            // =============================================================

            command.ExecuteNonQuery();
            conn.Close();
        }

        public void DeleteMhs(string nim)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            SqlCommand cmd = new SqlCommand("sp_DeleteMahasiswa", conn);

            // ========== PERBAIKAN ==========
            cmd.Parameters.AddWithValue("@NIM", nim);  // ← GANTI pNIM → NIM
                                                       // ================================

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public void resetData()
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlCommand cmdDelete = new SqlCommand("DELETE FROM mahasiswa;", conn);
            cmdDelete.ExecuteNonQuery();

            SqlCommand cmdInsert = new SqlCommand("INSERT INTO mahasiswa SELECT * FROM mahasiswa_backup;", conn);
            cmdInsert.ExecuteNonQuery();

            conn.Close();
        }

        public void testInject(string nim)
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                string query = "UPDATE mahasiswa SET nama = 'HACKED' WHERE NIM = " + nim;
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error saat simulasi SQL Injection: " + ex.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public DataTable GetMhsByNIM(string nim)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlCommand cmd = new SqlCommand("sp_GetMahasiswaByNIM", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("pNIM", nim);

            da = new SqlDataAdapter(cmd);
            dtMahasiswa = new DataTable();
            da.Fill(dtMahasiswa);

            conn.Close();
            return dtMahasiswa;
        }

  
    }
}