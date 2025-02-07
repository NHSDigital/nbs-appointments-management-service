Feature: Get site meta data using site id

  Scenario: Retrieve site meta data by using site id
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | ODSCode | Region | ICB  | InformationForCitizens | Accessibilities                                     | Longitude | Latitude |
      | 6877d86e-c2df-4def-8508-e1eccf0ea6be | Site-A | 1A Site Lane | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | site_details/info_for_citizen=test information | -60       | -60      |
    When I request site meta data for site '6877d86e-c2df-4def-8508-e1eccf0ea6be'
    Then the correct site meta data is returned
      | SiteName | AdditionalInformation |
      | Site-A   | test information      |

  Scenario: Retrieve site meta data when it hasn't been set
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | ODSCode | Region | ICB  | InformationForCitizens | Accessibilities             | Longitude | Latitude |
      | 6877d86e-c2df-4def-8508-e1eccf0ea6be | Site-A | 1A Site Lane | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | def_one/attr_one=false | -60       | -60      |
    When I request site meta data for site '6877d86e-c2df-4def-8508-e1eccf0ea6be'
    Then no site meta data is returned
