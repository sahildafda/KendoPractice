using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DataBringer.Controllers
{
    public class DatabaseEntity
    { 
        public int DatabaseID { get; set; }
        public string DatabaseName { get; set; }
        public List<TableEntity> AllTables{ get; set; }
    }

    public class TableEntity
    {
        public string TableName { get; set; }
        public string SchemaName { get; set; }
        public string DatabaseName { get; set; }
    }


    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {

        public string connectionString = @"Data Source=LAPTOP-AFUSNNHR;Initial Catalog=RishitsDatabase;Integrated Security=True";

        // GET: api/<DatabaseController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<DatabaseController>/5
        [HttpGet("GetTables/{database_name}")]
        public List<TableEntity> GetTables(string database_name)
        {
            return GetTablesFromDatabase(database_name);
        }

        private List<TableEntity> GetTablesFromDatabase(string database)
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


        [HttpGet("GetDatabases")]
        public List<DatabaseEntity> GetDatabases()
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


        // POST api/<DatabaseController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<DatabaseController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<DatabaseController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
