Feature: Query Sites

  Scenario: Query Sites By Type and ODS Code
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Type         | IsDeleted |
      | 238d42d1-8628-43d1-8180-76345e82e471 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | 0ecd3590-c7ae-4673-8fb4-3614c3cb0b61 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 | Pharmacy     | false     |
      | 06b1be0d-c170-4f6c-95c8-4a5629631031 | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | -0.13086317 | 51.583479 | PCN Site     | false     |
      | a5088094-0519-442b-9e19-e795fe7deb2e | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | Another Type | false     |
    When I query sites by site type and ODS code
      | Types    | OdsCode | Longitude | Latitude | SearchRadius |
      | Pharmacy | K12     | 0.082     | 51.5     | 6000         |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude  | Latitude  | Distance  | Type     |
      | 0ecd3590-c7ae-4673-8fb4-3614c3cb0b61 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false  | 0.14566747 | 51.482472 | 4819      | Pharmacy |

  Scenario: Query Sites by accessibilities
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                | Longitude   | Latitude  | Type         | IsDeleted |
      | 0d89d01c-b6aa-47c3-8800-38b8ba3917d5 | Site-1 | 1 Roadside | 0113 1111111 | ODS1    | R1     | ICB1 | Info 1                 | accessibility/attr_three=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | baa74fcb-a5b6-434e-9915-14e9743f95d9 | Site-2 | 2 Roadside | 0113 2222222 | ODS2    | R2     | ICB2 | Info 2                 | accessibility/attr_one=false   | 0.14566747  | 51.482472 | Pharmacy     | false     |
      | b5d6918d-e674-4e0a-a095-e48352f07053 | Site-3 | 3 Roadside | 0113 3333333 | ODS3    | R3     | ICB3 | Info 3                 | accessibility/attr_one=false   | -0.13086317 | 51.583479 | PCN Site     | false     |
      | 8daeea45-0ac2-424b-a6f0-8d770c31d145 | Site-4 | 4 Roadside | 0113 4444444 | ODS4    | R4     | ICB4 | Info 4                 | accessibility/attr_three=true  | 0.040992272 | 51.455788 | Another Type | false     |
    When I query sites by access needs
      | Longitude | Latitude | SearchRadius | AccessNeeds |
      | 0.082     | 51.5     | 6000         | attr_three  |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude   | Distance  | Type         |
      | 0d89d01c-b6aa-47c3-8800-38b8ba3917d5 | Site-1 | 1 Roadside | 0113 1111111 | ODS1    | R1     | ICB1 | Info 1                 | accessibility/attr_three=true | 0.082750916 | 51.494056  | 662       | GP Practice  |
      | 8daeea45-0ac2-424b-a6f0-8d770c31d145 | Site-4 | 4 Roadside | 0113 4444444 | ODS4    | R4     | ICB4 | Info 4                 | accessibility/attr_three=true | 0.040992272 | 51.455788  | 5677      | Another Type |

  Scenario: Query Sites by service and availability - Sites returned
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                | Longitude   | Latitude  | Type         | IsDeleted |
      | 27b5b656-31d1-44e4-9851-f38c2a0dcc05 | Site-1 | 1 Roadside | 0113 1111111 | ODS1    | R1     | ICB1 | Info 1                 | accessibility/attr_three=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | 7c08e6ba-25b4-4154-95e6-a76b8a92809d | Site-2 | 2 Roadside | 0113 2222222 | ODS2    | R2     | ICB2 | Info 2                 | accessibility/attr_one=false   | 0.14566747  | 51.482472 | Pharmacy     | false     |
      | 2bff8018-e99b-4625-ae10-e699bdc42d4e | Site-3 | 3 Roadside | 0113 3333333 | ODS3    | R3     | ICB3 | Info 3                 | accessibility/attr_one=false   | -0.13086317 | 51.583479 | PCN Site     | false     |
      | c5b2c4de-3ed5-4f46-bd6d-c4ba7658fa5c | Site-4 | 4 Roadside | 0113 4444444 | ODS4    | R4     | ICB4 | Info 4                 | accessibility/attr_three=true  | 0.040992272 | 51.455788 | Another Type | false     |
    And the following sessions exist for site '27b5b656-31d1-44e4-9851-f38c2a0dcc05'
      | Date        | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult | 5           | 1        |
    And the following sessions exist for site '7c08e6ba-25b4-4154-95e6-a76b8a92809d'
      | Date        | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:5-6  | 5           | 1        |
    And the following sessions exist for site '2bff8018-e99b-4625-ae10-e699bdc42d4e'
      | Date        | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:2-3  | 5           | 1        |
    And the following sessions exist for site 'c5b2c4de-3ed5-4f46-bd6d-c4ba7658fa5c'
      | Date        | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult | 5           | 1        |
    When I query sites by service
      | Longitude | Latitude | SearchRadius | Services  | From     | Until    |
      | 0.082     | 51.5     | 6000         | RSV:Adult | Tomorrow | Tomorrow |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Distance |Type          |
      | 27b5b656-31d1-44e4-9851-f38c2a0dcc05 | Site-1 | 1 Roadside | 0113 1111111 | ODS1    | R1     | ICB1 | Info 1                 | accessibility/attr_three=true | 0.082750916 | 51.494056 | 662      | GP Practice  |
      | c5b2c4de-3ed5-4f46-bd6d-c4ba7658fa5c | Site-4 | 4 Roadside | 0113 4444444 | ODS4    | R4     | ICB4 | Info 4                 | accessibility/attr_three=true | 0.040992272 | 51.455788 | 5677     | Another Type |

  Scenario: Query Sites by service and availability - No sites returned
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                | Longitude   | Latitude  | Type         | IsDeleted |
      | 79d5fae3-0988-457b-9b2a-c8a8596e5ac3 | Site-1 | 1 Roadside | 0113 1111111 | ODS1    | R1     | ICB1 | Info 1                 | accessibility/attr_three=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | c2848ba3-8dca-45ca-bacf-4f1769fff8dc | Site-2 | 2 Roadside | 0113 2222222 | ODS2    | R2     | ICB2 | Info 2                 | accessibility/attr_one=false   | 0.14566747  | 51.482472 | Pharmacy     | false     |
      | e448fcf0-7e0a-4c28-ac64-bcf64fa0c33a | Site-3 | 3 Roadside | 0113 3333333 | ODS3    | R3     | ICB3 | Info 3                 | accessibility/attr_one=false   | -0.13086317 | 51.583479 | PCN Site     | false     |
      | 7d4e4c69-5ea7-4e19-a069-bd776df2c491 | Site-4 | 4 Roadside | 0113 4444444 | ODS4    | R4     | ICB4 | Info 4                 | accessibility/attr_three=true  | 0.040992272 | 51.455788 | Another Type | false     |
    And the following sessions exist for site '79d5fae3-0988-457b-9b2a-c8a8596e5ac3'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID:5-11 | 5           | 1        |
    And the following sessions exist for site 'c2848ba3-8dca-45ca-bacf-4f1769fff8dc'
      | Date        | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:5-6  | 5           | 1        |
    And the following sessions exist for site 'e448fcf0-7e0a-4c28-ac64-bcf64fa0c33a'
      | Date        | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:2-3  | 5           | 1        |
    And the following sessions exist for site '7d4e4c69-5ea7-4e19-a069-bd776df2c491'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID:5-11 | 5           | 1        |
    When I query sites by service
      | Longitude | Latitude | SearchRadius | Services  | From              | Until             |
      | 0.082     | 51.5     | 6000         | RSV:Adult | 2 days from today | 3 days from today |
    Then no sites are returned

  Scenario: Query Sites by multiple filters - Returns all sites
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Type         | IsDeleted |
      | 60d5105e-eb83-4a99-8f75-0a8790583233 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | 5972cb6a-b9bb-4e6e-9a40-53e574d22c7a | Site-2 | 2 Roadside | 0113 2222222 | ODSB    | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.14566747  | 51.482472 | Pharmacy     | false     |
      | 30f48c6e-c65f-48a5-a219-1e073a3a7001 | Site-3 | 3 Roadside | 0113 3333333 | ODSB    | R3     | ICB3 | Info 3                 | accessibility/attr_four=false | -0.13086317 | 51.583479 | Pharmacy     | false     |
      | daa8f2a1-c682-49ff-8d15-b721bdac730a | Site-4 | 4 Roadside | 0113 4444444 | ODSD    | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.040992272 | 51.455788 | Another Type | false     |
    When I query sites by multiple filters
      | Longitude | Latitude | SearchRadius | AccessNeeds | Types    | OdsCode |
      | 0.082     | 51.5     | 6000         | attr_four   | Pharmacy | ODSB    |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Distance |Type          |
      | 60d5105e-eb83-4a99-8f75-0a8790583233 | Site-1 | 1 Roadside | 0113 1111111 | ODSA    | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.082750916 | 51.494056 | 662      | GP Practice  |
      | 5972cb6a-b9bb-4e6e-9a40-53e574d22c7a | Site-2 | 2 Roadside | 0113 2222222 | ODSB    | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.14566747  | 51.482472 | 4819     | Pharmacy     |
      | 30f48c6e-c65f-48a5-a219-1e073a3a7001 | Site-3 | 3 Roadside | 0113 3333333 | ODSB    | R3     | ICB3 | Info 3                 | accessibility/attr_four=false | -0.13086317 | 51.583479 | 17402    | Pharmacy     |
      | daa8f2a1-c682-49ff-8d15-b721bdac730a | Site-4 | 4 Roadside | 0113 4444444 | ODSD    | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.040992272 | 51.455788 | 5677     | Another Type |

  Scenario: Query Sites does not return soft deleted sites
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Type         | IsDeleted |
      | 4739abc0-5eca-4b2f-8960-ada8f2081837 | Site-1 | 1 Roadside | 0113 1111111 | ODS123  | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.282750916 | 53.494056 | GP Practice  | false     |
      | 51641717-91a3-40d3-8d28-ac591c32b4ab | Site-2 | 2 Roadside | 0113 2222222 | ODS321  | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.24566747  | 53.482472 | Pharmacy     | true      |
      | 6fadf09d-5e7d-4262-a0c6-5467dba3bc83 | Site-3 | 3 Roadside | 0113 3333333 | ODS456  | R3     | ICB3 | Info 3                 | accessibility/attr_four=false | -0.03086317 | 53.583479 | Pharmacy     | true      |
      | d815ea3d-c614-4150-a9b7-f78dd1df853c | Site-4 | 4 Roadside | 0113 4444444 | ODS654  | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.240992272 | 53.455788 | Another Type | false     |
    When I query sites by location
      | Longitude | Latitude | SearchRadius |
      | 0.282     | 53.5     | 6000         |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Distance |Type          |
      | 4739abc0-5eca-4b2f-8960-ada8f2081837 | Site-1 | 1 Roadside | 0113 1111111 | ODS123  | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.282750916 | 53.494056 | 662      | GP Practice  |
      | d815ea3d-c614-4150-a9b7-f78dd1df853c | Site-4 | 4 Roadside | 0113 4444444 | ODS654  | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.240992272 | 53.455788 | 5615     | Another Type |

  Scenario: Query sites excludes specific site types
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Type         | IsDeleted |
      | d62690d3-9104-490f-a1bd-57545c7ea70b | Site-1 | 1 Roadside | 0113 1111111 | ODS123  | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.382750916 | 54.494056 | GP Practice  | false     |
      | 6016be1e-1b33-4322-a743-ae90a6595838 | Site-2 | 2 Roadside | 0113 2222222 | ODS123  | R2     | ICB2 | Info 2                 | accessibility/attr_four=false | 0.34566747  | 54.482472 | Pharmacy     | false     |
      | cb031bf3-1e93-4756-ab72-cbe2d3fedf2d | Site-3 | 3 Roadside | 0113 3333333 | ODS123  | R3     | ICB3 | Info 3                 | accessibility/attr_four=false | -0.23086317 | 54.583479 | Pharmacy     | false     |
      | 59bae202-3de1-4a57-8aa8-ac9dde083b1b | Site-4 | 4 Roadside | 0113 4444444 | ODS123  | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.340992272 | 54.455788 | Another Type | false     |
    When I query sites by site type and ODS code
      | Types    | OdsCode | Longitude | Latitude | SearchRadius |
      | !Pharmacy | ODS123 | 0.382     | 54.5     | 6000         |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities               | Longitude   | Latitude  | Distance |Type          |
      | d62690d3-9104-490f-a1bd-57545c7ea70b | Site-1 | 1 Roadside | 0113 1111111 | ODS123  | R1     | ICB1 | Info 1                 | accessibility/attr_four=true  | 0.382750916 | 54.494056 | 662      | GP Practice  |
      | 59bae202-3de1-4a57-8aa8-ac9dde083b1b | Site-4 | 4 Roadside | 0113 4444444 | ODS123  | R4     | ICB4 | Info 4                 | accessibility/attr_four=true  | 0.340992272 | 54.455788 | 5584     | Another Type |
