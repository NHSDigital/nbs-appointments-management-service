Feature: Get site meta data using site id

  Scenario: Retrieve site meta data by using site id
    Given the following default site exists in the system
      | Name   | Address      | PhoneNumber  | ODSCode | Region | ICB  | InformationForCitizens | Accessibilities                                     | Longitude | Latitude |
      | Site-A | 1A Site Lane | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | site_details/info_for_citizen=test information | -60       | -60      |
    When I request site meta data
    Then the correct site meta data is returned
      | SiteName | AdditionalInformation |
      | Site-A   | test information      |

  Scenario: Retrieve site meta data when it hasn't been set
    Given the following default site exists in the system
      | Name   | Address      | PhoneNumber  | ODSCode | Region | ICB  | InformationForCitizens | Accessibilities             | Longitude | Latitude |
      | Site-A | 1A Site Lane | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | def_one/attr_one=false | -60       | -60      |
    When I request site meta data
    Then no site meta data is returned
