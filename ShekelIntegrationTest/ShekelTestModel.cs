using System.ComponentModel.DataAnnotations;

namespace ShekelIntegrationTest
{
    public class Group
    {
        public string groupName { get; set; } 
        public int groupCode { get; set; } 

        public List<Customer> customers { get; set; }
    }    
    
    public class Customer
    {
        public string phone { get; set; } 
        public string address { get; set; } 
        public string name { get; set; } 
        public string customerId { get; set; } 
    }

    public class NewCustomerRequest
    {
        [Required(ErrorMessage = "{0} is required", AllowEmptyStrings = false)]
        public string name { get; set; }

        [Required(ErrorMessage = "{0} is required", AllowEmptyStrings = false)]
        public string phone { get; set; }        
        
        [Required(ErrorMessage = "{0} is required", AllowEmptyStrings = false)]
        public string customerId { get; set; }
        public string address { get; set; }
        public int groupCode { get; set; }
        public int factoryCode { get; set; }

    }
}
