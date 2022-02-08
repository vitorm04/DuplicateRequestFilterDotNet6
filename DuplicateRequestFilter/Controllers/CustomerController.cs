using DuplicateRequestFilter.Models;
using Microsoft.AspNetCore.Mvc;

namespace DuplicateRequestFilter.Controllers
{
    [Route("api/customers")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private static ICollection<Customer> _customers = new List<Customer>();

        [HttpPost]
        public IActionResult AddCustomer([FromBody] Customer customer)
        {
            _customers.Add(customer);

            return Created(uri: String.Empty, customer);
        }
    }
}
