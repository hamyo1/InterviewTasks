namespace ShekelIntegrationTest
{
    public interface IDbService
    {
        public Task<List<Group>> GetAllGroupsAndCustomers();
        public Task<bool> InsertNewCustomer(NewCustomerRequest newCustomerRequest);
    }
}
