using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ShekelIntegrationTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ShekelIntegrationTestController : ControllerBase
    {

        readonly ILogger<ShekelIntegrationTestController> _logger;
        readonly IDbService _dbService;

        public ShekelIntegrationTestController(ILogger<ShekelIntegrationTestController> logger, IDbService dbService)
        {
            _logger = logger;
            _dbService = dbService;
        }

        [Route("GetAllGroupsAndCustomers")]
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]

        public async Task<IActionResult> GetAllGroupsAndCustomers()
        {
            _logger.LogInformation("GetAllGroupsAndCustomers api contrroler is get a call");

            List<Group> groups = await _dbService.GetAllGroupsAndCustomers();

            if(groups != null && groups.Count > 0)
            {
                return Ok(groups);
            }
            return Ok("empty results!");
        }


        [Route("InsertNewCustomer")]
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]

        public async Task<IActionResult> InsertNewCustomer([FromQuery] NewCustomerRequest newCustomerRequest)
        {
            _logger.LogInformation("InsertNewCustomer api contrroler is get a call");

            if( await _dbService.InsertNewCustomer(newCustomerRequest))
            {
                return Ok();
            }
            return StatusCode(500);
        }
    }
}