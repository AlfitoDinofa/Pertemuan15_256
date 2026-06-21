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


    }
}