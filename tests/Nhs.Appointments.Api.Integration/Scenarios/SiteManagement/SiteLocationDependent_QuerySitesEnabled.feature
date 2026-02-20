Feature: Site Location Dependent - Query Sites Enabled

  Scenario: Query Sites By Type and ODS Code
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Type         | IsDeleted |
      | 238d42d1-8628-43d1-8180-76345e82e471 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | 0ecd3590-c7ae-4673-8fb4-3614c3cb0b61 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 | Pharmacy     | false     |
      | 06b1be0d-c170-4f6c-95c8-4a5629631031 | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | -0.13086317 | 51.583479 | PCN Site     | false     |
      | a5088094-0519-442b-9e19-e795fe7deb2e | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | Another Type | false     |
    When I query sites by site type and ODS code
      | Types    | OdsCode | Longitude | Latitude | SearchRadius |
      | phArMaCy | K12     | 0.082     | 51.5     | 6000         |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude  | Latitude  | Distance  | Type     |
      | 0ecd3590-c7ae-4673-8fb4-3614c3cb0b61 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false  | 0.14566747 | 51.482472 | 4819      | Pharmacy |

  Scenario: Query Sites by accessibilities
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                | Longitude   | Latitude  | Type         | IsDeleted |
      | 0d89d01c-b6aa-47c3-8800-38b8ba3917d5 | Site-1 | 1 Roadside | 0113 1111111 | ODS1    | R1     | ICB1 | Info 1                 | accessibility/attr_three=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | baa74fcb-a5b6-434e-9915-14e9743f95d9 | Site-2 | 2 Roadside | 0113 2222222 | ODS2    | R2     | ICB2 | Info 2                 | accessibility/attr_one=false   | 0.14566747  | 51.482472 | Pharmacy     | false     |
      | b5d6918d-e674-4e0a-a095-e48352f07053 | Site-3 | 3 Roadside | 0113 3333333 | ODS3    | R3     | ICB3 | Info 3                 | accessibility/attr_one=false   | -0.13086317 | 51.583479 | PCN Site     | false     |
      | 8daeea45-0ac2-424b-a6f0-8d770c31d145 | Site-4 | 4 Roadside | 0113 4444444 | ODS4    | R4     | ICB4 | Info 4                 | accessibility/attr_three=true  | 0.040992272 | 51.455788 | Another Type | false     |
    When I query sites by access needs
      | Longitude | Latitude | SearchRadius | AccessNeeds |
      | 0.082     | 51.5     | 6000         | attr_three |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude   | Distance  | Type         |
      | 0d89d01c-b6aa-47c3-8800-38b8ba3917d5 | Site-1 | 1 Roadside | 0113 1111111 | ODS1    | R1     | ICB1 | Info 1                 | accessibility/attr_three=true | 0.082750916 | 51.494056  | 662       | GP Practice  |
      | 8daeea45-0ac2-424b-a6f0-8d770c31d145 | Site-4 | 4 Roadside | 0113 4444444 | ODS4    | R4     | ICB4 | Info 4                 | accessibility/attr_three=true | 0.040992272 | 51.455788  | 5677      | Another Type |

  Scenario: Query Sites by service and availability - Sites returned
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                | Longitude   | Latitude  | Type         | IsDeleted |
      | 27b5b656-31d1-44e4-9851-f38c2a0dcc05 | Site-1 | 1 Roadside | 0113 1111111 | ODS1    | R1     | ICB1 | Info 1                 | accessibility/attr_three=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | 7c08e6ba-25b4-4154-95e6-a76b8a92809d | Site-2 | 2 Roadside | 0113 2222222 | ODS2    | R2     | ICB2 | Info 2                 | accessibility/attr_one=false   | 0.14566747  | 51.482472 | Pharmacy     | false     |
      | 2bff8018-e99b-4625-ae10-e699bdc42d4e | Site-3 | 3 Roadside | 0113 3333333 | ODS3    | R3     | ICB3 | Info 3                 | accessibility/attr_one=false   | -0.13086317 | 51.583479 | PCN Site     | false     |
      | c5b2c4de-3ed5-4f46-bd6d-c4ba7658fa5c | Site-4 | 4 Roadside | 0113 4444444 | ODS4    | R4     | ICB4 | Info 4                 | accessibility/attr_three=true  | 0.040992272 | 51.455788 | Another Type | false     |
    And the following sessions exist for existing site '27b5b656-31d1-44e4-9851-f38c2a0dcc05'
      | Date        | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult | 5           | 1        |
    And the following sessions exist for existing site '7c08e6ba-25b4-4154-95e6-a76b8a92809d'
      | Date        | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:5_6  | 5           | 1        |
    And the following sessions exist for existing site '2bff8018-e99b-4625-ae10-e699bdc42d4e'
      | Date        | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:2_3  | 5           | 1        |
    And the following sessions exist for existing site 'c5b2c4de-3ed5-4f46-bd6d-c4ba7658fa5c'
      | Date        | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult | 5           | 1        |
    When I query sites by service
      | Longitude | Latitude | SearchRadius | Services  | From     | Until    |
      | 0.082     | 51.5     | 6000         | RSV:Adult | Tomorrow | Tomorrow |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Distance | Type          |
      | 27b5b656-31d1-44e4-9851-f38c2a0dcc05 | Site-1 | 1 Roadside | 0113 1111111 | ODS1    | R1     | ICB1 | Info 1                 | accessibility/attr_three=true | 0.082750916 | 51.494056 | 662      | GP Practice  |
      | c5b2c4de-3ed5-4f46-bd6d-c4ba7658fa5c | Site-4 | 4 Roadside | 0113 4444444 | ODS4    | R4     | ICB4 | Info 4                 | accessibility/attr_three=true | 0.040992272 | 51.455788 | 5677     | Another Type |

  Scenario: Query Sites by service and availability - No sites returned
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                | Longitude   | Latitude  | Type         | IsDeleted |
      | 79d5fae3-0988-457b-9b2a-c8a8596e5ac3 | Site-1 | 1 Roadside | 0113 1111111 | ODS1    | R1     | ICB1 | Info 1                 | accessibility/attr_three=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | c2848ba3-8dca-45ca-bacf-4f1769fff8dc | Site-2 | 2 Roadside | 0113 2222222 | ODS2    | R2     | ICB2 | Info 2                 | accessibility/attr_one=false   | 0.14566747  | 51.482472 | Pharmacy     | false     |
      | e448fcf0-7e0a-4c28-ac64-bcf64fa0c33a | Site-3 | 3 Roadside | 0113 3333333 | ODS3    | R3     | ICB3 | Info 3                 | accessibility/attr_one=false   | -0.13086317 | 51.583479 | PCN Site     | false     |
      | 7d4e4c69-5ea7-4e19-a069-bd776df2c491 | Site-4 | 4 Roadside | 0113 4444444 | ODS4    | R4     | ICB4 | Info 4                 | accessibility/attr_three=true  | 0.040992272 | 51.455788 | Another Type | false     |
    And the following sessions exist for existing site '79d5fae3-0988-457b-9b2a-c8a8596e5ac3'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID:5-11 | 5           | 1        |
    And the following sessions exist for existing site 'c2848ba3-8dca-45ca-bacf-4f1769fff8dc'
      | Date        | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:5-6  | 5           | 1        |
    And the following sessions exist for existing site 'e448fcf0-7e0a-4c28-ac64-bcf64fa0c33a'
      | Date        | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:2-3  | 5           | 1        |
    And the following sessions exist for existing site '7d4e4c69-5ea7-4e19-a069-bd776df2c491'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID:5-11 | 5           | 1        |
    When I query sites by service
      | Longitude | Latitude | SearchRadius | Services  | From              | Until             |
      | 0.082     | 51.5     | 6000         | RSV:Adult | 2 days from today | 3 days from today |
    Then no sites are returned

  Scenario: Query Sites by multiple filters - Returns all sites
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Type         | IsDeleted |
      | 60d5105e-eb83-4a99-8f75-0a8790583233 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | 5972cb6a-b9bb-4e6e-9a40-53e574d22c7a | Site-2 | 2 Roadside | 0113 2222222 | ODSB    | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.14566747  | 51.482472 | Pharmacy     | false     |
      | 30f48c6e-c65f-48a5-a219-1e073a3a7001 | Site-3 | 3 Roadside | 0113 3333333 | ODSB    | R3     | ICB3 | Info 3                 | accessibility/attr_four=false | -0.13086317 | 51.583479 | Pharmacy     | false     |
      | daa8f2a1-c682-49ff-8d15-b721bdac730a | Site-4 | 4 Roadside | 0113 4444444 | ODSD    | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.040992272 | 51.455788 | Another Type | false     |
    When I query sites by multiple filters
      | Longitude | Latitude | SearchRadius | AccessNeeds | Types    | OdsCode |
      | 0.082     | 51.5     | 6000         | attr_four   | Pharmacy | ODSB    |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Distance | Type          |
      | 60d5105e-eb83-4a99-8f75-0a8790583233 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.082750916 | 51.494056 | 662      | GP Practice  |
      | 5972cb6a-b9bb-4e6e-9a40-53e574d22c7a | Site-2 | 2 Roadside | 0113 2222222 | ODSB    | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.14566747  | 51.482472 | 4819     | Pharmacy     |
      | 30f48c6e-c65f-48a5-a219-1e073a3a7001 | Site-3 | 3 Roadside | 0113 3333333 | ODSB    | R3     | ICB3 | Info 3                 | accessibility/attr_four=false | -0.13086317 | 51.583479 | 17402    | Pharmacy     |
      | daa8f2a1-c682-49ff-8d15-b721bdac730a | Site-4 | 4 Roadside | 0113 4444444 | ODSD    | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.040992272 | 51.455788 | 5677     | Another Type |

  Scenario: Query Sites does not return soft deleted sites
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Type         | IsDeleted |
      | 4739abc0-5eca-4b2f-8960-ada8f2081837 | Site-1 | 1 Roadside | 0113 1111111 | ODS123  | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.282750916 | 53.494056 | GP Practice  | false     |
      | 51641717-91a3-40d3-8d28-ac591c32b4ab | Site-2 | 2 Roadside | 0113 2222222 | ODS321  | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.24566747  | 53.482472 | Pharmacy     | true      |
      | 6fadf09d-5e7d-4262-a0c6-5467dba3bc83 | Site-3 | 3 Roadside | 0113 3333333 | ODS456  | R3     | ICB3 | Info 3                 | accessibility/attr_four=false | -0.03086317 | 53.583479 | Pharmacy     | true      |
      | d815ea3d-c614-4150-a9b7-f78dd1df853c | Site-4 | 4 Roadside | 0113 4444444 | ODS654  | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.240992272 | 53.455788 | Another Type | false     |
    When I query sites by location
      | Longitude | Latitude | SearchRadius |
      | 0.282     | 53.5     | 6000         |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Distance | Type          |
      | 4739abc0-5eca-4b2f-8960-ada8f2081837 | Site-1 | 1 Roadside | 0113 1111111 | ODS123  | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.282750916 | 53.494056 | 662      | GP Practice  |
      | d815ea3d-c614-4150-a9b7-f78dd1df853c | Site-4 | 4 Roadside | 0113 4444444 | ODS654  | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.240992272 | 53.455788 | 5615     | Another Type |

  Scenario: Query sites excludes specific site types
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Type         | IsDeleted |
      | d62690d3-9104-490f-a1bd-57545c7ea70b | Site-1 | 1 Roadside | 0113 1111111 | ODS123  | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.382750916 | 54.494056 | GP Practice  | false     |
      | 6016be1e-1b33-4322-a743-ae90a6595838 | Site-2 | 2 Roadside | 0113 2222222 | ODS123  | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.34566747  | 54.482472 | Pharmacy     | false     |
      | cb031bf3-1e93-4756-ab72-cbe2d3fedf2d | Site-3 | 3 Roadside | 0113 3333333 | ODS123  | R3     | ICB3 | Info 3                 | accessibility/attr_four=false | -0.23086317 | 54.583479 | Pharmacy     | false     |
      | 59bae202-3de1-4a57-8aa8-ac9dde083b1b | Site-4 | 4 Roadside | 0113 4444444 | ODS123  | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.340992272 | 54.455788 | Another Type | false     |
    When I query sites by site type and ODS code
      | Types    | OdsCode | Longitude | Latitude | SearchRadius |
      | !Pharmacy | ODS123 | 0.382     | 54.5     | 6000         |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Distance | Type          |
      | d62690d3-9104-490f-a1bd-57545c7ea70b | Site-1 | 1 Roadside | 0113 1111111 | ODS123  | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.382750916 | 54.494056 | 662      | GP Practice  |
      | 59bae202-3de1-4a57-8aa8-ac9dde083b1b | Site-4 | 4 Roadside | 0113 4444444 | ODS123  | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.340992272 | 54.455788 | 5584     | Another Type |

  Scenario: Query Sites By Type and ODS Code - 400 response due to invalid parameters
    Given the following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
      | 2686db3e-18df-416d-82f0-b758fc0d92dd | Site-B | 1B Site Lane | 0113 1111112 | 20N     | R2     | ICB2 | Info 2                 | accessibility/attr_one=true  | -60       | -60      | Pharmacy    |
      | a010ae04-98b5-4df5-aa57-9c27673ca854 | Site-C | 1C Site Lane | 0113 1111113 | 25N     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | -60       | -60      | GP Practice |
    When I query sites by site type and ODS code
      | Types    | OdsCode | Longitude | Latitude | SearchRadius |
      | Pharmacy | 20N     | -260      | -260     | 100          |
    Then the call should fail with 400

  Scenario: Query Sites only returns the maxRecords number of sites
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Type         | IsDeleted |
      | 60d5105e-eb83-4a99-8f75-0a8790583233 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | 5972cb6a-b9bb-4e6e-9a40-53e574d22c7a | Site-2 | 2 Roadside | 0113 2222222 | ODSB    | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.14566747  | 51.482472 | Pharmacy     | false     |
      | 30f48c6e-c65f-48a5-a219-1e073a3a7001 | Site-3 | 3 Roadside | 0113 3333333 | ODSB    | R3     | ICB3 | Info 3                 | accessibility/attr_four=false | -0.13086317 | 51.583479 | Pharmacy     | false     |
      | daa8f2a1-c682-49ff-8d15-b721bdac730a | Site-4 | 4 Roadside | 0113 4444444 | ODSD    | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.040992272 | 51.455788 | Another Type | false     |
    When I query sites with a low maxRecord count
      | Longitude | Latitude | SearchRadius | MaxRecords |
      | 0.082     | 51.5     | 6000         | 2          |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Distance | Type          |
      | 60d5105e-eb83-4a99-8f75-0a8790583233 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.082750916 | 51.494056 | 662      | GP Practice  |
      | 5972cb6a-b9bb-4e6e-9a40-53e574d22c7a | Site-2 | 2 Roadside | 0113 2222222 | ODSB    | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.14566747  | 51.482472 | 4819     | Pharmacy     |

  Scenario: Location-based radius filtering – Within, On Boundary, and Outside Radius
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude | Latitude  | Type         | IsDeleted |
      | d9a9a6e8-2f75-4343-9c92-303f0d044b48 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.100000  | 51.500000 | GP Practice  | false     |
      | 3e9ab683-9afa-4525-9062-f26391ee67ef | Site-2 | 2 Roadside | 0113 2222222 | ODSB    | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.150000  | 51.500000 | Pharmacy     | false     |
      | cf75bfd6-22b9-464c-a5f4-eef580149b90 | Site-3 | 3 Roadside | 0113 3333333 | ODSB    | R3     | ICB3 | Info 3                 | accessibility/attr_four=false | 0.300000  | 51.500000 | Pharmacy     | false     |
    When I query sites by location
      | Longitude | Latitude  | SearchRadius |
      | 0.100000  | 51.500000 | 3460         |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude | Latitude  | Distance | Type         |
      | d9a9a6e8-2f75-4343-9c92-303f0d044b48 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.100000  | 51.500000 | 0        | GP Practice  |
      | 3e9ab683-9afa-4525-9062-f26391ee67ef | Site-2 | 2 Roadside | 0113 2222222 | ODSB    | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.150000  | 51.500000 | 3460     | Pharmacy     |

  Scenario: Query Sites handles priority ordering on filters
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Type         | IsDeleted |
      | 8184c730-b4e6-45af-999a-f67569eb4367 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | 83599e38-41cf-4f22-8a12-0e84b861cd1b | Site-2 | 2 Roadside | 0113 2222222 | ODSB    | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.14566747  | 51.482472 | Pharmacy     | false     |
      | 1993855c-1dba-4ec8-8a09-4096c05ccecb | Site-3 | 3 Roadside | 0113 3333333 | ODSB    | R3     | ICB3 | Info 3                 | accessibility/attr_four=false | -0.13086317 | 51.583479 | Pharmacy     | false     |
      | a3133ea7-9b3b-4264-acc0-2a955eb996b4 | Site-4 | 4 Roadside | 0113 4444444 | ODSD    | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.040992272 | 51.455788 | Another Type | false     |
    When I query sites by multiple filters in reverse priority order
      | Longitude | Latitude | SearchRadius | AccessNeeds | Types    | MaxRecords |
      | 0.082     | 51.5     | 60000        | attr_four   | Pharmacy | 2          |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude | Latitude    | Distance | Type     |
      | 83599e38-41cf-4f22-8a12-0e84b861cd1b | Site-2 | 2 Roadside | 0113 2222222 | ODSB    | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.14566747  | 51.482472 | 4819     | Pharmacy |
      | 1993855c-1dba-4ec8-8a09-4096c05ccecb | Site-3 | 3 Roadside | 0113 3333333 | ODSB    | R3     | ICB3 | Info 3                 | accessibility/attr_four=false | -0.13086317 | 51.583479 | 17402    | Pharmacy |

  Scenario: Query Sites filters on multiple services
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Type         | IsDeleted |
      | 8184c730-b4e6-45af-999a-f67569eb4367 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | 83599e38-41cf-4f22-8a12-0e84b861cd1b | Site-2 | 2 Roadside | 0113 2222222 | ODSB    | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.14566747  | 51.482472 | Pharmacy     | false     |
      | 1993855c-1dba-4ec8-8a09-4096c05ccecb | Site-3 | 3 Roadside | 0113 3333333 | ODSB    | R3     | ICB3 | Info 3                 | accessibility/attr_four=false | -0.13086317 | 51.583479 | Pharmacy     | false     |
      | a3133ea7-9b3b-4264-acc0-2a955eb996b4 | Site-4 | 4 Roadside | 0113 4444444 | ODSD    | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.040992272 | 51.455788 | Another Type | false     |
    And the following sessions exist for existing site '8184c730-b4e6-45af-999a-f67569eb4367'
      | Date        | From  | Until | Services             | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID:5_11,RSV:Adult | 5           | 1        |
    And the following sessions exist for existing site '83599e38-41cf-4f22-8a12-0e84b861cd1b'
      | Date        | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:5-6  | 5           | 1        |
    And the following sessions exist for existing site '1993855c-1dba-4ec8-8a09-4096c05ccecb'
      | Date        | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:2-3  | 5           | 1        |
    And the following sessions exist for existing site 'a3133ea7-9b3b-4264-acc0-2a955eb996b4'
      | Date        | From  | Until | Services             | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID:5_11,RSV:Adult | 5           | 1        |
    When I query sites by service
      | Longitude | Latitude | SearchRadius | Services             | From     | Until    |
      | 0.082     | 51.5     | 6000         | RSV:Adult,COVID:5_11 | Tomorrow | Tomorrow |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance | Type         |
      | 8184c730-b4e6-45af-999a-f67569eb4367 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true | 0.082750916 | 51.494056 | 662      | GP Practice  |
      | a3133ea7-9b3b-4264-acc0-2a955eb996b4 | Site-4 | 4 Roadside | 0113 4444444 | ODSD    | R4     | ICB4 | Info 4                 | accessibility/attr_four=true | 0.040992272 | 51.455788 | 5677     | Another Type |

  Scenario: Query Sites filters on multiple services no sites returned
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Type         | IsDeleted |
      | 8184c730-b4e6-45af-999a-f67569eb4367 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | 83599e38-41cf-4f22-8a12-0e84b861cd1b | Site-2 | 2 Roadside | 0113 2222222 | ODSB    | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.14566747  | 51.482472 | Pharmacy     | false     |
      | 1993855c-1dba-4ec8-8a09-4096c05ccecb | Site-3 | 3 Roadside | 0113 3333333 | ODSB    | R3     | ICB3 | Info 3                 | accessibility/attr_four=false | -0.13086317 | 51.583479 | Pharmacy     | false     |
      | a3133ea7-9b3b-4264-acc0-2a955eb996b4 | Site-4 | 4 Roadside | 0113 4444444 | ODSD    | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.040992272 | 51.455788 | Another Type | false     |
    And the following sessions exist for existing site '8184c730-b4e6-45af-999a-f67569eb4367'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID:5_11 | 5           | 1        |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult  | 5           | 1        |
    And the following sessions exist for existing site '83599e38-41cf-4f22-8a12-0e84b861cd1b'
      | Date        | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:5_6  | 5           | 1        |
    And the following sessions exist for existing site '1993855c-1dba-4ec8-8a09-4096c05ccecb'
      | Date        | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:2_3  | 5           | 1        |
    And the following sessions exist for existing site 'a3133ea7-9b3b-4264-acc0-2a955eb996b4'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID:5_11 | 5           | 1        |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult  | 5           | 1        |
    When I query sites by service
      | Longitude | Latitude | SearchRadius | Services          | From     | Until    |
      | 0.082     | 51.5     | 6000         | RSV:Adult,FLU:2_3 | Tomorrow | Tomorrow |
    Then no sites are returned

  Scenario: Filters on multiple services only returns sites with the requested services on the same day
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Type         | IsDeleted |
      | 8184c730-b4e6-45af-999a-f67569eb4367 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | 83599e38-41cf-4f22-8a12-0e84b861cd1b | Site-2 | 2 Roadside | 0113 2222222 | ODSB    | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.14566747  | 51.482472 | Pharmacy     | false     |
      | 1993855c-1dba-4ec8-8a09-4096c05ccecb | Site-3 | 3 Roadside | 0113 3333333 | ODSB    | R3     | ICB3 | Info 3                 | accessibility/attr_four=false | -0.13086317 | 51.583479 | Pharmacy     | false     |
      | a3133ea7-9b3b-4264-acc0-2a955eb996b4 | Site-4 | 4 Roadside | 0113 4444444 | ODSD    | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.040992272 | 51.455788 | Another Type | false     |
    And the following sessions exist for existing site '8184c730-b4e6-45af-999a-f67569eb4367'
      | Date        | From  | Until | Services             | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID:5_11,RSV:Adult | 5           | 1        |
    And the following sessions exist for existing site '83599e38-41cf-4f22-8a12-0e84b861cd1b'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | COVID:5_11 | 5           | 1        |
      | 2 days from today | 09:00 | 17:00 | RSV:Adult  | 5           | 1        |
    And the following sessions exist for existing site '1993855c-1dba-4ec8-8a09-4096c05ccecb'
      | Date        | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | COVID:5_11 | 5           | 1        |
      | 2 days from today | 09:00 | 17:00 | RSV:Adult  | 5           | 1        |
    And the following sessions exist for existing site 'a3133ea7-9b3b-4264-acc0-2a955eb996b4'
      | Date        | From  | Until | Services             | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID:5_11,RSV:Adult | 5           | 1        |
    When I query sites by service
      | Longitude | Latitude | SearchRadius | Services             | From     | Until             |
      | 0.082     | 51.5     | 6000         | COVID:5_11,RSV:Adult | Tomorrow | 3 days from today |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance | Type         |
      | 8184c730-b4e6-45af-999a-f67569eb4367 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true | 0.082750916 | 51.494056 | 662      | GP Practice  |
      | a3133ea7-9b3b-4264-acc0-2a955eb996b4 | Site-4 | 4 Roadside | 0113 4444444 | ODSD    | R4     | ICB4 | Info 4                 | accessibility/attr_four=true | 0.040992272 | 51.455788 | 5677     | Another Type |

  Scenario: Filters on multiple services when the requested services are in multiple sessions on the same day
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Type         | IsDeleted |
      | 8184c730-b4e6-45af-999a-f67569eb4367 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | 83599e38-41cf-4f22-8a12-0e84b861cd1b | Site-2 | 2 Roadside | 0113 2222222 | ODSB    | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.14566747  | 51.482472 | Pharmacy     | false     |
      | 1993855c-1dba-4ec8-8a09-4096c05ccecb | Site-3 | 3 Roadside | 0113 3333333 | ODSB    | R3     | ICB3 | Info 3                 | accessibility/attr_four=false | -0.13086317 | 51.583479 | Pharmacy     | false     |
      | a3133ea7-9b3b-4264-acc0-2a955eb996b4 | Site-4 | 4 Roadside | 0113 4444444 | ODSD    | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.040992272 | 51.455788 | Another Type | false     |
    And the following sessions exist for existing site '8184c730-b4e6-45af-999a-f67569eb4367'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 12:00 | COVID:5_11 | 5           | 1        |
      | Tomorrow    | 12:00 | 17:00 | RSV:Adult  | 5           | 1        |
    And the following sessions exist for existing site '83599e38-41cf-4f22-8a12-0e84b861cd1b'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | COVID:5_11 | 5           | 1        |
      | 2 days from today | 09:00 | 17:00 | RSV:Adult  | 5           | 1        |
    And the following sessions exist for existing site '1993855c-1dba-4ec8-8a09-4096c05ccecb'
      | Date        | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | COVID:5_11 | 5           | 1        |
      | 2 days from today | 09:00 | 17:00 | RSV:Adult  | 5           | 1        |
    And the following sessions exist for existing site 'a3133ea7-9b3b-4264-acc0-2a955eb996b4'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 12:00 | COVID:5_11 | 5           | 1        |
      | Tomorrow    | 12:00 | 17:00 | RSV:Adult  | 5           | 1        |
    When I query sites by service
      | Longitude | Latitude | SearchRadius | Services             | From     | Until             |
      | 0.082     | 51.5     | 6000         | COVID:5_11,RSV:Adult | Tomorrow | 3 days from today |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance | Type         |
      | 8184c730-b4e6-45af-999a-f67569eb4367 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true | 0.082750916 | 51.494056 | 662      | GP Practice  |
      | a3133ea7-9b3b-4264-acc0-2a955eb996b4 | Site-4 | 4 Roadside | 0113 4444444 | ODSD    | R4     | ICB4 | Info 4                 | accessibility/attr_four=true | 0.040992272 | 51.455788 | 5677     | Another Type |

  Scenario: Can filter on multiple services when they're not in overlapping sessions
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Type         | IsDeleted |
      | 8184c730-b4e6-45af-999a-f67569eb4367 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | 83599e38-41cf-4f22-8a12-0e84b861cd1b | Site-2 | 2 Roadside | 0113 2222222 | ODSB    | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.14566747  | 51.482472 | Pharmacy     | false     |
      | 1993855c-1dba-4ec8-8a09-4096c05ccecb | Site-3 | 3 Roadside | 0113 3333333 | ODSB    | R3     | ICB3 | Info 3                 | accessibility/attr_four=false | -0.13086317 | 51.583479 | Pharmacy     | false     |
      | a3133ea7-9b3b-4264-acc0-2a955eb996b4 | Site-4 | 4 Roadside | 0113 4444444 | ODSD    | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.040992272 | 51.455788 | Another Type | false     |
    And the following sessions exist for existing site '8184c730-b4e6-45af-999a-f67569eb4367'
      | Date        | From  | Until | Services             | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 12:00 | COVID:5_11,RSV:Adult | 5           | 1        |
      | Tomorrow    | 13:00 | 17:00 | RSV:Adult,FLU:2_3    | 5           | 1        |
    And the following sessions exist for existing site '83599e38-41cf-4f22-8a12-0e84b861cd1b'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | 2 days from today | 09:00 | 17:00 | RSV:Adult  | 5           | 1        |
      | 2 days from today | 16:00 | 21:00 | FLU:2_3    | 5           | 1        |
    And the following sessions exist for existing site '1993855c-1dba-4ec8-8a09-4096c05ccecb'
      | Date        | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | COVID:5_11 | 5           | 1        |
      | 2 days from today | 09:00 | 17:00 | RSV:Adult  | 5           | 1        |
    And the following sessions exist for existing site 'a3133ea7-9b3b-4264-acc0-2a955eb996b4'
      | Date        | From  | Until | Services             | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 12:00 | COVID:5_11,RSV:Adult | 5           | 1        |
      | Tomorrow    | 13:00 | 17:00 | RSV:Adult,FLU:2_3    | 5           | 1        |
    When I query sites by service
      | Longitude | Latitude | SearchRadius | Services           | From     | Until             |
      | 0.082     | 51.5     | 6000         | COVID:5_11,FLU:2_3 | Tomorrow | 3 days from today |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance | Type         |
      | 8184c730-b4e6-45af-999a-f67569eb4367 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true | 0.082750916 | 51.494056 | 662      | GP Practice  |
      | a3133ea7-9b3b-4264-acc0-2a955eb996b4 | Site-4 | 4 Roadside | 0113 4444444 | ODSD    | R4     | ICB4 | Info 4                 | accessibility/attr_four=true | 0.040992272 | 51.455788 | 5677     | Another Type |

  Scenario: Returns sites when filtering on multiple services even when capactiy is zero
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Type         | IsDeleted |
      | 8184c730-b4e6-45af-999a-f67569eb4367 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | 83599e38-41cf-4f22-8a12-0e84b861cd1b | Site-2 | 2 Roadside | 0113 2222222 | ODSB    | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.14566747  | 51.482472 | Pharmacy     | false     |
      | 1993855c-1dba-4ec8-8a09-4096c05ccecb | Site-3 | 3 Roadside | 0113 3333333 | ODSB    | R3     | ICB3 | Info 3                 | accessibility/attr_four=false | -0.13086317 | 51.583479 | Pharmacy     | false     |
      | a3133ea7-9b3b-4264-acc0-2a955eb996b4 | Site-4 | 4 Roadside | 0113 4444444 | ODSD    | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.040992272 | 51.455788 | Another Type | false     |
    And the following sessions exist for existing site '8184c730-b4e6-45af-999a-f67569eb4367'
      | Date        | From  | Until | Services             | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 12:00 | COVID:5_11,RSV:Adult | 5           | 0        |
      | Tomorrow    | 13:00 | 17:00 | RSV:Adult,FLU:2_3    | 5           | 0        |
    And the following sessions exist for existing site '83599e38-41cf-4f22-8a12-0e84b861cd1b'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | 2 days from today | 09:00 | 17:00 | RSV:Adult  | 5           | 0        |
      | 2 days from today | 16:00 | 21:00 | FLU:2_3    | 5           | 0        |
    And the following sessions exist for existing site '1993855c-1dba-4ec8-8a09-4096c05ccecb'
      | Date              | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | COVID:5_11 | 5           | 0        |
      | 2 days from today | 09:00 | 17:00 | RSV:Adult  | 5           | 0        |
    And the following sessions exist for existing site 'a3133ea7-9b3b-4264-acc0-2a955eb996b4'
      | Date        | From  | Until | Services             | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 12:00 | COVID:5_11,RSV:Adult | 5           | 0        |
      | Tomorrow    | 13:00 | 17:00 | RSV:Adult,FLU:2_3    | 5           | 0        |
    When I query sites by service
      | Longitude | Latitude | SearchRadius | Services           | From     | Until             |
      | 0.082     | 51.5     | 6000         | COVID:5_11,FLU:2_3 | Tomorrow | 3 days from today |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance | Type         |
      | 8184c730-b4e6-45af-999a-f67569eb4367 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true | 0.082750916 | 51.494056 | 662      | GP Practice  |
      | a3133ea7-9b3b-4264-acc0-2a955eb996b4 | Site-4 | 4 Roadside | 0113 4444444 | ODSD    | R4     | ICB4 | Info 4                 | accessibility/attr_four=true | 0.040992272 | 51.455788 | 5677     | Another Type |

  Scenario: Returns no sites when requested services are present in date range but not present on the same day
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Type         | IsDeleted |
      | 8184c730-b4e6-45af-999a-f67569eb4367 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | 83599e38-41cf-4f22-8a12-0e84b861cd1b | Site-2 | 2 Roadside | 0113 2222222 | ODSB    | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.14566747  | 51.482472 | Pharmacy     | false     |
      | 1993855c-1dba-4ec8-8a09-4096c05ccecb | Site-3 | 3 Roadside | 0113 3333333 | ODSB    | R3     | ICB3 | Info 3                 | accessibility/attr_four=false | -0.13086317 | 51.583479 | Pharmacy     | false     |
      | a3133ea7-9b3b-4264-acc0-2a955eb996b4 | Site-4 | 4 Roadside | 0113 4444444 | ODSD    | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.040992272 | 51.455788 | Another Type | false     |
    And the following sessions exist for existing site '8184c730-b4e6-45af-999a-f67569eb4367'
      | Date              | From  | Until | Services             | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 12:00 | COVID:5_11,RSV:Adult | 5           | 0        |
      | 2 days from today | 13:00 | 17:00 | RSV:Adult,FLU:2_3    | 5           | 0        |
    And the following sessions exist for existing site '83599e38-41cf-4f22-8a12-0e84b861cd1b'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult  | 5           | 0        |
      | 2 days from today | 16:00 | 21:00 | FLU:2_3    | 5           | 0        |
    And the following sessions exist for existing site '1993855c-1dba-4ec8-8a09-4096c05ccecb'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | COVID:5_11 | 5           | 0        |
      | 2 days from today | 09:00 | 17:00 | FLU:2_3    | 5           | 0        |
    And the following sessions exist for existing site 'a3133ea7-9b3b-4264-acc0-2a955eb996b4'
      | Date              | From  | Until | Services             | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 12:00 | COVID:5_11,RSV:Adult | 5           | 0        |
      | 3 days from today | 13:00 | 17:00 | RSV:Adult,FLU:2_3    | 5           | 0        |
    When I query sites by service
      | Longitude | Latitude | SearchRadius | Services                     | From     | Until             |
      | 0.082     | 51.5     | 6000         | COVID:5_11,FLU:2_3,RSV:Adult | Tomorrow | 3 days from today |
    Then no sites are returned

  Scenario: Retrieve sites when filtering on multiple services, access needs and site type
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Type         | IsDeleted |
      | 8184c730-b4e6-45af-999a-f67569eb4367 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | 83599e38-41cf-4f22-8a12-0e84b861cd1b | Site-2 | 2 Roadside | 0113 2222222 | ODSB    | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.14566747  | 51.482472 | Pharmacy     | false     |
      | 1993855c-1dba-4ec8-8a09-4096c05ccecb | Site-3 | 3 Roadside | 0113 3333333 | ODSB    | R3     | ICB3 | Info 3                 | accessibility/attr_four=false | -0.13086317 | 51.583479 | Pharmacy     | false     |
      | a3133ea7-9b3b-4264-acc0-2a955eb996b4 | Site-4 | 4 Roadside | 0113 4444444 | ODSD    | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.040992272 | 51.455788 | Another Type | false     |
    And the following sessions exist for existing site '8184c730-b4e6-45af-999a-f67569eb4367'
      | Date        | From  | Until | Services             | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 12:00 | COVID:5_11,RSV:Adult | 5           | 1        |
      | Tomorrow    | 13:00 | 17:00 | RSV:Adult,FLU:2_3    | 5           | 1        |
    And the following sessions exist for existing site '83599e38-41cf-4f22-8a12-0e84b861cd1b'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | 2 days from today | 09:00 | 17:00 | RSV:Adult  | 5           | 1        |
      | 2 days from today | 16:00 | 21:00 | FLU:2_3    | 5           | 1        |
    And the following sessions exist for existing site '1993855c-1dba-4ec8-8a09-4096c05ccecb'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | COVID:5_11 | 5           | 1        |
      | 2 days from today | 09:00 | 17:00 | RSV:Adult  | 5           | 1        |
    And the following sessions exist for existing site 'a3133ea7-9b3b-4264-acc0-2a955eb996b4'
      | Date        | From  | Until | Services             | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 12:00 | COVID:5_11,RSV:Adult | 5           | 1        |
      | Tomorrow    | 13:00 | 17:00 | RSV:Adult,FLU:2_3    | 5           | 1        |
    When I query sites by multiple services, site type and access needs
      | Longitude | Latitude | SearchRadius | AccessNeeds | Types       | MaxRecords | Services           | From     | Until             |
      | 0.082     | 51.5     | 60000        | attr_four   | GP Practice | 10         | COVID:5_11,FLU:2_3 | Tomorrow | 3 days from today |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance | Type         |
      | 8184c730-b4e6-45af-999a-f67569eb4367 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true | 0.082750916 | 51.494056 | 662      | GP Practice  |
      | a3133ea7-9b3b-4264-acc0-2a955eb996b4 | Site-4 | 4 Roadside | 0113 4444444 | ODSD    | R4     | ICB4 | Info 4                 | accessibility/attr_four=true | 0.040992272 | 51.455788 | 5677     | Another Type |

  Scenario: Returns the site when multiple duplicate sessions exist
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Type         | IsDeleted |
      | 8184c730-b4e6-45af-999a-f67569eb4367 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
    And the following sessions exist for existing site '8184c730-b4e6-45af-999a-f67569eb4367'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult  | 5           | 1        |
      | Tomorrow    | 09:00 | 17:00 | COVID:5_11 | 5           | 1        |
      | Tomorrow    | 09:00 | 17:00 | COVID:5_11 | 5           | 1        |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult  | 5           | 1        |
      | Tomorrow    | 09:00 | 17:00 | COVID:5_11 | 5           | 1        |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult  | 5           | 1        |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult  | 5           | 1        |
    When I query sites by service
      | Longitude | Latitude | SearchRadius | Services             | From     | Until    |
      | 0.082     | 51.5     | 6000         | RSV:Adult,COVID:5_11 | Tomorrow | Tomorrow |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance | Type          |
      | 8184c730-b4e6-45af-999a-f67569eb4367 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true | 0.082750916 | 51.494056 | 662      | GP Practice  |

  Scenario: Query Sites - Ensure access need values are lowercase
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                | Longitude   | Latitude  | Type         | IsDeleted |
      | 0d89d01c-b6aa-47c3-8800-38b8ba3917d5 | Site-1 | 1 Roadside | 0113 1111111 | ODS1    | R1     | ICB1 | Info 1                 | accessibility/attr_three=True  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | baa74fcb-a5b6-434e-9915-14e9743f95d9 | Site-2 | 2 Roadside | 0113 2222222 | ODS2    | R2     | ICB2 | Info 2                 | accessibility/attr_one=false   | 0.14566747  | 51.482472 | Pharmacy     | false     |
      | b5d6918d-e674-4e0a-a095-e48352f07053 | Site-3 | 3 Roadside | 0113 3333333 | ODS3    | R3     | ICB3 | Info 3                 | accessibility/attr_one=false   | -0.13086317 | 51.583479 | PCN Site     | false     |
      | 8daeea45-0ac2-424b-a6f0-8d770c31d145 | Site-4 | 4 Roadside | 0113 4444444 | ODS4    | R4     | ICB4 | Info 4                 | accessibility/attr_three=TRUE  | 0.040992272 | 51.455788 | Another Type | false     |
    When I query sites by access needs
      | Longitude | Latitude | SearchRadius | AccessNeeds |
      | 0.082     | 51.5     | 6000         | attr_three |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude   | Distance  | Type         |
      | 0d89d01c-b6aa-47c3-8800-38b8ba3917d5 | Site-1 | 1 Roadside | 0113 1111111 | ODS1    | R1     | ICB1 | Info 1                 | accessibility/attr_three=true | 0.082750916 | 51.494056  | 662       | GP Practice  |
      | 8daeea45-0ac2-424b-a6f0-8d770c31d145 | Site-4 | 4 Roadside | 0113 4444444 | ODS4    | R4     | ICB4 | Info 4                 | accessibility/attr_three=true | 0.040992272 | 51.455788  | 5677      | Another Type |

  Scenario: Query Site Search - Retrieve all sites within a designated distance
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 6877d86e-c2df-4def-8508-e1eccf0ea6be | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 5914b64a-66bb-4ee2-ab8a-94958c1fdfcb | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | -0.13086317 | 51.583479 |
      | 10a54cc1-f052-4c7b-bfc8-de4e5ee7e193 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    When I make the 'query sites' request without access needs
      | Max Records | Search Radius | Longitude | Latitude |
      | 50          | 6000          | 0.082     | 51.5     |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | 6877d86e-c2df-4def-8508-e1eccf0ea6be | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 | 4819     |
      | 10a54cc1-f052-4c7b-bfc8-de4e5ee7e193 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |

  Scenario: Query Site Search - Only return the number of sites requested
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude    | Latitude   |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-5 | 5 Roadside | 0113 5555555 | J12     | R5     | ICB5 | Info 5                 | accessibility/attr_one=true  | -0.082750916 | -51.494056 |
      | 6877d86e-c2df-4def-8508-e1eccf0ea6be | Site-6 | 6 Roadside | 0113 6666666 | K12     | R6     | ICB6 | Info 6                 | accessibility/attr_one=false | -0.14566747  | -51.482472 |
      | 10a54cc1-f052-4c7b-bfc8-de4e5ee7e193 | Site-7 | 7 Roadside | 0113 7777777 | L12     | R7     | ICB7 | Info 7                 | accessibility/attr_one=true  | 0.13086317   | -51.583479 |
      | 5914b64a-66bb-4ee2-ab8a-94958c1fdfcb | Site-8 | 8 Roadside | 0113 8888888 | M12     | R8     | ICB8 | Info 8                 | accessibility/attr_one=false | -0.040992272 | -51.455788 |
    When I make the 'query sites' request without access needs
      | Max Records | Search Radius | Longitude | Latitude |
      | 2           | 100000        | -0.082    | -51.5    |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude    | Latitude   | Distance |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-5 | 5 Roadside | 0113 5555555 | J12     | R5     | ICB5 | Info 5                 | accessibility/attr_one=true  | -0.082750916 | -51.494056 | 662      |
      | 6877d86e-c2df-4def-8508-e1eccf0ea6be | Site-6 | 6 Roadside | 0113 6666666 | K12     | R6     | ICB6 | Info 6                 | accessibility/attr_one=false | -0.14566747  | -51.482472 | 4819     |

  Scenario: Query Site Search - Only return sites with requested access needs
    Given the following sites exist in the system
      | Site                                 | Name    | Address     | PhoneNumber  | OdsCode | Region | ICB   | InformationForCitizens | Accessibilities                                          | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-9  | 9 Roadside  | 0113 9999999 | J12     | R9     | ICB9  | Info 9                 | accessibility/attr_one=true,accessibility/attr_two=true  | -0.0827   | -51.5    |
      | 6877d86e-c2df-4def-8508-e1eccf0ea6be | Site-10 | 10 Roadside | 0113 1010101 | K12     | R10    | ICB10 | Info 10                | accessibility/attr_one=true,accessibility/attr_two=false | -0.0827   | -51.5    |
    When I make the 'query sites' request with access needs
      | Max Records | Search Radius | Longitude | Latitude | AccessNeeds       |
      | 50          | 100000        | -0.082    | -51.5    | attr_one,attr_two |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                         | Longitude | Latitude | Distance |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-9 | 9 Roadside | 0113 9999999 | J12     | R9     | ICB9 | Info 9                 | accessibility/attr_one=true,accessibility/attr_two=true | -0.0827   | -51.5    | 48       |

  Scenario: Query Site Search - Retrieve sites by service filter - single result
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | a03982ab-f9a8-4d4b-97ca-419d1154896f | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
    And the following sessions exist for existing site 'a03982ab-f9a8-4d4b-97ca-419d1154896f'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult      | 5           | 1        |
    When I make the 'query sites' request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From     | Until    |
      | 3           | 6000          | 0.082     | 51.5     | RSV:Adult   | Tomorrow | Tomorrow |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | a03982ab-f9a8-4d4b-97ca-419d1154896f | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |

  Scenario: Query Site Search - Retrieve sites by service filter - multiple results limited to max records ordered by distance
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 3ac1981b-5d62-424a-b403-9d08a40739ce | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 3525af0d-9d89-4b32-ad6b-b85ae94589dc | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | f178d668-a8d7-4fa6-a4b1-b886feef29a6 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
      | 156141af-89ab-4a30-83d4-a4d27a8322c2 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
    And the following sessions exist for existing site '156141af-89ab-4a30-83d4-a4d27a8322c2'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult      | 5           | 1        |
    And the following sessions exist for existing site 'f178d668-a8d7-4fa6-a4b1-b886feef29a6'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult      | 5           | 1        |
    And the following sessions exist for existing site '3525af0d-9d89-4b32-ad6b-b85ae94589dc'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult      | 5           | 1        |
    And the following sessions exist for existing site '3ac1981b-5d62-424a-b403-9d08a40739ce'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult      | 5           | 1        |
    When I make the 'query sites' request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From     | Until    |
      | 3           | 6000          | 0.082     | 51.5     | RSV:Adult   | Tomorrow | Tomorrow |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 156141af-89ab-4a30-83d4-a4d27a8322c2 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | 3525af0d-9d89-4b32-ad6b-b85ae94589dc | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 | 3849     |
      | 3ac1981b-5d62-424a-b403-9d08a40739ce | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 | 4819     |
    When I make the 'query sites' request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From     | Until    |
      | 4           | 6000          | 0.082     | 51.5     | RSV:Adult   | Tomorrow | Tomorrow |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 156141af-89ab-4a30-83d4-a4d27a8322c2 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | 3525af0d-9d89-4b32-ad6b-b85ae94589dc | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 | 3849     |
      | 3ac1981b-5d62-424a-b403-9d08a40739ce | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 | 4819     |
      | f178d668-a8d7-4fa6-a4b1-b886feef29a6 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |

  Scenario: Query Site Search - Retrieve sites by service filter - limits results to only those that support the service
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | b0fa3eaa-cbab-4736-90dd-31922a021074 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 319eb942-1bcd-4d9b-b8b2-777f06d63320 | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | 8f3259bf-e44e-43e6-9837-54a5c87198c7 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
      | 55ad05a8-4fb5-47e1-a961-d18f4008862b | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
    And the following sessions exist for existing site 'b0fa3eaa-cbab-4736-90dd-31922a021074'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:2-3    | 5           | 1        |
    And the following sessions exist for existing site '319eb942-1bcd-4d9b-b8b2-777f06d63320'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:5-6    | 5           | 1        |
    And the following sessions exist for existing site '8f3259bf-e44e-43e6-9837-54a5c87198c7'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult      | 5           | 1        |
    And the following sessions exist for existing site '55ad05a8-4fb5-47e1-a961-d18f4008862b'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult      | 5           | 1        |
    When I make the 'query sites' request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From     | Until    |
      | 4           | 6000          | 0.082     | 51.5     | RSV:Adult   | Tomorrow | Tomorrow |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 55ad05a8-4fb5-47e1-a961-d18f4008862b | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | 8f3259bf-e44e-43e6-9837-54a5c87198c7 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |

  Scenario: Query Site Search - Retrieve sites by service filter - finds a site that isn't the closest because it supports the service
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 12e87824-a2ff-4257-92f3-ee1667c271a3 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 4aeedaf7-48a8-4071-955c-93ccbcbc925c | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | 1950e7f1-356c-4017-ba62-62f3f973681f | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
      | 78d28642-f429-4164-b758-f770b3dcd705 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
    And the following sessions exist for existing site '12e87824-a2ff-4257-92f3-ee1667c271a3'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:2-3    | 5           | 1        |
    And the following sessions exist for existing site '4aeedaf7-48a8-4071-955c-93ccbcbc925c'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:5-6    | 5           | 1        |
    And the following sessions exist for existing site '1950e7f1-356c-4017-ba62-62f3f973681f'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult      | 5           | 1        |
    And the following sessions exist for existing site '78d28642-f429-4164-b758-f770b3dcd705'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:8-16   | 5           | 1        |
