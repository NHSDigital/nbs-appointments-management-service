Feature: Download Site Users Report

  Scenario: Can download site users report when toggle is enabled
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Type         | IsDeleted |
      | d8ce8d0a-6c95-421b-9136-9865fba555a1 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
    And the following role assignments for 'test-user-1' exist
      | Site                                 | Roles                        |
      | d8ce8d0a-6c95-421b-9136-9865fba555a1 | canned:availability-manager  |
    And the following role assignments for 'test-user-2' exist
      | Site                                 | Roles                        |
      | d8ce8d0a-6c95-421b-9136-9865fba555a1 | canned:site-details-manager  |
    And the following role assignments for 'test-user-3' exist
      | Site                                 | Roles                       |
      | d8ce8d0a-6c95-421b-9136-9865fba555a1 | canned:appointment-manager  |
    And the following role assignments for 'test-user-4' exist
      | Site                                 | Roles               |
      | d8ce8d0a-6c95-421b-9136-9865fba555a1 | canned:user-manager |
    When I request a site users report for site 'd8ce8d0a-6c95-421b-9136-9865fba555a1'
    Then the call should be successful
    And the report has the following headers
      | User |
    And the report contains the following data
      | User        |
      | test-user-1 |
      | test-user-2 |
      | test-user-3 |
      | test-user-4 |
