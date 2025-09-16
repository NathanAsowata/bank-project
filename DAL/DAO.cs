using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DAO
    {
        SqlConnection con;
        private readonly string _connectionString;
        public DAO()
        {
            // Set the connection string as a readonly string
            _connectionString = ConfigurationManager.ConnectionStrings["DBCon"].ConnectionString;
            
        }

        public SqlConnection OpenCon()
        {
            // Initialize the dao connection only when called to avoid clashes/errors.
            con = new SqlConnection(_connectionString);
            
            if (con.State == ConnectionState.Broken || con.State == ConnectionState.Closed)
            {
                con.Open();
                
            }
            return con;
        }

        public void CloseCon()
        {
            if (con != null)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }
    }
}
