Feature: Manage site reference details

  Scenario: Update reference details of a site
    And the following default site exists in the system
      | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                             | Longitude | Latitude |
      | Site-A | 1A New Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true  | -60       | -60      |
    When I update the reference details at the default site
      | OdsCode     | ICB  | Region |
      | 16B         | ICB2 | R34    |
    Then the correct information for the default site is returned
      | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                             | Longitude | Latitude |
      | Site-A | 1A New Lane | 0113 1111111 | 16B     | R34    | ICB2 | Info 1                 | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true  | -60       | -60      |

  Scenario: Update reference details of a site - lastUpdatedBy property updates
    Given the following default site exists in the system
      | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                             | Longitude | Latitude |
      | Site-A | 1A New Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true  | -60       | -60      |
    #Verify default setup
    And the default site document has lastUpdatedBy 'api@test'
    And I register and use a http client with details
      | User Id    | Role                         | Scope  |
      | mya_admin  | system:integration-test-user | global |
    When I update the reference details at the default site
      | OdsCode     | ICB  | Region |
      | 16B         | ICB2 | R34    |
    Then the correct information for the default site is returned
      | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                             | Longitude | Latitude |
      | Site-A | 1A New Lane | 0113 1111111 | 16B     | R34    | ICB2 | Info 1                 | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true  | -60       | -60      |
    And the default site document has lastUpdatedBy 'api@mya_admin'

  Scenario: Update with empty reference details of a site
    Given the following default site exists in the system
      | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                             | Longitude | Latitude |
      | Site-A | 1A New Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true  | -60       | -60      |
    When I update the reference details at the default site
      | OdsCode     | ICB  | Region |
      |             |      |        |
    Then a bad request response is returned with the following error messages
      | Provide a valid ODS code | Provide a valid ICB | Provide a valid region |
