Feature: Get hourly availability
  

  Scenario: Hourly availability is returned from session templates with 5 min appointments
    Given the following sessions
      | Date       | From  | Until | Services | Slot Length | Capacity |
      | 2077-01-01 | 09:00 | 11:00 | COVID    | 5           | 1        |
    When I check hourly availability for 'COVID' between '2077-01-01' and '2077-01-01'
    Then the following availability is returned for '2077-01-01'
      | From  | Until | Count |
      | 09:00 | 10:00 | 12    |
      | 10:00 | 11:00 | 12    |

  Scenario: Hourly availability is returned from sessions with 5 min appointments and multiple capacity
    Given the following sessions
      | Date       | From  | Until | Services | Slot Length | Capacity |
      | 2077-01-01 | 09:00 | 11:00 | COVID    | 10          | 2        |    
    When I check hourly availability for 'COVID' between '2077-01-01' and '2077-01-01'
    Then the following availability is returned for '2077-01-01'
      | From  | Until | Count |
      | 09:00 | 10:00 | 12    |
      | 10:00 | 11:00 | 12    |

  Scenario: Hourly availability is returned from overlapping sessions
    Given the following sessions
      | Date       | From  | Until | Services | Slot Length | Capacity |
      | 2077-01-01 | 09:00 | 11:00 | COVID    | 10          | 2        |
      | 2077-01-01 | 09:00 | 11:00 | COVID    | 10          | 3        |      
    When I check hourly availability for 'COVID' between '2077-01-01' and '2077-01-01'
    Then the following availability is returned for '2077-01-01'
      | From  | Until | Count |
      | 09:00 | 10:00 | 30    |
      | 10:00 | 11:00 | 30    |
  
  Scenario: Hourly availability is returned for multiple days
    Given the following sessions
      | Date       | From  | Until | Services | Slot Length | Capacity |
      | 2077-01-01 | 09:00 | 13:00 | COVID    | 5           | 1        |
      | 2077-01-02 | 09:00 | 13:00 | COVID    | 5           | 1        |
      | 2077-01-03 | 09:00 | 13:00 | COVID    | 5           | 1        |
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
