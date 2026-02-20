Feature: Manage site accessibilities

  Scenario: Add accessibility to a site
    Given the following sites exist in the system
      | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities | Longitude | Latitude |
      | Site-A | 1A New Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | __empty__       | -60       | -60      |
    When I update the accessibilities at the default site
      | Accessibilities                                                           |
      | def_one/accessibility_one=true, def_one/accessibility_two=false, def_two/accessibility_one=true |
    Then the correct information for the default site is returned
      | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                                                 | Longitude | Latitude |
      | Site-A | 1A New Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/accessibility_one=true, def_one/accessibility_two=false, def_two/accessibility_one=true | -60       | -60      |

  Scenario: Add accessibility to a site - lastUpdatedBy property updates
    Given the following sites exist in the system
      | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities | Longitude | Latitude |
      | Site-A | 1A New Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | __empty__       | -60       | -60      |
    #Verify default setup
    And the default site document has lastUpdatedBy 'api@test'
    And I register and use a http client with details
      | User Id  | Role                         | Scope  |
      | mya_user | system:integration-test-user | global |
    When I update the accessibilities at the default site
      | Accessibilities                                                           |
      | def_one/accessibility_one=true, def_one/accessibility_two=false, def_two/accessibility_one=true |
    Then the correct information for the default site is returned
      | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                                                 | Longitude | Latitude |
      | Site-A | 1A New Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/accessibility_one=true, def_one/accessibility_two=false, def_two/accessibility_one=true | -60       | -60      |
    And the default site document has lastUpdatedBy 'api@mya_user'
    
  Scenario: Switch off accessibilities for a site
    Given the following sites exist in the system
      | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                                                | Longitude | Latitude |
      | Site-B | 1B Park Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/accessibility_one=true, def_one/accessibility_two=true, def_two/accessibility_one=true | -60       | -60      |
    When I update the accessibilities at the default site
      | Accessibilities                                                             |
      | def_one/accessibility_one=false, def_one/accessibility_two=false, def_two/accessibility_one=false |
    Then the correct information for the default site is returned
      | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                                                   | Longitude | Latitude |
      | Site-B | 1B Park Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/accessibility_one=false, def_one/accessibility_two=false, def_two/accessibility_one=false | -60       | -60      |

  Scenario: Returns site not found message when site does not exist
    Given The site 'zero' does not exist in the system
    When I update the accessibilities at the default site
      | Accessibilities                 |
      | def_one/accessibility_one=false |
    Then a message is returned saying the site is not found

  Scenario: Returns site not found message when site update attempted with ODS code
    Given the following sites exist in the system
      | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                | Longitude | Latitude |
      | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/accessibility_one=true | -60       | -60      |
    When I update the accessibilities for ODS code '15N'
      | Accessibilities             |
      | def_one/accessibility_one=false |
    Then a message is returned saying the site is not found
