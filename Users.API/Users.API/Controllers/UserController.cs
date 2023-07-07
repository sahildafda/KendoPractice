using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Users.API.Models;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Cors;
using System.Data;
using System.Text.Json;
using System.Collections.Specialized;
using System.Collections;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Users.API.Controllers
{

    public class DatabaseEntity
    {
        public int DatabaseID { get; set; }
        public string DatabaseName { get; set; }
        public List<TableEntity> AllTables { get; set; }
    }

    public class TableEntity
    {
        public string TableName { get; set; }
        public string SchemaName { get; set; }
        public string DatabaseName { get; set; }
    }

    public class Column
    {
        public string Field { get; set; }
        public string Title { get; set; }
    }


    [EnableCors]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        static string connectionString = "Data Source=DESKTOP-CL2VPGO;Initial Catalog=DBLearning;Integrated Security=True";

        SqlConnection connection = new SqlConnection("Data Source=DESKTOP-CL2VPGO;Initial Catalog=DBLearning;Integrated Security=True");

        // POST api/<DatabaseController>
        [HttpPost("TableUpdater")]
        public string TableUpdater([FromBody] UpdaterEntity data)
        {
            try
            {
                // update statement 
                string query = $"UPDATE {data.TableName} SET ";
                List<string> setters = new List<string>();
                // add set fields
                foreach (DictionaryEntry entry in data.updated_data)
                {
                    string key = entry.Key.ToString();
                    string value = GetValueAsString(entry.Value.ToString().Replace("'", "''"));
                    setters.Add($" {key} = {value} ");
                }
                setters.RemoveAt(0);
                query += string.Join(" , ", setters);
                // add where clause
                query += " WHERE ";
                List<string> Conditionals = new List<string>();
                foreach (DictionaryEntry entry in data.old_data)
                {
                    string key = entry.Key.ToString();
                    string value = GetValueAsString(entry.Value.ToString().Replace("'", "''"));
                    if (value != "'N/A'")
                        Conditionals.Add($" {key} = {value} ");
                    else
                        Conditionals.Add($" {key} is null ");
                }
                query += string.Join(" AND ", Conditionals);
                QueryExecutor(query);
                return query;
            }
            catch (Exception error)
            {
                return null;
            }
        }



        // POST api/<DatabaseController>
        [HttpPost("TableDeleter")]
        public string TableDeleter([FromBody] DeleterEntity data)
        {
            try
            {
                // update statement 
                string query = $"Delete from {data.TableName} where ";

                foreach (DictionaryEntry entry in data.old_data)
                {
                    string key = entry.Key.ToString();
                    string value = GetValueAsString(entry.Value.ToString().Replace("'", "''"));
                    query += $"{key} = {value} AND ";
                }

                // remove last "AND"
                query = query.TrimEnd(" AND ".ToCharArray());
                QueryExecutor(query);
                return query;
            }
            catch (Exception error)
            {
                return null;
            }
        }



        [HttpPost("TableInserter")]
        public string TableInserter([FromBody] InserterEntity data)
        {
            try
            {
                // insert statement with columns 
                string query = $"insert into {data.TableName} ( ";
                List<string> keys = new List<string>();
                // add values
                foreach (object entry in data.updated_data.Keys)
                    keys.Add(entry.ToString());

                keys.RemoveAt(0);
                query += string.Join(" , ", keys);
                query += " ) values ( ";
                List<string> values = new List<string>();
                // add values
                foreach (object entry in data.updated_data.Values)
                {
                    string value = GetValueAsString(entry.ToString().Replace("'", "''"));
                    values.Add(value);
                }
                values.RemoveAt(0);
                query += string.Join(" , ", values) + " ) ";
                QueryExecutor(query);
                return query;
            }
            catch (Exception error)
            {
                return null;
            }
        }

        private JsonValueKind GetValueKind(object value)
        {
            JsonDocument document = JsonDocument.Parse(value.ToString());
            JsonElement root = document.RootElement;
            return root.ValueKind;
        }

        // Helper method to convert values to string and add single quotations for strings
        private string GetValueAsString(object value)
        {
            try
            {
                if(GetValueKind(value) is string)
                    return $"'{value}'";


                int i = Convert.ToInt32(value.ToString());
                return $"{value}";
            }
            catch (Exception error)
            {
                return $"'{value}'";
            }
        }



        [HttpGet("getcols/{tableDetails}")]
        public List<string> getColumns(string tableDetails)
        {
            List<string> columns = new List<string>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = $"select top 1 * from {tableDetails}";
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            for (int i = 0; i < reader.FieldCount; i++)
                                columns.Add(reader.GetName(i));
                    }
                }
            }
            return columns;
        }


        private void QueryExecutor(string Query)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = Query;
                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }
            }
            catch (Exception error) { }
        }
        public class UpdaterEntity
        {
            public string TableName { get; set; }
            public OrderedDictionary old_data { get; set; }
            public OrderedDictionary updated_data { get; set; }
        }



        public class InserterEntity
        {
            public string TableName { get; set; }
            public OrderedDictionary updated_data { get; set; }
        }



        public class DeleterEntity
        {
            public string TableName { get; set; }
            public OrderedDictionary old_data { get; set; }
        }

        [HttpGet("GetAnyTableData/{tabelDetails}")]
        public string GetAnyTableData(string tabelDetails)
        {
            string tableData="";
            string query = $"declare @var varchar(max) = ( select top 200 * from {tabelDetails} for json auto) ; select @var; ";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    tableData = command.ExecuteScalar().ToString();
                }
            }

            return tableData;
        }

        [HttpGet("GetFullTableData/{table_details}")]
        public List<Dictionary<string, object>> GetFullTableData(string table_details)
        {
            try
            {
                List<Dictionary<string, object>> tableData = new List<Dictionary<string, object>>();

                string query = $"SELECT * FROM {table_details}";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, object> row = new Dictionary<string, object>();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string columnName = reader.GetName(i);
                                    object value = reader.GetValue(i);
                                    if (value == null || value == DBNull.Value)
                                        row[columnName] = "N/A";
                                    else
                                        row[columnName] = value;
                                }

                                tableData.Add(row);
                            }
                        }
                    }
                }

                return tableData;
            }
            catch (Exception error) { }

        }

        [HttpGet("GetTableColumns/{table_details}")]
        public  List<Column> GetTableColumns(string table_details)
        {
            try
            {
                List<Column> columns = new List<Column>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Execute the sp_help stored procedure to retrieve the column information
                    using (SqlCommand command = new SqlCommand($"exec sp_help '{table_details}'", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Skip the first result set, which contains general table information
                            reader.NextResult();

                            // Read the second result set, which contains column information
                            while (reader.Read())
                            {
                                string columnName = reader.GetString(0);

                                // Create a Column object and add it to the list
                                Column column = new Column
                                {
                                    Field = columnName,
                                    Title = columnName
                                };

                                columns.Add(column);
                            }
                        }
                    }
                }

                return columns;
            }
            catch (Exception error) { }
        }



        // GET api/<DatabaseController>/5
        [HttpGet("GetTables/{database_name}")]
        public List<TableEntity> GetTables(string database_name)
        {
            return GetTablesFromDatabase(database_name);
        }

        private List<TableEntity> GetTablesFromDatabase(string database)
        {
            try
            {
                List<TableEntity> tables = new List<TableEntity>();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = $"use {database};SELECT TABLE_NAME,TABLE_CATALOG,TABLE_SCHEMA FROM INFORMATION_SCHEMA.TABLES;use master;";
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                            tables.Add(new TableEntity()
                            {
                                TableName = reader["TABLE_NAME"].ToString(),
                                DatabaseName = reader["TABLE_CATALOG"].ToString(),
                                SchemaName = reader["TABLE_SCHEMA"].ToString()
                            });

                        reader.Close();
                    }
                    connection.Close();
                }
                return tables;
            }
            catch (Exception error) { }
        }


        [HttpGet("GetDatabases")]
        public List<DatabaseEntity> GetDatabases()
        {
            try
            {
                List<DatabaseEntity> tables = new List<DatabaseEntity>();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"SELECT name,database_id FROM sys.databases;";
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                            tables.Add(new DatabaseEntity()
                            {
                                DatabaseName = reader["name"].ToString(),
                                DatabaseID = Convert.ToInt32(reader["database_ID"]),
                                AllTables = GetTablesFromDatabase(reader["name"].ToString())
                            });

                        reader.Close();
                    }
                    connection.Close();
                }
                return tables;
            }
            catch (Exception error) { }
        }









        // GET: api/<UserController>
        [HttpGet]
        public IEnumerable<User> GetUsers()
        {
            connection.Open();

            List<User> userList = new List<User>();

            var cmd = new SqlCommand("select * from tblUsers;",connection);

            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                userList.Add(new User() { Username = reader["Username"].ToString(), Password = reader["Password"].ToString(), UserId = Convert.ToInt32(reader["UserId"]), UserType = reader["UserType"].ToString() });
            }

            connection.Close();
            return userList;
        }

        // GET api/<UserController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<UserController>
        [HttpPost]
        public void Post([FromBody] User user)
        {
            connection.Open();

            SqlCommand cmd = new SqlCommand($"insert into tblUsers (username, password, userType) values ('{user.Username}', '{user.Password}', '{user.UserType}')", connection);

            cmd.ExecuteNonQuery();

            connection.Close();
        }

        // PUT api/<UserController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] User user)
        {
            connection.Open();

            SqlCommand cmd = new SqlCommand($"update tblUsers set username = '{user.Username}', password = '{user.Password}', userType = '{user.UserType}' where userId = {id}", connection);
            cmd.ExecuteNonQuery();

            connection.Close();
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            connection.Open();

            SqlCommand cmd = new SqlCommand($"delete from tblUsers where userId = {id}", connection);
            cmd.ExecuteNonQuery();

            connection.Close();
        }
    }
}
