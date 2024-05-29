Feature: Get available appointment slots

  Background:
    Given The following service configuration
      | Code          | Duration |
      | COVID         | 5        |
      | FLU           | 8        |
      | COADMIN       | 10       |

  Scenario: Dates and slot availability are returned from session templates with 5 min appointments
    Given The following week templates
      | Name | Days | From  | Until | Services |
      | Test | All  | 09:00 | 09:30 | COVID    |
    And the following template assignments
      | Template | From       | Until      |
      | Test     | 2077-01-01 | 2077-01-31 |
    When I check slot availability for 'COVID' between '2077-01-01' and '2077-01-01'
    Then the following availability is returned for '2077-01-01'
      | From  | Until | Count |
      | 09:00 | 09:05 | 1     |
      | 09:05 | 09:10 | 1     |
      | 09:10 | 09:15 | 1     |
      | 09:15 | 09:20 | 1     |
      | 09:20 | 09:25 | 1     |
      | 09:25 | 09:30 | 1     |     

  Scenario: Dates and slot availability are returned from session templates with 8 min appointments
    Given The following week templates
      | Name | Days | From  | Until | Services |
      | Test | All  | 09:00 | 09:30 | FLU      |
    And the following template assignments
      | Template | From       | Until      |
      | Test     | 2077-01-01 | 2077-01-31 |
    When I check slot availability for 'FLU' between '2077-01-01' and '2077-01-01'
    Then the following availability is returned for '2077-01-01'
      | From  | Until | Count |
      | 09:00 | 09:08 | 1     |
      | 09:08 | 09:16 | 1     |
      | 09:16 | 09:24 | 1     |
    
  Scenario: Dates and slot availability are returned from session templates with 10 min appointments
    Given The following week templates
      | Name | Days | From  | Until | Services |
      | Test | All  | 09:00 | 09:30 | COADMIN  |
    And the following template assignments
      | Template | From       | Until      |
      | Test     | 2077-01-01 | 2077-01-31 |
    When I check slot availability for 'COADMIN' between '2077-01-01' and '2077-01-01'
    Then the following availability is returned for '2077-01-01'
      | From  | Until | Count |
      | 09:00 | 09:10 | 1     |
      | 09:10 | 09:20 | 1     |
      | 09:20 | 09:30 | 1     |

  Scenario: Dates and slot availability are returned for multiple days
    Given The following week templates
      | Name | Days     | From  | Until | Services |
      | Test | Friday   | 09:00 | 09:30 | COADMIN  |
      | Test | Saturday | 14:30 | 15:00 | COADMIN  |
      | Test | Sunday   | 11:45 | 12:15 | COADMIN  |
    And the following template assignments
      | Template | From       | Until      |
      | Test     | 2077-01-01 | 2077-01-31 |
    When I check slot availability for 'COADMIN' between '2077-01-01' and '2077-01-03'
    Then the following availability is returned for '2077-01-01'
      | From  | Until | Count |
      | 09:00 | 09:10 | 1     |
      | 09:10 | 09:20 | 1     |
      | 09:20 | 09:30 | 1     |
    And the following availability is returned for '2077-01-02'
      | From  | Until | Count |
      | 14:30 | 14:40 | 1     |
      | 14:40 | 14:50 | 1     |
      | 14:50 | 15:00 | 1     |
    And the following availability is returned for '2077-01-03'
      | From  | Until | Count |
      | 11:45 | 11:55 | 1     |
      | 11:55 | 12:05 | 1     |
      | 12:05 | 12:15 | 1     |