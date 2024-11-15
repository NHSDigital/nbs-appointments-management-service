Feature: Manage site attributes

  Scenario: Add attribute to a site
    Given The following sites exist in the system
      | Site | Name   | Address     | PhoneNumber  | Attributes | Longitude | Latitude |
      | A    | Site-A | 1A New Lane | 0113 1111111 | __empty__  | -60       | -60      |
    When I update the attributes for site 'A'
      | Attributes                                                           |
      | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true |
    Then the correct information for site 'A' is returned
      | Site | Name   | Address     | PhoneNumber  | Attributes                                                           | Longitude | Latitude |
      | A    | Site-A | 1A New Lane | 0113 1111111 | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true | -60       | -60      |

  Scenario: Switch off attributes for a site
    Given The following sites exist in the system
      | Site | Name   | Address      | PhoneNumber  | Attributes                                                           | Longitude | Latitude |
      | B    | Site-B | 1B Park Lane | 0113 1111111 | def_one/attr_one=true, def_one/attr_two=true, def_two/attr_one=true  | -60       | -60      |
    When I update the attributes for site 'B'
      | Attributes                                                             |
      | def_one/attr_one=false, def_one/attr_two=false, def_two/attr_one=false |
    Then the correct information for site 'B' is returned
      | Site | Name   | Address      | PhoneNumber  | Attributes                                                             | Longitude | Latitude |
      | B    | Site-B | 1B Park Lane | 0113 1111111 | def_one/attr_one=false, def_one/attr_two=false, def_two/attr_one=false | -60       | -60      |

  Scenario: Returns site not found message when site does not exist
    Given The site 'zero' does not exist in the system
    When I update the attributes for site 'zero'
      | Attributes                                                           |
      | def_one/attr_one=false |
    Then a message is returned saying the site is not found