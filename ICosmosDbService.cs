namespace employeeDatabase
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using todo.Models;

    public interface ICosmosDbService
    {
        Task CreateEmployeeEntityAsync(EmployeeEntity employeeEntity);
        Task<EmployeeEntity> ReadEmployeeEntityAsync(string employeeID);
        Task UpdateEmployeeEntityAsync(string employeeID, EmployeeEntity employeeEntity);
        Task DeleteEmployeeEntityAsync(string employeeID);
    }
}
