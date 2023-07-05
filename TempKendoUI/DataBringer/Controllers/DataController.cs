using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Text.Json;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DataBringer.Controllers
{
    public class Brand
    { 
        public string Brand_ID { get; set; }
        public string Brand_Name { get; set; }
        public string Brand_Type { get; set; }
        public string status { get; set; }
    }


    [EnableCors]
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private string connectionString = @"Data Source=LAPTOP-AFUSNNHR;Initial Catalog=RishitsDatabase;Integrated Security=True";
        
        [HttpGet("GetAnyTableData/{databaseName},{schema},{tableName}")]
        public List<Dictionary<string, object>> GetAnyTableData(string databaseName,string schema,string tableName)
        {
            List<Dictionary<string, object>> tableData = new List<Dictionary<string, object>>();

            string query = $"SELECT top 200 * FROM [{databaseName}].[{schema}].[{tableName}]";

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

                                row[columnName] = value;
                            }

                            tableData.Add(row);
                        }
                    }
                }
            }

            return tableData;
        }
    

    // GET: api/<DataController>
    [HttpGet]
        public string Get()
        {
            string Result = "";
            using (SqlConnection connection = new SqlConnection(@"Data Source=LAPTOP-AFUSNNHR;Initial Catalog=RishitsDatabase;Integrated Security=True"))
            { 
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"declare @var varchar(max) = (
                                            select * from Production.brands for json auto
                                            );
                                            select @var;
                                            ";
                    Result = command.ExecuteScalar().ToString();
                }
                connection.Close();
            }
            return Result;
        }

        // GET api/<DataController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<DataController>
        [HttpPost("AddBrand")]
        public void Post([FromBody] Brand brand)
        {
            using (SqlConnection connection = new SqlConnection(@"Data Source=LAPTOP-AFUSNNHR;Initial Catalog=RishitsDatabase;Integrated Security=True"))
            {   
                string query = "INSERT INTO production.brands (Brand_Name,Brand_Type,status) VALUES (@BrandName,@BrandType,@status)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@BrandName", SqlDbType.VarChar).Value = brand.Brand_Name;
                    command.Parameters.Add("@BrandType", SqlDbType.VarChar).Value = brand.Brand_Type;
                    command.Parameters.Add("@status", SqlDbType.VarChar).Value = brand.status;

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        // PUT api/<DataController>/5
        [HttpPut("UpdateBrand/{id}")]
        public void Put(int id, [FromBody] Brand brand)
        {
            string connectionString = @"Data Source=LAPTOP-AFUSNNHR;Initial Catalog=RishitsDatabase;Integrated Security=True";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "UPDATE production.brands SET Brand_Name = @BrandName,Brand_Type=@BrandType,status=@status WHERE Brand_ID = @BrandID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@BrandName", SqlDbType.VarChar).Value = brand.Brand_Name;
                    command.Parameters.Add("@BrandType", SqlDbType.VarChar).Value = brand.Brand_Type;
                    command.Parameters.Add("@status", SqlDbType.VarChar).Value = brand.status;

                    command.Parameters.Add("@BrandID", SqlDbType.Int).Value = brand.Brand_ID;

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

        }

        // DELETE api/<DataController>/5
        [HttpDelete("DeleteBrand/{id}")]
        public void Delete(int id)
        {
            string connectionString = @"Data Source=LAPTOP-AFUSNNHR;Initial Catalog=RishitsDatabase;Integrated Security=True";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "delete from production.brands WHERE Brand_ID = @BrandID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@BrandID", SqlDbType.Int).Value = id;

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
