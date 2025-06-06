  Scenario: Update details of a site
    Given The following sites exist in the system
      | Site                                 | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                      | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-A | 1A New Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true | -2.3      | 50.1     |
    When I update the details for site 'beeae4e0-dd4a-4e3a-8f4d-738f9418fb51'
      | Name   | Address     | PhoneNumber  | Longitude | Latitude |
      | Site-B | 2B New Lane | 011322222222 | -4.1      | 58.5     |
    Then the correct information for site 'beeae4e0-dd4a-4e3a-8f4d-738f9418fb51' is returned
      | Site                                 | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                      | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-B | 2B New Lane | 011322222222 | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true | -4.1      | 58.5     |

  Scenario: Update details of a site with an valid phone number with spaces
    Given The following sites exist in the system
      | Site                                 | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                      | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-A | 1A New Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true | -2.3      | 50.1     |
    When I update the details for site 'beeae4e0-dd4a-4e3a-8f4d-738f9418fb51'
      | Name   | Address     | PhoneNumber  | Longitude | Latitude |
      | Site-B | 2B New Lane | 0113 2222222 | -4.1      | 58.5     |
    Then the correct information for site 'beeae4e0-dd4a-4e3a-8f4d-738f9418fb51' is returned
      | Site                                 | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                      | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-B | 2B New Lane | 0113 2222222 | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true | -4.1      | 58.5     |

  Scenario: Update details of a site with an empty phone number
  Given The following sites exist in the system
  | Site                                 | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                      | Longitude | Latitude |
  | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-A | 1A New Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true | -2.3      | 50.1     |
  When I update the details for site 'beeae4e0-dd4a-4e3a-8f4d-738f9418fb51'
  | Name   | Address     | PhoneNumber  | Longitude | Latitude |
  | Site-B | 2B New Lane |              | -4.1      | 58.5     |
  Then the correct information for site 'beeae4e0-dd4a-4e3a-8f4d-738f9418fb51' is returned
  | Site                                 | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                      | Longitude | Latitude |
  | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-B | 2B New Lane |              | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true | -4.1      | 58.5     |

  Scenario: Update details of a site with an invalid phone number
    Given The following sites exist in the system
      | Site                                 | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                      | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-A | 1A New Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true | -2.3      | 50.1     |
    When I update the details for site 'beeae4e0-dd4a-4e3a-8f4d-738f9418fb51'
      | Name   | Address     | PhoneNumber   | Longitude | Latitude |
      | Site-B | 2B New Lane | 0113 1111111f | -2.3      | 50.1     |
    Then a bad request response is returned with the following error messages
      | Phone number must contain numbers and spaces only |

  Scenario: Update details of a site with an invalid phone number
    Given The following sites exist in the system
      | Site                                 | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                      | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-A | 1A New Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true | -2.3      | 50.1     |
    When I update the details for site 'beeae4e0-dd4a-4e3a-8f4d-738f9418fb51'
      | Name   | Address     | PhoneNumber  | Longitude | Latitude |
      | Site-B | 2B New Lane | abcdefg12345 | -2.3      | 50.1     |
    Then a bad request response is returned with the following error messages
      | Phone number must contain numbers and spaces only |

  Scenario: Update details of a site with an empty name and an invalid phone number
    Given The following sites exist in the system
      | Site                                 | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                      | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-A | 1A New Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true | -2.3      | 50.1     |
    When I update the details for site 'beeae4e0-dd4a-4e3a-8f4d-738f9418fb51'
      | Name | Address     | PhoneNumber  | Longitude | Latitude |
      |      | 2B New Lane | abcdefg12345 | -2.3      | 50.1     |
    Then a bad request response is returned with the following error messages
      | Provide a valid name | Phone number must contain numbers and spaces only |

  Scenario: Update details of a site with all info invalid
    Given The following sites exist in the system
      | Site                                 | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                      | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-A | 1A New Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true | -2.3      | 50.1     |
    When I update the details for site 'beeae4e0-dd4a-4e3a-8f4d-738f9418fb51'
      | Name | Address | PhoneNumber | Longitude | Latitude  |
      |      |         |             | -50.45.34 | dsfsdfdsf |
    Then a bad request response is returned with the following error messages
  | Provide a valid name | Provide a valid address | Latitude must be a decimal number | Longitude must be a decimal number |


  Scenario: Returns site not found message when site does not exist
    Given The site 'zero' does not exist in the system
    When I update the details for site 'zero'
      | Name   | Address     | PhoneNumber  | Longitude | Latitude |
      | Site-B | 2B New Lane | 011322222222 | -2.3      | 50.1     |
    Then a message is returned saying the site is not found

  Scenario: Returns site not found message when site update attempted with ODS code
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities       | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=true | -2.3      | 50.1     |
    When I update the details for site '15N'
      | Name   | Address     | PhoneNumber  | Longitude | Latitude |
      | Site-B | 2B New Lane | 011322222222 | -2.3      | 50.1     |
    Then a message is returned saying the site is not found
