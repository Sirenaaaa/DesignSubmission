## Use Cases, Constrains, and Assumptions


### Use Cases



* Support CRUD operations against billions of employee entities through publicly available endpoints
    * User can interact with 4 publicly available APIs to perform CRUD operations on the database.
        * Create a new employee entity
        * Read an employee entity stored in the database
        * Update an existing employee entity
        * Delete an existing employee entity
* Service efficiently handles high RPS (reads per second)


### Constraints (Out of Scope Items)



* Authentication won’t be required for users to interact with the database
* No protection against common attack vectors like DDoS attacks - all traffic is assumed to be legitimate
* Employee entities won’t be organized under a tenant ID or employer ID within the database
* No special handling of user data replication to comply with data privacy/ethics laws like EUDB. All data can be replicated across any geo region regardless of where that data was generated from
* No UI implementation - the service is backend only


### Assumptions



* Traffic will span the globe
* Most of the workload will happen during business days/hours 
* Most users will be querying for data that is within the geo region they are in
* RPS = reads per second
* Writes to the database will be much more infrequent compared to reads
* There are no use cases where a user wants to create a new object by calling the update API
    * Prevent edge cases where an employee entity is deleted and someone tries to perform an update on that item. When the backend sees the update is for an object that doesn’t exist, we don’t want to recreate it. 
* There are no use cases where a user wants to create separate employee entities that share the same employeeID
* Database doesn’t need to be ACID (atomicity, consistency, isolation, durability) compliant. Users can wait for a reasonable amount of time (we can define a SLA - service level agreement - here) for eventual consistency to happen in the database.


## High Level Architecture Diagrams






![alt_text](https://github.com/Sirenaaaa/EmployeeMicroserviceDesignQuestion/blob/main/Visual.png)



### Diagram with Implementation Using Azure Services




![alt_text](https://github.com/Sirenaaaa/EmployeeMicroserviceDesignQuestion/blob/main/VisualWithMSComponents.png)



## Core Components - Controller, Cache, and Database


### Database

We have two main options for implementing a database for the model: SQL and NoSQL.



* SQL
    * Pros
        * Strong data consistency and integrity
            * We won’t have to worry about users getting stale data back from the database.  
        * ACID (atomicity, consistency, isolation, durability) compliant - provides best guarantee for data accuracy 
            * Important updates (like employee termination or hiring) are reflected immediately without needing to wait for eventual consistency from the database.
    * Cons
        * Data needs to be structured and organized into a rigid schema 
            * This will limit how we can model the employee entity object in the database. It will also make it difficult to change our model if we need to for future feature work. 
        * Doesn’t support horizontal scaling well, vertical scaling needed instead
            * Horizontal scaling is critical to providing a service with the flexibility needed to cater to fluctuations in traffic effectively. There can be large variations in traffic during work days/hours versus outside of them. Without efficient horizontal scaling, we can’t scale out to meet periods of high traffic, and during low traffic periods, being unable to scale back in will drive up service costs.
        * Connections between tables in large databases will negatively impact query speed
            * If we want to expand our database to one day support organizing employee entities under a tenant ID or employer ID, this will negatively impact the database’s efficiency.  
* NoSQL
    * Pros
        * Supports horizontal scalability well
            * We can automatically scale out during the business hours, then scale back in when traffic drops outside of those times.
        * Good for large/frequently changing databases 
            * Our database is not write heavy, so we don’t expect frequent changes to it. However, we do have a large database that encompasses billions of employee entity objects. 
        * Fast query speed even in large databases
            * Crucial for handling traffic that generates high reads per second in a large database. 
    * Cons
        * Complex queries negatively impact speed 
            * We only offer four queries in the form of CRUD APIs, so the complexity is limited.
        * Data retrieval inconsistency 
            * Stale data shouldn’t be a common issue when the database is mostly supporting reads, not writes. Even in cases where writes are sent to the database, we don’t anticipate cases where a user is unable to wait for the eventual consistency to kick in if they do see stale data. 

High availability, scalability, and flexibility are crucial to meeting our customer needs. We cannot sacrifice those needs for improved data consistency and integrity. Our database is read, not write, heavy, and we do not store data that needs to be ACID compliant. Therefore, a noSQL database will be the best choice.

A possible choice for the noSQL database would be to use Azure CosmosDB noSQL. 



* Autoscaling 
* Multi region write to replicate the database across multiple geo regions
* Backup and restoration of the database
    * Can be used to support soft deletion of customer data. Useful if an employer is required to retain employee records for a certain amount of time before hard deletion.


### Cache

Implementing a cache is important for efficiency and preventing our service from being throttled by the database. Caching is beneficial for usage patterns like ours that are read heavy and write light. It will improve our performance, scalability, and availability significantly. 

Important points to consider when using a cache:



* Must ensure our service doesn’t go down if the cache becomes unavailable
    * The database is still our main source of truth, even with a cache
    * If the cache is unavailable, the service should fall back to connecting directly with the database until the cache comes online again
* Managing eventual consistency with the cache
    * Using a noSQL database means the data our service uses might not be totally consistent all the time
    * We need to have a cache expiration policy to prevent data in the cache from becoming too stale

Azure offers a redis cache that we can use to store recently accessed data.   


### Controller

The controller will include a web app to implement the 4 publicly exposed CRUD APIs users call to interact with the employee entity database. The web app uses interfaces to pass user requests to the model, our database, where the employee entities are stored, and responses from the database back to the user. To efficiently handle high reads per second, the controller will include a load balancer and autoscaler. 

The load balancer will:



* Spread out traffic using round robin to  the VMs (virtual machines) that the web app is deployed on 

The autoscaler will:



* Dynamically scale out and in the number of VMs based on fluctuations in user traffic
    * We can configure the autoscaler to scale out more aggressively during business hours when we anticipate the most traffic
    * During non business hours, weekends, and holidays, we can configure the autoscaler to scale out more conservatively, since we’re unlikely to see heavy usage during this time
    * Paying attention to CPU load percentage and memory usage metrics would be helpful in building autoscale policies that let us detect changes in load. 
        * Ex. If CPU load percentage is over 70% for the last 30 minutes, spin up 10 more VMs. If CPU load percentage drops below 50% for the last 30 minutes, spin down 5 VMs. 
        * We can set baseline minimum and maximum VM thresholds we don’t want to cross
            * Ex. don’t drop below 10 machines or go over 50 machines

Along the way, logs and metrics will be collected from:



* API calls
    * Track whether the CRUD calls are successful or not
    * Record what errors or exceptions failed calls ran into
* VMs
    * Machine metadata like memory usage and CPU load that can be useful for autoscaling
* Web app 
    * Unexpected errors and exceptions that could come from:
        * Issues connecting to the database
        * Issues sending data back to the client
        * Invalid user input
* Cache
    * Keep track of the cache hit and miss rate 
    * See how many requests get forwarded to the database 

Telemetry will be stored in a separate database outside the employee entity one. 

A possible implementation of the controller would be to:



* Use ASP.NET Core Web API to:
    * Create the web app for connecting to the Azure CosmosDB noSQL database
    * Create the 4 public facing CRUD APIs
    * Implement all the necessary interfaces for communicating with CosmosDB
* Use Azure App Service to:
    * Publish the web app (get it on VMs)
    * Create an autoscale policy
    * Implement a round robin load balancer
* Use open telemetry to:
    * Collect telemetry from API calls, VMs, and the web app
    * Publish that telemetry to a special database
