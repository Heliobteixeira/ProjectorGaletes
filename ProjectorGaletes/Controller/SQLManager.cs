using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ProjectorGaletes
{
    class SQLManager
    {
        SqlConnection dbConnection;
        string connectionString;

        public SQLManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        static void ToggleConfigEncryption(string exeConfigName)
        {
            // Takes the executable file name without the 
            // .config extension. 
            try
            {
                // Open the configuration file and retrieve  
                // the connectionStrings section.
                Configuration config = ConfigurationManager.
                    OpenExeConfiguration(exeConfigName);

                ConnectionStringsSection section =
                    config.GetSection("connectionStrings")
                    as ConnectionStringsSection;

                if (section.SectionInformation.IsProtected)
                {
                    // Remove encryption.
                    section.SectionInformation.UnprotectSection();
                }
                else
                {
                    // Encrypt the section.
                    section.SectionInformation.ProtectSection(
                        "DataProtectionConfigurationProvider");
                }
                // Save the current configuration.
                config.Save();

                Console.WriteLine("Protected={0}",
                    section.SectionInformation.IsProtected);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public bool Connect()
        {
            if (dbConnection==null) 
            {
                //var connectionString = ConfigurationManager.ConnectionStrings["DesperdicioCartao"].ConnectionString;
                dbConnection = new SqlConnection(connectionString);

                try
                {
                    dbConnection.Open();
                }
                catch (Exception e)
                {
                    dbConnection = null;
                    Console.WriteLine(e.ToString());
                }
            }
            else
            {
                return true;
            }

            if (dbConnection != null)
                return true;
            else
                return false;

        }

        public void Disconnect()
        {
            if (dbConnection != null) dbConnection.Close();
        }

        public void executeStoredProcedure(string storedProcName) // SqlParameters are passed as reference to allow read OUT values
        {
            if (dbConnection == null) Connect();
            SqlCommand cmd = new SqlCommand(storedProcName, dbConnection);
            cmd.CommandType = CommandType.StoredProcedure;

            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                // iterate through results, printing each to console
                while (rdr.Read())
                {
                    Console.WriteLine(rdr);
                }
            }
        }

        public void executeStoredProcedure(string storedProcName, ref SqlParameter[] sqlParameters) // SqlParameters are passed as reference to allow read OUT values
        {
            if (dbConnection == null) Connect();
            SqlCommand cmd = new SqlCommand(storedProcName, dbConnection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddRange(sqlParameters);

            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                // iterate through results, printing each to console
                while (rdr.Read())
                {
                    Console.WriteLine(rdr);
                }
            }
        }

        public static SqlParameter newSqlParameter(string name, object value, SqlDbType type, ParameterDirection direction = ParameterDirection.Input) 
        {
            SqlParameter sqlPrmtr = new SqlParameter(name, value);
            sqlPrmtr.SqlDbType = type;
            sqlPrmtr.Direction = direction;

            return sqlPrmtr;
        }


        /*
        public void SaveRecord(DateTime datahora, string origem, int peso)
        {
            if (dbConnection==null) Connect();
            string sqlQuery=String.Format("INSERT INTO t_sobrascartao (datahora, origem, peso) VALUES (@datahora, @origem, @peso)");
            SqlCommand cmd = new SqlCommand(sqlQuery, dbConnection);

            cmd.Parameters.Add(new SqlParameter("@datahora", datahora));
            cmd.Parameters.Add(new SqlParameter("@origem", origem));
            cmd.Parameters.Add(new SqlParameter("@peso", peso));

            cmd.ExecuteNonQuery();

        }
         */


    }
}
