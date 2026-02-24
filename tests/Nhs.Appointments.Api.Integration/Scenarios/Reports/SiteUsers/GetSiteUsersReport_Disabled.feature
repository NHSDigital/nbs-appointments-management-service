Feature: Download Site Users Report

  Scenario: Cannot download site users report when toggle is disabled
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Type         | IsDeleted |
      | d8ce8d0a-6c95-421b-9136-9865fba555a1 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60         | -60       | GP Practice  | false     |
    And the following role assignments for 'test-user-1' exist at site 'd8ce8d0a-6c95-421b-9136-9865fba555a1'
      | Roles                        |
      | canned:availability-manager  |
    When I request a site users report
    Then the call should fail with 501
