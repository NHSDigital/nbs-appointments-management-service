Feature: Get daily availability

    Scenario: Dates and sessions are returned within date range
        Given the following sessions
          | Date              | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow          | 09:00 | 17:00 | COVID    | 5           | 1        |
          | 2 days from today | 09:00 | 17:00 | COVID    | 5           | 1        |
          | 5 days from today | 09:00 | 17:00 | COVID    | 5           | 1        |
        When I check daily availability for the current site between 'Tomorrow' and '3 days from now'
        Then the following daily availability is returned
          | Date              | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow          | 09:00 | 17:00 | COVID    | 5           | 1        |
          | 2 days from today | 09:00 | 17:00 | COVID    | 5           | 1        |