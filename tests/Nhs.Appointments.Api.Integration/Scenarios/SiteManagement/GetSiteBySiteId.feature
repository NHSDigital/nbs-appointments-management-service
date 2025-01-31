Feature: Get site details using site id

  Scenario: Retrieve site information by using site id
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities             | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=true | -60       | -60      |
    When I request site details for site 'beeae4e0-dd4a-4e3a-8f4d-738f9418fb51'
    Then the correct site is returned
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities            | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=true | -60       | -60      |

  Scenario: Returns site not found message when site does not exist
    Given The site 'zero' does not exist in the system
    When I request site details for site 'zero'
    Then a message is returned saying the site is not found

  Scenario: Returns site not found message when querying by ODS code
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities            | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=true | -60       | -60      |
    When I request site details for site '15N'
    Then a message is returned saying the site is not found

