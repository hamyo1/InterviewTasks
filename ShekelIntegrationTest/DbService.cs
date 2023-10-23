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
                        int index = 0;
                        Customer customer = new Customer();
                        Group group = new Group();
                        group.customers = new List<Customer>();


                        while (await reader.ReadAsync())
                        {
                            if (reader["groupName"].GetType() == typeof(string) && !string.IsNullOrWhiteSpace((string)reader["groupName"])
                                && (int)reader["groupCode"] > 0)
                            {


                                if (index != 0 && preGroupCode != (int)reader["groupCode"])
                                {
                                    groups.Add(group);
                                    preGroupCode = group.groupCode;
                                    group = new Group();
                                    group.customers = new List<Customer>();
                                }
                                else if (index == 0)
                                {
                                    preGroupCode = (int)reader["groupCode"];
                                }

                                group.groupName = (string)reader["groupName"];
                                group.groupCode = (int)reader["groupCode"];

                                if (reader["name"].GetType() == typeof(string) && !string.IsNullOrWhiteSpace((string)reader["name"]) ||
                                    reader["customerId"].GetType() == typeof(string) && !string.IsNullOrWhiteSpace((string)reader["customerId"]))
                                {
                                    customer.name = (string)reader["name"];
                                    customer.customerId = (string)reader["customerId"];

                                    group.customers.Add(customer);
                                    customer = new Customer();
                                }

                            }
                            index++;
                        }

                        if ((groups.Count == 0 && preGroupCode != 0) || (groups.Count > 0 && preGroupCode != group.groupCode))
                        {
                            groups.Add(group);
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
