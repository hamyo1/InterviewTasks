using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
using DatabaseWrapper;
using DatabaseWrapper.Core;
using ExpressionTree;
using System.Data.SqlClient;
using MySqlX.XDevAPI.Common;

namespace ShekelIntegrationTest
{
    public class DatabaseWrapper : IDbService
    {
        readonly IConfiguration _configuration;
        readonly IConfigurationSection _SqlConfigSection;
        readonly ILogger<DatabaseWrapper> _logger;



        public DatabaseWrapper(IConfiguration configuration, ILogger<DatabaseWrapper> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _SqlConfigSection = configuration.GetSection("SqlConnectionStrings");
        }

        public async Task<List<Group>> GetAllGroupsAndCustomers()
        {
            List<Group> groups = new List<Group>();

            try
            {
                //DatabaseClient client = new DatabaseClient(DbTypeEnum.Mysql, "[hostname]", "[port]", "[user]", "[password]", "[databasename]");
                using (DatabaseClient client = new DatabaseClient(DbTypeEnum.SqlServer, _SqlConfigSection["HostName"], int.Parse(_SqlConfigSection["Port"])
                    , null, null, _SqlConfigSection["DatabaseName"]))
                {
                    DataTable result = await client.QueryAsync("SELECT G.groupCode,G.groupName,C.[name], C.customerId FROM [dbo].[Groups] G LEFT JOIN [dbo].[FactoriesToCustomer] F on G.groupCode = F.groupCode LEFT JOIN [dbo].[Customers] C on F.customerId = C.customerId ORDER BY G.groupCode");

                    if (result.Rows.Count > 0 && result.Rows[0]["groupCode"] != null && result.Rows[0]["groupName"].GetType() == typeof(string)
                        && !string.IsNullOrWhiteSpace((string)result.Rows[0]["groupName"]))
                    {
                        Group group = new Group();
                        group.groupCode = (int)result.Rows[0]["groupCode"];
                        group.groupName = (string)result.Rows[0]["groupName"];
                        group.customers = new List<Customer>();
                        groups.Add(group);

                        foreach (DataRow row in result.Rows)
                        {
                            if (row["groupName"].GetType() == typeof(string) && !string.IsNullOrWhiteSpace((string)row["groupName"])
                                                   && row["groupCode"] != null && (int)row["groupCode"] > -1
                                                   && (int)row["groupCode"] != group.groupCode)
                            {
                                group = new Group();
                                group.groupCode = (int)row["groupCode"];
                                group.groupName = (string)row["groupName"];
                                group.customers = new List<Customer>();
                                groups.Add(group);
                            }
                            if (row["name"].GetType() == typeof(string) && !string.IsNullOrWhiteSpace((string)row["name"]) ||
                                row["customerId"].GetType() == typeof(string) && !string.IsNullOrWhiteSpace((string)row["customerId"]))
                            {
                                Customer customer = new Customer();
                                customer.name = (string)row["name"];
                                customer.customerId = (string)row["customerId"];
                                group.customers.Add(customer);
                            }
                        }
                    }
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
                using (DatabaseClient client = new DatabaseClient(DbTypeEnum.SqlServer, _SqlConfigSection["HostName"], int.Parse(_SqlConfigSection["Port"])
                                                        , null, null, _SqlConfigSection["DatabaseName"]))
                {
                    string query = $"INSERT INTO [dbo].[Customers]([customerId], [name], [address], [phone]) " +
                        $"VALUES({newCustomerRequest.customerId},'{newCustomerRequest.name}', '{newCustomerRequest.address}', {newCustomerRequest.phone})";

                    DataTable result = await client.QueryAsync(query);

                    string query2 = $"INSERT INTO[dbo].[FactoriesToCustomer] ([groupCode],[factoryCode],[customerId]) " +
                        $"VALUES({newCustomerRequest.groupCode}, {newCustomerRequest.factoryCode}, {newCustomerRequest.customerId})";

                    DataTable result2 = await client.QueryAsync(query2);
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
