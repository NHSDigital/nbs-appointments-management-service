﻿Feature: Get daily availability

  Scenario: Dates and availability are returned from sessions with 5 min appointments
    Given the following sessions
      | Date       | From  | Until | Services | Slot Length | Capacity |
      | 2077-01-01 | 09:00 | 17:00 | COVID    | 5           | 1        |
      | 2077-01-02 | 09:00 | 17:00 | COVID    | 5           | 1        |
      | 2077-01-03 | 09:00 | 17:00 | COVID    | 5           | 1        |
    When I check daily availability for 'COVID' between '2077-01-01' and '2077-01-03'
    Then the following daily availability is returned
      | Date       | Am | Pm |
      | 2077-01-01 | 36 | 60 |
      | 2077-01-02 | 36 | 60 |
      | 2077-01-03 | 36 | 60 |
      
  Scenario: Dates and availability are returned from sessions with 5 min appointments and multiple capacity
    Given the following sessions
      | Date       | From  | Until | Services | Slot Length | Capacity |
      | 2077-01-01 | 09:00 | 17:00 | COVID    | 5           | 2        |
      | 2077-01-02 | 09:00 | 17:00 | COVID    | 5           | 3        |
      | 2077-01-03 | 09:00 | 17:00 | COVID    | 5           | 4        |
    When I check daily availability for 'COVID' between '2077-01-01' and '2077-01-03'
    Then the following daily availability is returned
      | Date       | Am | Pm  |
      | 2077-01-01 | 72 | 120 | 
      | 2077-01-02 | 108| 180 |
      | 2077-01-03 | 144| 240 |

  Scenario: Dates and availability are returned from overlapping sessions
    Given the following sessions
      | Date       | From  | Until | Services | Slot Length | Capacity |
      | 2077-01-01 | 09:00 | 10:00 | COVID    | 5           | 2        |
      | 2077-01-01 | 09:00 | 10:00 | COVID    | 5           | 3        |      
    When I check daily availability for 'COVID' between '2077-01-01' and '2077-01-03'
    Then the following daily availability is returned
      | Date       | Am | Pm |
      | 2077-01-01 | 60 | 0  |
      | 2077-01-02 | 0  | 0  |
      | 2077-01-03 | 0  | 0  |

  Scenario: Bookings take up availability
    Given the following sessions
      | Date       | From  | Until | Services | Slot Length | Capacity |
      | 2077-01-01 | 09:00 | 10:00 | COVID    | 5           | 1        |
      | 2077-01-02 | 09:00 | 10:00 | COVID    | 5           | 1        |
      | 2077-01-03 | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date       | Time  | Duration | Service |
      | 2077-01-02 | 09:20 | 5        | COVID   |
    When I check daily availability for 'COVID' between '2077-01-01' and '2077-01-03'
    Then the following daily availability is returned
      | Date       | Am | Pm |
      | 2077-01-01 | 12 | 0  |
      | 2077-01-02 | 11 | 0  |
      | 2077-01-03 | 12 | 0  |