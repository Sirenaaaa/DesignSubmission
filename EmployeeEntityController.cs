namespace employeeDatabase.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using employeeDatabase.Models;

    public class EmployeeEntityController : Controller
    {
        private readonly ICosmosDbService _cosmosDbService;
        public EmployeeEntityController(ICosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }

        // Public facing Create API
        [HttpPost]
        public async Task<ActionResult> CreateAsync([Bind("employeeID,startDate,level,isIndividualContributor,isManager")] EmployeeEntity employeeEntity)
        {
            if (ModelState.IsValid)
            {
                // Create the object
                employeeEntity.employeeID = Guid.NewGuid().ToString();
                await _cosmosDbService.CreateEmployeeEntityAsync(employeeEntity);
            else
            {
                // Log bad input error
                return BadRequest(employeeEntity);
            }
        }

        // Public facing Read API
        [HttpGet]
        public async Task<IActionResult> ReadAsync(string employeeID)
        {
            return Ok(await _cosmosDbService.ReadEmployeeEntityAsync(employeeID));
        }

        // Public facing Update API
        [HttpPost]
        public async Task<ActionResult> EditAsync([Bind("employeeID,startDate,level,isIndividualContributor,isManager")] EmployeeEntity employeeEntity)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _cosmosDbService.UpdateEmployeeEntityAsync(employeeEntity.employeeID, employeeEntity));
            }
            else
            {
                // Log bad input error
                return BadRequest(employeeEntity);
            }
        }

        // Public facing Delete API
        [HttpDelete]
        public async Task<ActionResult> DeleteAsync(string employeeID)
        {
            if (id == null)
            {
                // Log bad input error
                return BadRequest();
            }
            
            await _cosmosDbService.DeleteEmployeeEntityAsync(employeeID);
            return NoContent();
        }
    }
}
