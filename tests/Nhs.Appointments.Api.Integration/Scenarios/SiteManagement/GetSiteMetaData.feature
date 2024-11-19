Feature: Get site meta data using site id
  
  Scenario: Retrieve site meta data by using site id 
    Given The following sites exist in the system
      | Site | Name   | Address      | PhoneNumber  | Region | ICB  | Attributes                                     | Longitude | Latitude |
      | A    | Site-A | 1A Site Lane | 0113 1111111 | R1     | ICB1 | site_details/info_for_citizen=test information | -60       | -60      |
    When I request site meta data for site 'A'
    Then the correct site meta data is returned
      | Site   | AdditionalInformation |
      | Site-A | test information      |

  Scenario: Retrieve site meta data when it hasn't been set
    Given The following sites exist in the system
      | Site | Name   | Address      | PhoneNumber  | Region | ICB  | Attributes             | Longitude | Latitude |
      | A    | Site-A | 1A Site Lane | 0113 1111111 | R1     | ICB1 | def_one/attr_one=false | -60       | -60      |
    When I request site meta data for site 'A'
    Then no site meta data is returned