Feature: Manage site attributes

  Scenario: Add attribute to a site
    Given The following sites exist in the system
      | Site  | Name       | Address      | Attributes | Longitude | Latitude |
      | ABC01 | Site-ABC01 | 1A Park Lane | __empty__  | -60       | -60      |
    When I update the following attributes for site 'ABC01'
      | Attributes                                                           |
      | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true |
    Then the correct information for site 'ABC01' is returned
      | Site  | Name       | Address      | Attributes                                                           | Longitude | Latitude |
      | ABC01 | Site-ABC01 | 1A Park Lane | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true | -60       | -60      |

  Scenario: Switch off attributes for a site
    Given The following sites exist in the system
      | Site  | Name       | Address      | Attributes                                                           | Longitude | Latitude |
      | ABC02 | Site-ABC02 | 1B Park Lane | def_one/attr_one=true, def_one/attr_two=true, def_two/attr_one=true  | -60       | -60      |
    When I update the following attributes for site 'ABC02'
      | Attributes                                                           |
      | def_one/attr_one=false, def_one/attr_two=false, def_two/attr_one=false |
    Then the correct information for site 'ABC02' is returned
      | Site  | Name       | Address      | Attributes                                                             | Longitude | Latitude |
      | ABC02 | Site-ABC02 | 1B Park Lane | def_one/attr_one=false, def_one/attr_two=false, def_two/attr_one=false | -60       | -60      |

  Scenario: Returns site not found message when site does not exist
    Given The site 'zero' does not exist in the system
    When I request site details for site 'zero'
    Then a message is returned saying the site is not found
      | Site  | Name       | Address     | Attributes            | Longitude | Latitude |
      | WXY01 | Site-WXY01 | 1 Park Lane | def_one/attr_one=true | -60       | -60      |