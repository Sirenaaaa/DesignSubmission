namespace employeeDatabase
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using todo.Models;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Fluent;
    using Microsoft.Extensions.Configuration;

    public class CosmosDbService : ICosmosDbService
    {
        private Container _container;

        public CosmosDbService(
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        // Create API that talks to cosmosDB
        public async Task CreateEmployeeEntityAsync(EmployeeEntity employeeEntity)
        {
            await this._container.CreateItemAsync<EmployeeEntity>(employeeEntity, new PartitionKey(employeeEntity.employeeID));
        }

        // Read API that talks to cosmosDB
        public async Task<EmployeeEntity> ReadEmployeeEntityAsync(string employeeID)
        {
            try
            {
                // Try to retreive item from cache before going to database
                // Pretend cache has been implemented
                EmployeeEntity employeeEntity = await cache.StringGetAsync(employeeEntity.employeeID);

                // If there is a cache miss, update the cache
                // Call the database directly instead
                if (employeeEntity == null)
                {
                    await StringSetAsync(employeeEntity.employeeID, employeeEntity);
                    ItemResponse<EmployeeEntity> response = await this._container.ReadItemAsync<EmployeeEntity>(employeeID, new PartitionKey(employeeID));
                    return response.Resource;
                }
                else
                {
                    return employeeEntity;
                }
            }
            // EmployeeID doesn't exist
            catch(CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            { 
                // Add logging here to capture the exception
                return null;
            }
        }

        // Upadte API that talks to cosmosDB
        public async Task UpdateEmployeeEntityAsync(string employeeID, EmployeeEntity employeeEntity)
        {
            // Check if the item you're trying to update doesn't exist
            // Only do the update if the item already exists
            try
            {
                ItemResponse<EmployeeEntity> response = await this._container.ReadItemAsync<EmployeeEntity>(employeeID, new PartitionKey(employeeID));
                await this._container.UpsertItemAsync<EmployeeEntity>(employeeEntity, new PartitionKey(employeeID));
            }
            // Item doesn't exist, don't update
            catch(CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            { 
                // Add logging here to capture the exception
                return null;
            }
            
        }

        // Delete API that talks to cosmosDB
        public async Task DeleteEmployeeEntityAsync(string employeeID)
        {
            await this._container.DeleteItemAsync<EmployeeEntity>(employeeID, new PartitionKey(employeeID));
        }      
    }
}
