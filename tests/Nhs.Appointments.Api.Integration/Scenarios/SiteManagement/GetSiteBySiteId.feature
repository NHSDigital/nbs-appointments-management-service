Feature: Get site details using site id
  
  Scenario: Retrieve site information by using site id 
    Given The following sites exist in the system
      | Site | Name   | Address      | PhoneNumber  | Attributes            | Longitude | Latitude |
      | A    | Site-A | 1A Site Lane | 0113 1111111 |def_one/attr_one=true | -60       | -60      |
    When I request site details for site 'A' with the scope '*'
    Then the correct site is returned
      | Site | Name   | Address      | PhoneNumber  | Attributes            | Longitude | Latitude |
      | A    | Site-A | 1A Site Lane | 0113 1111111 |def_one/attr_one=true | -60       | -60      |

  Scenario: Returns site not found message when site does not exist
    Given The site 'zero' does not exist in the system
    When I request site details for site 'zero' with the scope '*'
    Then a message is returned saying the site is not found

  Scenario: Returns site with filtered attributes
    Given The following sites exist in the system
      | Site | Name   | Address      | PhoneNumber  | Attributes                                      | Longitude | Latitude |
      | A    | Site-A | 1A Site Lane | 0113 1234567 |def_one/attr_one=true, test_scope/attr_two=test | -60       | -60      |
    When I request site details for site 'A' with the scope 'test_scope'
    Then the correct site is returned
      | Site | Name   | Address      | PhoneNumber  | Attributes               | Longitude | Latitude |
      | A    | Site-A | 1A Site Lane | 0113 1234567 | test_scope/attr_two=test | -60       | -60      |