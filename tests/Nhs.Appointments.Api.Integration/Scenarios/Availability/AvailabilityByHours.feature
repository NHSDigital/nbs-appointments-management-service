Feature: Get hourly availability

  Background:
    Given The following service configuration
      | Code          | Duration |
      | COVID         | 5        |
      | FLU           | 8        |
      | COADMIN       | 10       |

  Scenario: Dates and availability are returned from session templates with 5 min appointments
    Given The following week templates
      | Name | Days | From  | Until | Services |
      | Test | All  | 09:00 | 11:00 | COVID    |
    And the following template assignments
      | Template | From       | Until      |
      | Test     | 2077-01-01 | 2077-01-31 |
    When I check hourly availability for 'COVID' between '2077-01-01' and '2077-01-01'
    Then the following availability is returned for '2077-01-01'
      | From  | Until | Count |
      | 09:00 | 10:00 | 12    |
      | 10:00 | 11:00 | 12    |

  Scenario: Dates and availability are returned from session templates with 8 min appointments
    Given The following week templates
      | Name | Days | From  | Until | Services |
      | Test | All  | 09:00 | 11:00 | FLU      |
    And the following template assignments
      | Template | From       | Until      |
      | Test     | 2077-01-01 | 2077-01-31 |
    When I check hourly availability for 'FLU' between '2077-01-01' and '2077-01-01'
    Then the following availability is returned for '2077-01-01'
      | From  | Until | Count |
      | 09:00 | 10:00 | 8     |
      | 10:00 | 11:00 | 7     |
    
  Scenario: Dates and availability are returned from session templates with 10 min appointments
    Given The following week templates
      | Name | Days | From  | Until | Services |
      | Test | All  | 09:00 | 11:00 | COADMIN  |
    And the following template assignments
      | Template | From       | Until      |
      | Test     | 2077-01-01 | 2077-01-31 |
    When I check hourly availability for 'COADMIN' between '2077-01-01' and '2077-01-01'
    Then the following availability is returned for '2077-01-01'
      | From  | Until | Count |
      | 09:00 | 10:00 | 6     |
      | 10:00 | 11:00 | 6     |

  Scenario: Dates and availability are returned for multiple days
    Given The following week templates
      | Name | Days | From  | Until | Services |
      | Test | All  | 09:00 | 13:00 | COVID    |
    And the following template assignments
      | Template | From       | Until      |
      | Test     | 2077-01-01 | 2077-01-31 |
    When I check hourly availability for 'COVID' between '2077-01-01' and '2077-01-03'
    Then the following availability is returned for '2077-01-01'
      | From  | Until | Count |
      | 09:00 | 10:00 | 12    |
      | 10:00 | 11:00 | 12    |
      | 11:00 | 12:00 | 12    |
      | 12:00 | 13:00 | 12    |
    And the following availability is returned for '2077-01-02'
      | From  | Until | Count |
      | 09:00 | 10:00 | 12    |
      | 10:00 | 11:00 | 12    |
      | 11:00 | 12:00 | 12    |
      | 12:00 | 13:00 | 12    |
    And the following availability is returned for '2077-01-03'
      | From  | Until | Count |
      | 09:00 | 10:00 | 12    |
      | 10:00 | 11:00 | 12    |
      | 11:00 | 12:00 | 12    |
      | 12:00 | 13:00 | 12    |