#    Prove without service filtering it doesn't return a supported service site
    When I make the 'query sites' request without access needs
      | Max Records | Search Radius | Longitude | Latitude |
      | 1           | 6000          | 0.082     | 51.5     |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 78d28642-f429-4164-b758-f770b3dcd705 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
#    Prove new endpoint returns a supported service site
    When I make the 'query sites' request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service     | From     | Until    |
      | 1           | 6000          | 0.082     | 51.5     | RSV:Adult   | Tomorrow | Tomorrow |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 1950e7f1-356c-4017-ba62-62f3f973681f | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |

  Scenario: Query Site Search - Retrieve sites by service filter - no results if the only site that supports the service is outside the search radius
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 12e87824-a2ff-4257-92f3-ee1667c271a3 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 4aeedaf7-48a8-4071-955c-93ccbcbc925c | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | 1950e7f1-356c-4017-ba62-62f3f973681f | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
      | 78d28642-f429-4164-b758-f770b3dcd705 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
    And the following sessions exist for existing site '12e87824-a2ff-4257-92f3-ee1667c271a3'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:2-3    | 5           | 1        |
    And the following sessions exist for existing site '4aeedaf7-48a8-4071-955c-93ccbcbc925c'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:5-6    | 5           | 1        |
    And the following sessions exist for existing site '1950e7f1-356c-4017-ba62-62f3f973681f'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult      | 5           | 1        |
    And the following sessions exist for existing site '78d28642-f429-4164-b758-f770b3dcd705'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:8-16   | 5           | 1        |
    When I make the 'query sites' request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From     | Until    |
      | 1           | 5000          | 0.082     | 51.5     | RSV:Adult   | Tomorrow | Tomorrow |
    Then no sites are returned

  Scenario: Query Site Search - Retrieve sites by service filter - no results if no sessions
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | beeae4e0-dd4a-4e4a-8f4d-738f9418fb51 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 6877d86e-c2df-1def-8508-e1eccf0ea6be | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 5914b64a-66bb-4ee2-ab8a-94958c1fdfcb | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | -0.13086317 | 51.583479 |
      | 10a54cc1-c052-4c7b-bfc8-de4e5ee7e193 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    When I make the 'query sites' request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From     | Until    |
      | 3           | 6000          | 0.082     | 51.5     | RSV:Adult   | Tomorrow | Tomorrow |
    Then no sites are returned

  Scenario: Query Site Search - Retrieve sites by service filter - no results if no sessions for service exist
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 355ca42f-586c-4f7a-a274-4d53844e3e0c | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 8da01caa-f589-4914-9c4c-42d7adb185ae | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 96f49dd3-f0cb-4b1b-826d-d07065e14c86 | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | -0.13086317 | 51.583479 |
      | 7627459e-15b5-44e7-9318-1b1f3ca5c414 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following sessions exist for existing site '355ca42f-586c-4f7a-a274-4d53844e3e0c'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:5-6    | 5           | 1        |
    And the following sessions exist for existing site '8da01caa-f589-4914-9c4c-42d7adb185ae'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult-19   | 5           | 1        |
    And the following sessions exist for existing site '96f49dd3-f0cb-4b1b-826d-d07065e14c86'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:2-3    | 5           | 1        |
    And the following sessions exist for existing site '7627459e-15b5-44e7-9318-1b1f3ca5c414'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COV        | 5           | 1        |
    When I make the 'query sites' request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From     | Until    |
      | 3           | 6000          | 0.082     | 51.5     | RSV:Adult   | Tomorrow | Tomorrow |
    Then no sites are returned

  Scenario: Query Site Search - Retrieve sites by service filter - no results if no sessions for service exist in the date range required
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | b37000df-f261-40ce-b86b-68b31540e804 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 6092852c-b454-4249-9e12-0c0152056708 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 96e527f3-5791-4567-a6a7-6691f5dcecb5 | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | -0.13086317 | 51.583479 |
      | 38d0905e-9395-4954-ba72-0ae5a81ff876 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following sessions exist for existing site 'b37000df-f261-40ce-b86b-68b31540e804'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult      | 5           | 1        |
    And the following sessions exist for existing site '6092852c-b454-4249-9e12-0c0152056708'
      | Date                 | From  | Until | Services   | Slot Length | Capacity |
      | 4 days from today    | 09:00 | 17:00 | RSV:Adult      | 5           | 1        |
    And the following sessions exist for existing site '96e527f3-5791-4567-a6a7-6691f5dcecb5'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult      | 5           | 1        |
    And the following sessions exist for existing site '38d0905e-9395-4954-ba72-0ae5a81ff876'
      | Date                 | From  | Until | Services     | Slot Length | Capacity |
      | 4 days from today    | 09:00 | 17:00 | RSV:Adult        | 5           | 1        |
    When I make the 'query sites' request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From              | Until             |
      | 3           | 6000          | 0.082     | 51.5     | RSV:Adult   | 2 days from today | 3 days from today |
    Then no sites are returned

  Scenario: Query Site Search - Retrieve sites by service filter - results when service sessions are on different days
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 9ac67f31-cc79-46c0-b0d2-e3be1d7b8caa | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | a1a30efa-9506-47aa-b6a6-fa0e74c848f4 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 45a73a10-87c1-4826-a740-e1ebc4585618 | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | -0.13086317 | 51.583479 |
      | 61da1c54-0b24-470a-8978-bb7c66a15816 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following sessions exist for existing site '9ac67f31-cc79-46c0-b0d2-e3be1d7b8caa'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult      | 5           | 1        |
    And the following sessions exist for existing site 'a1a30efa-9506-47aa-b6a6-fa0e74c848f4'
      | Date                 | From  | Until | Services   | Slot Length | Capacity |
      | 2 days from today    | 09:00 | 17:00 | RSV:Adult      | 5           | 1        |
    And the following sessions exist for existing site '45a73a10-87c1-4826-a740-e1ebc4585618'
      | Date               | From  | Until | Services   | Slot Length | Capacity |
      | 3 days from today  | 09:00 | 17:00 | RSV:Adult      | 5           | 1        |
    And the following sessions exist for existing site '61da1c54-0b24-470a-8978-bb7c66a15816'
      | Date                 | From  | Until | Services     | Slot Length | Capacity |
      | 4 days from today    | 09:00 | 17:00 | RSV:Adult        | 5           | 1        |
    When I make the 'query sites' request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From        | Until             |
      | 3           | 6000          | 0.082     | 51.5     | RSV:Adult   | Tomorrow    | 4 days from today |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 9ac67f31-cc79-46c0-b0d2-e3be1d7b8caa | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | a1a30efa-9506-47aa-b6a6-fa0e74c848f4 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 | 4819     |
      | 61da1c54-0b24-470a-8978-bb7c66a15816 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |

  Scenario: Query Site Search - Retrieve sites by service filter - results when service sessions have multiple services
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 4dbaffc9-f476-494b-817a-37dc8aa151c9 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | b06f4be6-16e3-40be-b373-b67a47301185 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 2ba498ab-42b5-4536-9f11-796077922ce1 | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | -0.13086317 | 51.583479 |
      | d2f9f101-b145-42ef-93e0-ae449efb9a78 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following sessions exist for existing site '4dbaffc9-f476-494b-817a-37dc8aa151c9'
      | Date        | From  | Until | Services             | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID:19, RSV:Adult  | 5           | 1        |
    And the following sessions exist for existing site 'b06f4be6-16e3-40be-b373-b67a47301185'
      | Date                 | From  | Until | Services        | Slot Length | Capacity |
      | 2 days from today    | 09:00 | 17:00 | RSV:Adult, FLU:2-3  | 5           | 1        |
    And the following sessions exist for existing site '2ba498ab-42b5-4536-9f11-796077922ce1'
      | Date               | From  | Until | Services           | Slot Length | Capacity |
      | 3 days from today  | 09:00 | 17:00 | RSV:Adult, RSV:Adult:5-18  | 5           | 1        |
    And the following sessions exist for existing site 'd2f9f101-b145-42ef-93e0-ae449efb9a78'
      | Date                 | From  | Until | Services        | Slot Length | Capacity |
      | 4 days from today    | 09:00 | 17:00 | RSV:Adult, FLU:2-3  | 5           | 1        |
    When I make the 'query sites' request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From        | Until             |
      | 3           | 6000          | 0.082     | 51.5     | RSV:Adult   | Tomorrow    | 4 days from today |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 4dbaffc9-f476-494b-817a-37dc8aa151c9 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | b06f4be6-16e3-40be-b373-b67a47301185 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 | 4819     |
      | d2f9f101-b145-42ef-93e0-ae449efb9a78 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |
    
