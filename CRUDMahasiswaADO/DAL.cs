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

 
    }
}