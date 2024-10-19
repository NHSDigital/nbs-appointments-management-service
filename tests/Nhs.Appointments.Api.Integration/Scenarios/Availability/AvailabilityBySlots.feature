﻿Feature: Get available appointment slots

  Scenario: Dates and slot availability are returned from session templates with 5 min appointments
    Given the following sessions
      | Date       | From  | Until | Services | Slot Length | Capacity |
      | 2077-01-01 | 09:00 | 09:30 | COVID    | 5           | 1        |
    When I check slot availability for 'COVID' between '2077-01-01' and '2077-01-01'
    Then the following availability is returned for '2077-01-01'
      | From  | Until | Count |
      | 09:00 | 09:05 | 1     |
      | 09:05 | 09:10 | 1     |
      | 09:10 | 09:15 | 1     |
      | 09:15 | 09:20 | 1     |
      | 09:20 | 09:25 | 1     |
      | 09:25 | 09:30 | 1     |     

  Scenario: Dates and slot availability are returned for multiple days
    Given the following sessions
      | Date       | From  | Until | Services | Slot Length | Capacity |
      | 2077-01-01 | 09:00 | 09:30 | COVID    | 10          | 1        |
      | 2077-01-02 | 10:30 | 11:00 | COVID    | 10          | 1        |
      | 2077-01-03 | 09:00 | 09:30 | COVID    | 15          | 1        |
    When I check slot availability for 'COVID' between '2077-01-01' and '2077-01-03'
    Then the following availability is returned for '2077-01-01'
      | From  | Until | Count |
      | 09:00 | 09:10 | 1     |
      | 09:10 | 09:20 | 1     |
      | 09:20 | 09:30 | 1     |
    And the following availability is returned for '2077-01-02'
      | From  | Until | Count |
      | 10:30 | 10:40 | 1     |
      | 10:40 | 10:50 | 1     |
      | 10:50 | 11:00 | 1     |
    And the following availability is returned for '2077-01-03'
      | From  | Until | Count |
      | 09:00 | 09:15 | 1     |
      | 09:15 | 09:30 | 1     |