# This shows the temporary quicker solution for this hotfix 2.2.2 (APPT-1203) and how it isn't a perfect solution and should be fixed properly in the future
  Scenario: Query Site Search - Retrieve sites by service filter - returns sites that are fully booked and support the service (intended behaviour)
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 2d1780ea-73cf-43c1-ad19-1f0cb288e35b | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 586bc02d-310a-4b02-a117-d0d104de16bb | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | a01e7aec-4721-410b-853d-1bed6ade4c3c | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | a5f2f93e-26e8-45ac-a09b-2485517f1d9c | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following sessions exist for existing site '2d1780ea-73cf-43c1-ad19-1f0cb288e35b'
      | Date        | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 09:10 | RSV:Adult     | 10           | 1       |
    And the following bookings have been made at site '2d1780ea-73cf-43c1-ad19-1f0cb288e35b'
      | Date        | Time  | Duration | Service | Reference   |
      | Tomorrow    | 09:00 | 10       | RSV:Adult   | 56345-11111 |
    And the following sessions exist for existing site '586bc02d-310a-4b02-a117-d0d104de16bb'
      | Date                 | From  | Until | Services  | Slot Length | Capacity |
      | 2 days from today    | 09:00 | 09:10 | RSV:Adult     | 10           | 1       |
    And the following bookings have been made at site '586bc02d-310a-4b02-a117-d0d104de16bb'
      | Date                 | Time  | Duration | Service | Reference   |
      | 2 days from today    | 09:00 | 10       | RSV:Adult   | 56345-22222 |
    And the following sessions exist for existing site 'a01e7aec-4721-410b-853d-1bed6ade4c3c'
      | Date               | From  | Until | Services  | Slot Length | Capacity |
      | 3 days from today  | 09:00 | 09:10 | RSV:Adult     | 10           | 1       |
    And the following bookings have been made at site 'a01e7aec-4721-410b-853d-1bed6ade4c3c'
      | Date                 | Time  | Duration | Service | Reference   |
      | 3 days from today    | 09:00 | 10       | RSV:Adult   | 56345-33333 |
    And the following sessions exist for existing site 'a5f2f93e-26e8-45ac-a09b-2485517f1d9c'
      | Date                 | From  | Until | Services | Slot Length | Capacity |
      | 4 days from today    | 09:00 | 09:10 | RSV:Adult    | 10           | 1       |
    And the following bookings have been made at site 'a5f2f93e-26e8-45ac-a09b-2485517f1d9c'
      | Date                 | Time  | Duration | Service | Reference   |
      | 4 days from today    | 09:00 | 10       | RSV:Adult   | 56345-44444 |
    When I check daily availability for site '2d1780ea-73cf-43c1-ad19-1f0cb288e35b' for 'RSV:Adult' between 'Tomorrow' and '4 days from today'
    Then the following availability is returned for 'Tomorrow'
      | From  | Until | Count |
      | 00:00 | 12:00 | 0     |
      | 12:00 | 00:00 | 0     |
    When I check daily availability for site '586bc02d-310a-4b02-a117-d0d104de16bb' for 'RSV:Adult' between 'Tomorrow' and '4 days from today'
    Then the following availability is returned for '2 days from today'
      | From  | Until | Count |
      | 00:00 | 12:00 | 0     |
      | 12:00 | 00:00 | 0     |
    When I check daily availability for site 'a01e7aec-4721-410b-853d-1bed6ade4c3c' for 'RSV:Adult' between 'Tomorrow' and '4 days from today'
    Then the following availability is returned for '3 days from today'
      | From  | Until | Count |
      | 00:00 | 12:00 | 0     |
      | 12:00 | 00:00 | 0     |
    When I check daily availability for site 'a5f2f93e-26e8-45ac-a09b-2485517f1d9c' for 'RSV:Adult' between 'Tomorrow' and '4 days from today'
    Then the following availability is returned for '4 days from today'
      | From  | Until | Count |
      | 00:00 | 12:00 | 0     |
      | 12:00 | 00:00 | 0     |
    When I make the 'query sites' request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From        | Until             |
      | 4           | 6000          | 0.082     | 51.5     | RSV:Adult   | Tomorrow    | 4 days from today |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 2d1780ea-73cf-43c1-ad19-1f0cb288e35b | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | a01e7aec-4721-410b-853d-1bed6ade4c3c | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 | 3849     |
      | 586bc02d-310a-4b02-a117-d0d104de16bb | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 | 4819     |
      | a5f2f93e-26e8-45ac-a09b-2485517f1d9c | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |

  Scenario: Query Site Search - Retrieve sites by service filter - returns sites that are partially booked and support the service (intended behaviour)
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 20e7b709-83c6-416b-b5d8-27d03222e1bf | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 9bf7f58b-ca1a-425a-869e-7a574e183a2c | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 6beadf23-2c8c-4080-8be6-896c73634efb | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | aa8ceff5-d152-4687-b8ea-030df7d5efb1 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following sessions exist for existing site '20e7b709-83c6-416b-b5d8-27d03222e1bf'
      | Date        | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 09:20 | RSV:Adult     | 10           | 1       |
    And the following bookings have been made at site '20e7b709-83c6-416b-b5d8-27d03222e1bf'
      | Date        | Time  | Duration | Service | Reference   |
      | Tomorrow    | 09:00 | 10       | RSV:Adult   | 56345-11111 |
    And the following sessions exist for existing site '9bf7f58b-ca1a-425a-869e-7a574e183a2c'
      | Date                 | From  | Until | Services  | Slot Length | Capacity |
      | 2 days from today    | 09:00 | 09:20 | RSV:Adult     | 10           | 1       |
    And the following bookings have been made at site '9bf7f58b-ca1a-425a-869e-7a574e183a2c'
      | Date                 | Time  | Duration | Service | Reference   |
      | 2 days from today    | 09:00 | 10       | RSV:Adult   | 56345-22222 |
    And the following sessions exist for existing site '6beadf23-2c8c-4080-8be6-896c73634efb'
      | Date               | From  | Until | Services  | Slot Length | Capacity |
      | 3 days from today  | 09:00 | 09:20 | RSV:Adult     | 10           | 1       |
    And the following bookings have been made at site '6beadf23-2c8c-4080-8be6-896c73634efb'
      | Date                 | Time  | Duration | Service | Reference   |
      | 3 days from today    | 09:00 | 10       | RSV:Adult   | 56345-33333 |
    And the following sessions exist for existing site 'aa8ceff5-d152-4687-b8ea-030df7d5efb1'
      | Date                 | From  | Until | Services | Slot Length | Capacity |
      | 4 days from today    | 09:00 | 09:20 | RSV:Adult    | 10           | 1       |
    And the following bookings have been made at site 'aa8ceff5-d152-4687-b8ea-030df7d5efb1'
      | Date                 | Time  | Duration | Service | Reference   |
      | 4 days from today    | 09:00 | 10       | RSV:Adult   | 56345-44444 |
    When I check daily availability for site '20e7b709-83c6-416b-b5d8-27d03222e1bf' for 'RSV:Adult' between 'Tomorrow' and '4 days from today'
    Then the following availability is returned for 'Tomorrow'
      | From  | Until | Count |
      | 00:00 | 12:00 | 1     |
      | 12:00 | 00:00 | 0     |
    When I check daily availability for site '9bf7f58b-ca1a-425a-869e-7a574e183a2c' for 'RSV:Adult' between 'Tomorrow' and '4 days from today'
    Then the following availability is returned for '2 days from today'
      | From  | Until | Count |
      | 00:00 | 12:00 | 1     |
      | 12:00 | 00:00 | 0     |
    When I check daily availability for site '6beadf23-2c8c-4080-8be6-896c73634efb' for 'RSV:Adult' between 'Tomorrow' and '4 days from today'
    Then the following availability is returned for '3 days from today'
      | From  | Until | Count |
      | 00:00 | 12:00 | 1     |
      | 12:00 | 00:00 | 0     |
    When I check daily availability for site 'aa8ceff5-d152-4687-b8ea-030df7d5efb1' for 'RSV:Adult' between 'Tomorrow' and '4 days from today'
    Then the following availability is returned for '4 days from today'
      | From  | Until | Count |
      | 00:00 | 12:00 | 1     |
      | 12:00 | 00:00 | 0     |
    When I make the 'query sites' request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From        | Until             |
      | 4           | 6000          | 0.082     | 51.5     | RSV:Adult   | Tomorrow    | 4 days from today |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 20e7b709-83c6-416b-b5d8-27d03222e1bf | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | 6beadf23-2c8c-4080-8be6-896c73634efb | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 | 3849     |
      | 9bf7f58b-ca1a-425a-869e-7a574e183a2c | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 | 4819     |
      | aa8ceff5-d152-4687-b8ea-030df7d5efb1 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |

  Scenario: Retrieve sites by service filter - returns no sites if they have orphaned bookings for that service, but no sessions for that service
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | dbd61ab0-281a-4df0-b1b7-6b6793f8e119 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | dfd0ac4e-d5ae-4c63-87c9-c84a84c9a7d1 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 5fdbbb82-45ee-4d0c-bbe0-57d25b55512a | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | 6f2108ef-d1b3-4479-a5dc-cf2bb68dceb9 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following orphaned bookings exist for site 'dbd61ab0-281a-4df0-b1b7-6b6793f8e119'
      | Date        | Time  | Duration | Service | Reference   |
      | Tomorrow    | 09:00 | 10       | RSV:Adult   | 56345-11111 |
    And the following orphaned bookings exist for site 'dfd0ac4e-d5ae-4c63-87c9-c84a84c9a7d1'
      | Date                 | Time  | Duration | Service | Reference   |
      | 2 days from today    | 09:00 | 10       | RSV:Adult   | 56345-22222 |
    And the following orphaned bookings exist for site '5fdbbb82-45ee-4d0c-bbe0-57d25b55512a'
      | Date                 | Time  | Duration | Service | Reference   |
      | 3 days from today    | 09:00 | 10       | RSV:Adult   | 56345-33333 |
    And the following orphaned bookings exist for site '6f2108ef-d1b3-4479-a5dc-cf2bb68dceb9'
      | Date                 | Time  | Duration | Service | Reference   |
      | 4 days from today    | 09:00 | 10       | RSV:Adult   | 56345-44444 |
    When I make the 'query sites' request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From        | Until             |
      | 4           | 6000          | 0.082     | 51.5     | RSV:Adult   | Tomorrow    | 4 days from today |
    Then no sites are returned

  Scenario: Query Site Search - Retrieve sites by service filter - only return sites with requested access needs
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 9635bee5-895c-4368-a106-2d6bc1d74087 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 7e7bc5ed-a188-499d-9eab-1566d9e3b972 | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | 3558dad9-d0a6-49f4-942a-c951d07eb283 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
      | ad8ef3bd-cf15-47a6-8510-8bffcd52bd7b | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
    And the following sessions exist for existing site '9635bee5-895c-4368-a106-2d6bc1d74087'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult      | 5           | 1        |
    And the following sessions exist for existing site '7e7bc5ed-a188-499d-9eab-1566d9e3b972'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult      | 5           | 1        |
    And the following sessions exist for existing site '3558dad9-d0a6-49f4-942a-c951d07eb283'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:2_3    | 5           | 1        |
    And the following sessions exist for existing site 'ad8ef3bd-cf15-47a6-8510-8bffcd52bd7b'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult      | 5           | 1        |
    When I make the 'query sites' request with service filtering and with access needs
      | Max Records | Search Radius | Longitude | Latitude | Service     | From     | Until    | AccessNeeds  |
      | 4           | 6000          | 0.082     | 51.5     | RSV:Adult   | Tomorrow | Tomorrow | attr_one     |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | ad8ef3bd-cf15-47a6-8510-8bffcd52bd7b | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |

  Scenario: Query Site Search - Retrieve sites by service filter - only return sites with all requested access needs
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                            | Longitude   | Latitude  |
      | 6997851c-0bcd-462e-b1f5-b1c02f24d37d | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false,accessibility/attr_two=true   | 0.14566747  | 51.482472 |
      | 1efc8a32-ad71-4066-821e-e535317c309a | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=true,accessibility/attr_two=false   | 0.13086317  | 51.483479 |
      | ab746a05-345f-4c06-829b-2d5d52ec341b | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=false,accessibility/attr_two=false  | 0.040992272 | 51.455788 |
      | 8eb79504-1545-4fd9-a358-430a649e0352 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true,accessibility/attr_two=true    | 0.082750916 | 51.494056 |
      | ae579504-1545-4fd9-a358-430a649e0354 | Site-5 | 5 Roadside | 0113 1111111 | N12     | R5     | ICB5 | Info 5                 | accessibility/attr_one=true,accessibility/attr_two=true    | 0.081750916 | 51.484056 |
    And the following sessions exist for existing site '6997851c-0bcd-462e-b1f5-b1c02f24d37d'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult      | 5           | 1        |
    And the following sessions exist for existing site '1efc8a32-ad71-4066-821e-e535317c309a'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult      | 5           | 1        |
    And the following sessions exist for existing site 'ab746a05-345f-4c06-829b-2d5d52ec341b'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:2_3    | 5           | 1        |
    And the following sessions exist for existing site '8eb79504-1545-4fd9-a358-430a649e0352'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult      | 5           | 1        |
    And the following sessions exist for existing site 'ae579504-1545-4fd9-a358-430a649e0354'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:2-3    | 5           | 1        |
    When I make the 'query sites' request with service filtering and with access needs
      | Max Records | Search Radius | Longitude | Latitude | Service | From     | Until    | AccessNeeds        |
      | 4           | 6000          | 0.082     | 51.5     | RSV:Adult   | Tomorrow | Tomorrow | attr_one,attr_two  |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                          | Longitude   | Latitude  | Distance |
      | 8eb79504-1545-4fd9-a358-430a649e0352 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true,accessibility/attr_two=true  | 0.082750916 | 51.494056 | 662      |

  Scenario: Query Site Search - Filter on multiple services returns no sites as services cannot be satisfied
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 20e7b709-83c6-416b-b5d8-27d03222e1bf | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 9bf7f58b-ca1a-425a-869e-7a574e183a2c | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 6beadf23-2c8c-4080-8be6-896c73634efb | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | aa8ceff5-d152-4687-b8ea-030df7d5efb1 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following sessions exist for existing site '20e7b709-83c6-416b-b5d8-27d03222e1bf'
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 09:20 | RSV:Adult | 10          | 1       |
      | Tomorrow | 09:00 | 09:20 | COVID:5_11 | 10         | 1       |
    And the following sessions exist for existing site '9bf7f58b-ca1a-425a-869e-7a574e183a2c'
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 09:20 | RSV:Adult | 10          | 1       |
      | Tomorrow | 09:00 | 09:20 | COVID:5_11 | 10         | 1       |
    And the following sessions exist for existing site '6beadf23-2c8c-4080-8be6-896c73634efb'
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 09:20 | RSV:Adult | 10          | 1       |
      | Tomorrow | 09:00 | 09:20 | COVID:5_11 | 10         | 1       |
    And the following sessions exist for existing site 'aa8ceff5-d152-4687-b8ea-030df7d5efb1'
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 09:20 | COVID:5_11 | 10         | 1       |
      | Tomorrow | 09:00 | 09:20 | RSV:Adult | 10          | 1       |
    When I make the 'query sites' request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service           | From     | Until    |
      | 4           | 6000          | 0.082     | 51.5     | RSV:Adult,FLU:2_3 | Tomorrow | Tomorrow |
    Then no sites are returned

  Scenario: Query Site Search - Returns correct sites when filtering on multiple services and services are present on same day
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 20e7b709-83c6-416b-b5d8-27d03222e1bf | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 9bf7f58b-ca1a-425a-869e-7a574e183a2c | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 6beadf23-2c8c-4080-8be6-896c73634efb | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | aa8ceff5-d152-4687-b8ea-030df7d5efb1 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following sessions exist for existing site '20e7b709-83c6-416b-b5d8-27d03222e1bf'
      | Date     | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow | 09:00 | 15:00 | RSV:Adult  | 10          | 1        |
      | Tomorrow | 09:00 | 15:00 | COVID:5_11 | 10          | 1        |
    And the following sessions exist for existing site '9bf7f58b-ca1a-425a-869e-7a574e183a2c'
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 15:20 | RSV:Adult | 10          | 1        |
      | Tomorrow | 09:00 | 15:20 | FLU:2_3   | 10          | 1        |
    And the following sessions exist for existing site '6beadf23-2c8c-4080-8be6-896c73634efb'
      | Date     | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow | 09:00 | 15:20 | FLU:2_3    | 10          | 1        |
      | Tomorrow | 09:00 | 15:20 | COVID:5_11 | 10          | 1        |
    And the following sessions exist for existing site 'aa8ceff5-d152-4687-b8ea-030df7d5efb1'
      | Date     | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow | 09:00 | 15:20 | COVID:5_11 | 10          | 1        |
      | Tomorrow | 09:00 | 15:20 | RSV:Adult  | 10          | 1        |
    When I make the 'query sites' request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service              | From     | Until    |
      | 4           | 6000          | 0.082     | 51.5     | RSV:Adult,COVID:5_11 | Tomorrow | Tomorrow |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 20e7b709-83c6-416b-b5d8-27d03222e1bf | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | aa8ceff5-d152-4687-b8ea-030df7d5efb1 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |

  Scenario: Query Site Search - Filters on multiple services when services are present across multiple sessions
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 20e7b709-83c6-416b-b5d8-27d03222e1bf | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 9bf7f58b-ca1a-425a-869e-7a574e183a2c | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 6beadf23-2c8c-4080-8be6-896c73634efb | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | aa8ceff5-d152-4687-b8ea-030df7d5efb1 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following sessions exist for existing site '20e7b709-83c6-416b-b5d8-27d03222e1bf'
      | Date     | From  | Until | Services           | Slot Length | Capacity |
      | Tomorrow | 09:00 | 15:00 | RSV:Adult,FLU:2_3  | 10          | 1        |
      | Tomorrow | 09:00 | 15:00 | COVID:5_11,FLU:2_3 | 10          | 1        |
    And the following sessions exist for existing site '9bf7f58b-ca1a-425a-869e-7a574e183a2c'
      | Date     | From  | Until | Services             | Slot Length | Capacity |
      | Tomorrow | 09:00 | 15:20 | RSV:Adult,COVID:5_11 | 10          | 1        |
      | Tomorrow | 09:00 | 15:20 | FLU:2_3              | 10          | 1        |
    And the following sessions exist for existing site '6beadf23-2c8c-4080-8be6-896c73634efb'
      | Date     | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow | 09:00 | 15:20 | FLU:2_3    | 10          | 1        |
      | Tomorrow | 09:00 | 15:20 | COVID:5_11 | 10          | 1        |
    And the following sessions exist for existing site 'aa8ceff5-d152-4687-b8ea-030df7d5efb1'
      | Date     | From  | Until | Services           | Slot Length | Capacity |
      | Tomorrow | 09:00 | 15:20 | COVID:5_11,FLU:2_3 | 10          | 1        |
      | Tomorrow | 09:00 | 15:20 | RSV:Adult          | 10          | 1        |
    When I make the 'query sites' request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service              | From     | Until    |
      | 4           | 6000          | 0.082     | 51.5     | RSV:Adult,COVID:5_11 | Tomorrow | Tomorrow |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 20e7b709-83c6-416b-b5d8-27d03222e1bf | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | 9bf7f58b-ca1a-425a-869e-7a574e183a2c | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 | 4819     |
      | aa8ceff5-d152-4687-b8ea-030df7d5efb1 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |

  Scenario: Query Site Search - Returns no sites when requested services are present but not on the same days
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 20e7b709-83c6-416b-b5d8-27d03222e1bf | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 9bf7f58b-ca1a-425a-869e-7a574e183a2c | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 6beadf23-2c8c-4080-8be6-896c73634efb | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | aa8ceff5-d152-4687-b8ea-030df7d5efb1 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following sessions exist for existing site '20e7b709-83c6-416b-b5d8-27d03222e1bf'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 15:00 | RSV:Adult  | 10          | 1        |
      | 2 days from today | 09:00 | 15:00 | COVID:5_11 | 10          | 1        |
      | 3 days from today | 09:00 | 15:00 | FLU:2_3    | 10          | 1        |
    And the following sessions exist for existing site '9bf7f58b-ca1a-425a-869e-7a574e183a2c'
      | Date              | From  | Until | Services             | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 15:00 | RSV:Adult  | 10          | 1        |
      | 2 days from today | 09:00 | 15:00 | COVID:5_11 | 10          | 1        |
      | 3 days from today | 09:00 | 15:00 | FLU:2_3    | 10          | 1        |
    And the following sessions exist for existing site '6beadf23-2c8c-4080-8be6-896c73634efb'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 15:00 | RSV:Adult  | 10          | 1        |
      | 2 days from today | 09:00 | 15:00 | COVID:5_11 | 10          | 1        |
      | 3 days from today | 09:00 | 15:00 | FLU:2_3    | 10          | 1        |
    And the following sessions exist for existing site 'aa8ceff5-d152-4687-b8ea-030df7d5efb1'
      | Date     | From  | Until | Services            | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 15:00 | RSV:Adult  | 10          | 1        |
      | 2 days from today | 09:00 | 15:00 | COVID:5_11 | 10          | 1        |
      | 3 days from today | 09:00 | 15:00 | FLU:2_3    | 10          | 1        |
    When I make the 'query sites' request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service                      | From     | Until             |
      | 4           | 6000          | 0.082     | 51.5     | RSV:Adult,COVID:5_11,FLU:2_3 | Tomorrow | 3 days from today |
    Then no sites are returned

  Scenario: Query Site Search - Still returns sites with matching sessions even though their capacity is zero
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 20e7b709-83c6-416b-b5d8-27d03222e1bf | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 9bf7f58b-ca1a-425a-869e-7a574e183a2c | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 6beadf23-2c8c-4080-8be6-896c73634efb | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | aa8ceff5-d152-4687-b8ea-030df7d5efb1 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following sessions exist for existing site '20e7b709-83c6-416b-b5d8-27d03222e1bf'
      | Date     | From  | Until | Services           | Slot Length | Capacity |
      | Tomorrow | 09:00 | 15:00 | RSV:Adult,FLU:2_3  | 10          | 0        |
      | Tomorrow | 09:00 | 15:00 | COVID:5_11,FLU:2_3 | 10          | 0        |
    And the following sessions exist for existing site '9bf7f58b-ca1a-425a-869e-7a574e183a2c'
      | Date     | From  | Until | Services             | Slot Length | Capacity |
      | Tomorrow | 09:00 | 15:20 | RSV:Adult,COVID:5_11 | 10          | 0        |
      | Tomorrow | 09:00 | 15:20 | FLU:2_3              | 10          | 0        |
    And the following sessions exist for existing site '6beadf23-2c8c-4080-8be6-896c73634efb'
      | Date     | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow | 09:00 | 15:20 | FLU:2_3    | 10          | 0        |
      | Tomorrow | 09:00 | 15:20 | COVID:5_11 | 10          | 0        |
    And the following sessions exist for existing site 'aa8ceff5-d152-4687-b8ea-030df7d5efb1'
      | Date     | From  | Until | Services           | Slot Length | Capacity |
      | Tomorrow | 09:00 | 15:20 | COVID:5_11,FLU:2_3 | 10          | 0        |
      | Tomorrow | 09:00 | 15:20 | RSV:Adult          | 10          | 0        |
    When I make the 'query sites' request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service              | From     | Until    |
      | 4           | 6000          | 0.082     | 51.5     | RSV:Adult,COVID:5_11 | Tomorrow | Tomorrow |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 20e7b709-83c6-416b-b5d8-27d03222e1bf | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | 9bf7f58b-ca1a-425a-869e-7a574e183a2c | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 | 4819     |
      | aa8ceff5-d152-4687-b8ea-030df7d5efb1 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |

  Scenario: Query Site Search - Retrieve sites which have requested services even though they're in non overlapping sessions
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 20e7b709-83c6-416b-b5d8-27d03222e1bf | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 9bf7f58b-ca1a-425a-869e-7a574e183a2c | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 6beadf23-2c8c-4080-8be6-896c73634efb | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | aa8ceff5-d152-4687-b8ea-030df7d5efb1 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following sessions exist for existing site '20e7b709-83c6-416b-b5d8-27d03222e1bf'
      | Date     | From  | Until | Services           | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult,FLU:2_3  | 10          | 1        |
      | Tomorrow | 13:00 | 17:00 | COVID:5_11,FLU:2_3 | 10          | 1        |
    And the following sessions exist for existing site '9bf7f58b-ca1a-425a-869e-7a574e183a2c'
      | Date     | From  | Until | Services             | Slot Length | Capacity |
      | Tomorrow | 09:00 | 11:20 | RSV:Adult,COVID:5_11 | 10          | 1        |
      | Tomorrow | 12:00 | 15:20 | FLU:2_3              | 10          | 1        |
    And the following sessions exist for existing site '6beadf23-2c8c-4080-8be6-896c73634efb'
      | Date     | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:20 | FLU:2_3    | 10          | 1        |
      | Tomorrow | 12:00 | 15:20 | COVID:5_11 | 10          | 1        |
    And the following sessions exist for existing site 'aa8ceff5-d152-4687-b8ea-030df7d5efb1'
      | Date     | From  | Until | Services           | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:20 | COVID:5_11,FLU:2_3 | 10          | 1        |
      | Tomorrow | 12:00 | 16:20 | RSV:Adult          | 10          | 1        |
    When I make the 'query sites' request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service              | From     | Until    |
      | 4           | 6000          | 0.082     | 51.5     | RSV:Adult,COVID:5_11 | Tomorrow | Tomorrow |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 20e7b709-83c6-416b-b5d8-27d03222e1bf | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | 9bf7f58b-ca1a-425a-869e-7a574e183a2c | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 | 4819     |
      | aa8ceff5-d152-4687-b8ea-030df7d5efb1 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |

  Scenario: Query Site Search - Multi service filtering only returns sites which also match the requested access needs even if they have the capacity for the services
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 20e7b709-83c6-416b-b5d8-27d03222e1bf | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 9bf7f58b-ca1a-425a-869e-7a574e183a2c | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 6beadf23-2c8c-4080-8be6-896c73634efb | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | aa8ceff5-d152-4687-b8ea-030df7d5efb1 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following sessions exist for existing site '20e7b709-83c6-416b-b5d8-27d03222e1bf'
      | Date     | From  | Until | Services           | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult,FLU:2_3  | 10          | 1        |
      | Tomorrow | 13:00 | 17:00 | COVID:5_11,FLU:2_3 | 10          | 1        |
    And the following sessions exist for existing site '9bf7f58b-ca1a-425a-869e-7a574e183a2c'
      | Date     | From  | Until | Services             | Slot Length | Capacity |
      | Tomorrow | 09:00 | 11:20 | RSV:Adult,COVID:5_11 | 10          | 1        |
      | Tomorrow | 12:00 | 15:20 | FLU:2_3              | 10          | 1        |
    And the following sessions exist for existing site '6beadf23-2c8c-4080-8be6-896c73634efb'
      | Date     | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:20 | RSV:Adult  | 10          | 1        |
      | Tomorrow | 12:00 | 15:20 | COVID:5_11 | 10          | 1        |
    And the following sessions exist for existing site 'aa8ceff5-d152-4687-b8ea-030df7d5efb1'
      | Date     | From  | Until | Services           | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:20 | COVID:5_11,FLU:2_3 | 10          | 1        |
      | Tomorrow | 12:00 | 16:20 | RSV:Adult          | 10          | 1        |
    When I make the 'query sites' request with service filtering and with access needs
      | Max Records | Search Radius | Longitude | Latitude | Service              | From     | Until    | AccessNeeds |
      | 4           | 6000          | 0.082     | 51.5     | RSV:Adult,COVID:5_11 | Tomorrow | Tomorrow | attr_one    |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 20e7b709-83c6-416b-b5d8-27d03222e1bf | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | aa8ceff5-d152-4687-b8ea-030df7d5efb1 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |
    
