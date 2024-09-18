Feature: Get site details using site id
  
  Scenario: Retrieve site information by using site id 
    Given The following sites exist in the system
      | Site | Name   | Address      | Attributes            | Longitude | Latitude | 
      | A    | Site-A | 1A Site Lane | def_one/attr_one=true | -60       | -60      |
    When I request site details for site 'A' 
    Then the correct site is returned
      | Site | Name   | Address      | Attributes            | Longitude | Latitude |
      | A    | Site-A | 1A Site Lane | def_one/attr_one=true | -60       | -60      |

  Scenario: Returns site not found message when site does not exist
    Given The site 'zero' does not exist in the system
    When I request site details for site 'zero'
    Then a message is returned saying the site is not found