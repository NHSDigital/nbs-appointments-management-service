Feature: Manage site reference details

  Scenario: Update reference details of a site
    Given The following sites exist in the system
      | Site                                 | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | Attributes                                                            | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-A | 1A New Lane | 0113 1111111 | 15N     | R1     | ICB1 | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true  | -60       | -60      |
    When I update the reference details for site 'beeae4e0-dd4a-4e3a-8f4d-738f9418fb51'
      | OdsCode     | ICB  | Region |
      | 16B         | ICB2 | R34    |
    Then the correct information for site 'beeae4e0-dd4a-4e3a-8f4d-738f9418fb51' is returned
      | Site                                 | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | Attributes                                                            | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-A | 1A New Lane | 0113 1111111 | 16B     | R34    | ICB2 | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true  | -60       | -60      |
