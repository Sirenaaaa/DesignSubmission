namespace employeeDatabase.Models
{
    using Newtonsoft.Json;
     
    // Model to represent an employee entity object
    public class EmployeeEntity
    {
      // employeeID is the partition key of the database
      [JsonProperty(PropertyName = "employeeID")]
      public string employeeID {get; set;}
    
      [JsonProperty(PropertyName = "startDate")]
      public string startDate {get; set;}
    
      [JsonProperty(PropertyName = "level")]
      public int level {get; set;}
    
      [JsonProperty(PropertyName = "isIndividualContributor")]
      public bool isIndividualContributor {get; set;}
    
      [JsonProperty(PropertyName = "isManager")]
      public bool isManager {get; set;}
    }
}
