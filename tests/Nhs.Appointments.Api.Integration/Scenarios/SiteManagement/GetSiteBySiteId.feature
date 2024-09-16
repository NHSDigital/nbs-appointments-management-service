Feature: Get site details using site id
  
  Scenario: Retrieve site information by using site id 
    Given The following sites exist in the system
      | Site  | Name       | Address     | AttributeName    | AttributeValue | Longitude | Latitude | 
      | WXY01 | Site-WXY01 | 1 Park Lane | def_one/attr_one | true           | -60       | -60      |
    When I request site details for site 'WXY01' 
    Then the correct site is returned
      | Site  | Name       | Address     | AttributeName    | AttributeValue | Longitude | Latitude |
      | WXY01 | Site-WXY01 | 1 Park Lane | def_one/attr_one | true           | -60       | -60      |