# A test to prove lazy sliding cache behaviours, including delayed updated cache results after a slide
  Scenario: Query Site Search - Lazy Slide Cache Test
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 40e7b709-83c6-416b-b5d8-27d03222e1bf | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 9cf7f58b-ca1a-425a-869e-7a574e183a2c | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 7beadf23-2c8c-4080-8be6-896c73634efb | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | fa8ceff5-d152-4687-b8ea-030df7d5efb1 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following sessions exist for existing site '40e7b709-83c6-416b-b5d8-27d03222e1bf'
      | Date     | From  | Until | Services           | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult,FLU:2_3  | 10          | 1        |
      | Tomorrow | 13:00 | 17:00 | COVID:5_11,FLU:2_3 | 10          | 1        |
    And the following sessions exist for existing site '9cf7f58b-ca1a-425a-869e-7a574e183a2c'
      | Date     | From  | Until | Services             | Slot Length | Capacity |
      | Tomorrow | 09:00 | 11:20 | RSV:Adult,COVID:5_11 | 10          | 1        |
      | Tomorrow | 12:00 | 15:20 | FLU:2_3              | 10          | 1        |
    And the following sessions exist for existing site '7beadf23-2c8c-4080-8be6-896c73634efb'
      | Date     | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:20 | RSV:Adult  | 10          | 1        |
      | Tomorrow | 12:00 | 15:20 | COVID:5_11 | 10          | 1        |
    When I make the 'query sites' request with service filtering, access needs, and caching
      | Max Records | Search Radius | Longitude | Latitude | Service              | From     | Until    | AccessNeeds |
      | 4           | 6000          | 0.082     | 51.5     | RSV:Adult,COVID:5_11 | Tomorrow | Tomorrow | attr_one    |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 40e7b709-83c6-416b-b5d8-27d03222e1bf | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
