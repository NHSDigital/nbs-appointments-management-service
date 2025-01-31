Feature: Manage site attributes

  Scenario: Add attribute to a site
    Given The following sites exist in the system
      | Site                                 | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-A | 1A New Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | __empty__  | -60       | -60      |
    When I update the attributes for site 'beeae4e0-dd4a-4e3a-8f4d-738f9418fb51'
      | Accessibilities                                                           |
      | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true |
    Then the correct information for site 'beeae4e0-dd4a-4e3a-8f4d-738f9418fb51' is returned
      | Site                                 | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                           | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-A | 1A New Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true | -60       | -60      |

  Scenario: Switch off attributes for a site
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                          | Longitude | Latitude |
      | 5914b64a-66bb-4ee2-ab8a-94958c1fdfcb | Site-B | 1B Park Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=true, def_one/attr_two=true, def_two/attr_one=true | -60       | -60      |
    When I update the attributes for site '5914b64a-66bb-4ee2-ab8a-94958c1fdfcb'
      | Accessibilities                                                             |
      | def_one/attr_one=false, def_one/attr_two=false, def_two/attr_one=false |
    Then the correct information for site '5914b64a-66bb-4ee2-ab8a-94958c1fdfcb' is returned
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                             | Longitude | Latitude |
      | 5914b64a-66bb-4ee2-ab8a-94958c1fdfcb | Site-B | 1B Park Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=false, def_one/attr_two=false, def_two/attr_one=false | -60       | -60      |

  Scenario: Returns site not found message when site does not exist
    Given The site 'zero' does not exist in the system
    When I update the attributes for site 'zero'
      | Accessibilities             |
      | def_one/attr_one=false |
    Then a message is returned saying the site is not found

  Scenario: Returns site not found message when site update attempted with ODS code
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities            | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=true | -60       | -60      |
    When I update the attributes for site '15N'
      | Accessibilities             |
      | def_one/attr_one=false |
    Then a message is returned saying the site is not found
