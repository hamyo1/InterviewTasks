using System.Data.Common;
using System.Data.SqlClient;

namespace ShekelIntegrationTest
{

    public class DbService : IDbService
    {
        readonly IConfiguration _configuration;
        readonly ILogger<DbService> _logger;



        public DbService(IConfiguration configuration, ILogger<DbService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<Group>> GetAllGroupsAndCustomers()
        {
            List<Group> groups = new List<Group>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ShekelTest")))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand("SELECT G.groupCode,G.groupName,C.[name], C.customerId FROM [dbo].[Groups] G LEFT JOIN [dbo].[FactoriesToCustomer] F on G.groupCode = F.groupCode LEFT JOIN [dbo].[Customers] C on F.customerId = C.customerId ORDER BY G.groupCode", connection))
                    {
                        DbDataReader reader = await command.ExecuteReaderAsync();

                        int preGroupCode = 0;
                        bool isFirst = true;
                        Group group = new Group();


                        while (await reader.ReadAsync())
                        {
                            if (reader["groupName"].GetType() == typeof(string) && !string.IsNullOrWhiteSpace((string)reader["groupName"])
                                && (int)reader["groupCode"] > 0)
                            {
                                if (isFirst)
                                {
                                    preGroupCode = (int)reader["groupCode"];

                                    group.groupName = (string)reader["groupName"];
                                    group.groupCode = (int)reader["groupCode"];
                                    group.customers = new List<Customer>();
                                    groups.Add(group);
                                    isFirst = false;
                                }

                                if (!isFirst && preGroupCode != (int)reader["groupCode"])
                                {
                                    preGroupCode = (int)reader["groupCode"];
                                    group = new Group();
                                    group.groupName = (string)reader["groupName"];
                                    group.groupCode = (int)reader["groupCode"];
                                    group.customers = new List<Customer>();
                                    groups.Add(group);

                                }



                                if (reader["name"].GetType() == typeof(string) && !string.IsNullOrWhiteSpace((string)reader["name"]) ||
                                    reader["customerId"].GetType() == typeof(string) && !string.IsNullOrWhiteSpace((string)reader["customerId"]))
                                {
                                    Customer customer = new Customer();
                                    customer.name = (string)reader["name"];
                                    customer.customerId = (string)reader["customerId"];
                                    group.customers.Add(customer);
                                }

                            }
                        }
                    }

                    await connection.CloseAsync();

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return groups;
        }

        public async Task<bool> InsertNewCustomer(NewCustomerRequest newCustomerRequest)
        {
            bool isSuccess = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ShekelTest")))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand("INSERT INTO [dbo].[Customers]([customerId], [name], [address], [phone]) VALUES(@customerId, @name, @address, @phone)", connection))
                    {
                        command.Parameters.AddWithValue("@customerId", newCustomerRequest.customerId);
                        command.Parameters.AddWithValue("@name", newCustomerRequest.name);
                        command.Parameters.AddWithValue("@address", newCustomerRequest.address);
                        command.Parameters.AddWithValue("@phone", newCustomerRequest.phone);
                        await command.ExecuteReaderAsync();
                    }
                    await connection.CloseAsync();

                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand("INSERT INTO [dbo].[FactoriesToCustomer]([groupCode],[factoryCode],[customerId]) VALUES (@groupCode,@factoryCode ,@customerId)", connection))
                    {
                        command.Parameters.AddWithValue("@groupCode", newCustomerRequest.groupCode);
                        command.Parameters.AddWithValue("@factoryCode", newCustomerRequest.factoryCode);
                        command.Parameters.AddWithValue("@customerId", newCustomerRequest.customerId);
                        await command.ExecuteReaderAsync();
                    }

                    await connection.CloseAsync();

                }
                isSuccess = true;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                isSuccess = false;
            }


            return isSuccess;
        }
    }
}