#   Now add some availability to a site that should match the request
    Given the following sessions exist for existing site 'fa8ceff5-d152-4687-b8ea-030df7d5efb1'
      | Date     | From  | Until | Services           | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:20 | COVID:5_11,FLU:2_3 | 10          | 1        |
      | Tomorrow | 12:00 | 16:20 | RSV:Adult          | 10          | 1        |
    When I make the 'query sites' request with service filtering, access needs, and caching
      | Max Records | Search Radius | Longitude | Latitude | Service              | From     | Until    | AccessNeeds |
      | 4           | 6000          | 0.082     | 51.5     | RSV:Adult,COVID:5_11 | Tomorrow | Tomorrow | attr_one    |
#   The site with the posted availability is not returned yet
#   Cache value is returned, and no slide occurs
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 40e7b709-83c6-416b-b5d8-27d03222e1bf | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
#   Wait for the slide threshold to pass
    When I wait for '1050' milliseconds
#   First request should slide the cache, updating the cache value to the new, but still return the old value
    When I make the 'query sites' request with service filtering, access needs, and caching
      | Max Records | Search Radius | Longitude | Latitude | Service              | From     | Until    | AccessNeeds |
      | 4           | 6000          | 0.082     | 51.5     | RSV:Adult,COVID:5_11 | Tomorrow | Tomorrow | attr_one    |
#   The site with the posted availability is not returned yet
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 40e7b709-83c6-416b-b5d8-27d03222e1bf | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
#   Second request should not slide the cache, but return the new value that was saved by the slide
    When I make the 'query sites' request with service filtering, access needs, and caching
      | Max Records | Search Radius | Longitude | Latitude | Service              | From     | Until    | AccessNeeds |
      | 4           | 6000          | 0.082     | 51.5     | RSV:Adult,COVID:5_11 | Tomorrow | Tomorrow | attr_one    |
#   The site with the posted availability is returned now!
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 40e7b709-83c6-416b-b5d8-27d03222e1bf | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | fa8ceff5-d152-4687-b8ea-030df7d5efb1 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |
    
