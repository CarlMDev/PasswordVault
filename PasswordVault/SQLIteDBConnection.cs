using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows;

namespace PasswordVault
{
    internal class SQLIteDBConnection
    {
        private string sqlStatement = null;
        private SQLiteConnection sqlConnection = null;
        private SQLiteCommand sqlCommand = null;
       
        public SQLIteDBConnection()
        {
            if (!File.Exists("RecordKeeping.sqlite"))
            {
                SQLiteConnection.CreateFile("RecordKeeping.sqlite");
            }
            sqlConnection = new SQLiteConnection("Data Source=RecordKeeping.sqlite;Version=3;");
        }

        ~SQLIteDBConnection()
        {
            sqlConnection = null;
        }

        public void CreateRecordsTable()
        {
 
            sqlStatement = "CREATE TABLE  company (id INTEGER PRIMARY KEY AUTOINCREMENT, ";
            sqlStatement = sqlStatement + "name text NOT NULL, ";
            sqlStatement = sqlStatement + "login text NOT NULL, ";
            sqlStatement = sqlStatement + "password text NOT NULL) ";
            try
            {
                sqlCommand = new SQLiteCommand(sqlStatement, sqlConnection);
                sqlConnection.Open();
                sqlCommand.ExecuteNonQuery();
            }
            catch(SQLiteException ex){
                MessageBox.Show("Error on CreateRecordsTable(): " + ex.ToString());
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                }
            }

        }


        public bool CompanyTableExists()
        {
            var tableExists = false;

            sqlStatement = "";
            sqlStatement = "SELECT name FROM sqlite_master WHERE type='table' AND name='company';";

            try
            {
                sqlCommand = new SQLiteCommand(sqlStatement, sqlConnection);
                sqlConnection.Open();
                var thisReader = sqlCommand.ExecuteReader();

                if (thisReader.HasRows)
                {
                    tableExists = true;
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Error on CompanyTableExists(): " + ex.ToString());
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                }
            }

            return tableExists;
        }

        public DataTable RetrieveCompanyDetails()
        {
            sqlStatement = "";
            sqlStatement = "SELECT id, name, login, password FROM company ORDER BY name;";

            sqlCommand = new SQLiteCommand(sqlStatement, sqlConnection);
            var allCompanies = new DataTable();

            try
            {
                sqlConnection.Open();
                var sqlAdapter = new SQLiteDataAdapter(sqlCommand);
                sqlAdapter.Fill(allCompanies);
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Error on RetrieveCompanyDetails(): " + ex.ToString());
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                }
            }

            return allCompanies;
        }

        public bool PasswordExists(Company company)
        {
            var passwordExists = false;

            sqlStatement = "";
            sqlStatement = "SELECT password FROM company WHERE password = @password and name = @coname;";

            sqlCommand = new SQLiteCommand(sqlStatement, sqlConnection);
            sqlCommand.Parameters.Add(new SQLiteParameter("@coname", company.Name));
            sqlCommand.Parameters.Add(new SQLiteParameter("@password", company.Password));

            try
            {
                sqlConnection.Open();
                var thisReader = sqlCommand.ExecuteReader();

                if (thisReader.HasRows)
                {
                    passwordExists = true;
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Error on PasswordExists(Company company): " + ex.ToString());
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                }
            }

            return passwordExists;
        }

        public void InsertNewCompany(Company company)
        {
            sqlStatement = "INSERT INTO company (name, login, password) VALUES (@name, @login, @pass);";
            sqlCommand = new SQLiteCommand(sqlStatement, sqlConnection);
            sqlCommand.Parameters.Add(new SQLiteParameter("@name",company.Name));
            sqlCommand.Parameters.Add(new SQLiteParameter("@login", company.Login));
            sqlCommand.Parameters.Add(new SQLiteParameter("@pass", company.Password));

            try
            {
                sqlConnection.Open();
                sqlCommand.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Error on InsertNewCompany(Company company): " + ex.ToString());
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                }
            }
        }

        public void UpdateCompany(Company company)
        {
            sqlStatement = "UPDATE company SET login = @login, password = @pass WHERE id = @id;";
            sqlCommand = new SQLiteCommand(sqlStatement, sqlConnection);
            sqlCommand.Parameters.Add(new SQLiteParameter("@id", company.Id));
            sqlCommand.Parameters.Add(new SQLiteParameter("@name", company.Name));
            sqlCommand.Parameters.Add(new SQLiteParameter("@login", company.Login));
            sqlCommand.Parameters.Add(new SQLiteParameter("@pass", company.Password));

            try
            {
                sqlConnection.Open();
                sqlCommand.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Error on UpdateCompanyPassword(Company company): " + ex.ToString());
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                }
            }
        }
    }
}