## Integration Test Ideas
* Create API
    * Test Case #1
        * Send a valid create request to make a new employee object, ensure that it gets a 200 back to verify successful creation. Verify that the object added to the database has all the right metadata that was submitted in the original request.
    * Test Case #2
        * Send an invalid create request by adding an extra field in the request that doesn't follow the model. Ensure that it gets a 400 bad request back. Verify that trying to do a read on the employeeID you submitted in the original request returns nothing.
* Read API
    * Test Case #1
        * Try to read an employee object that actually exists, verify that it gets a 200 back.
    * Test Case #2
        * Try to read an employee object that does not exist. Ensure error 404 not found is returned.
* Update API
    * Test Case #1
        * Try updating an object the doesn’t exist, ensure a 400 bad request is returned.Verify no new object is created by running read on the employeeID of the previous request.
    * Test Case #2
        * Try updating an object that exists correctly (like by updating the levels field), ensure a 200 gets returned. Verify the update was applied correctly by doing a read on the object and double checking the levels field.
    * Test Case #3
        * Try updating a field that doesn’t exist, ensure it a 400 bad request is returned. Verify no updates were done to the object by calling read on it and double checking no fields were updated.
* Delete API
    * Test Case #1
        * Try to call delete on an object that doesn’t exist, ensure it gets a 400 bad request back.
    * Test Case #2
        * Try to delete an object that does exist, ensure it gets a 200 back. Verify trying to read the employeeID of the object you deleted returns a 404 not found back.
