Feature: Get hourly availability

    Scenario: Hourly availability is returned from session templates with 5 min appointments
        Given the following sessions
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 11:00 | COVID    | 5           | 1        |
        When I check hourly availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 10:00 | 12    |
          | 10:00 | 11:00 | 12    |

    Scenario: Hourly availability is returned from sessions with 5 min appointments and multiple capacity
        Given the following sessions
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 11:00 | COVID    | 10          | 2        |
        When I check hourly availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 10:00 | 12    |
          | 10:00 | 11:00 | 12    |

    Scenario: Hourly availability is returned from overlapping sessions
        Given the following sessions
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 11:00 | COVID    | 10          | 2        |
          | Tomorrow | 09:00 | 11:00 | COVID    | 10          | 3        |
        When I check hourly availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 10:00 | 30    |
          | 10:00 | 11:00 | 30    |

    Scenario: Hourly availability is returned for multiple days
        Given the following sessions
          | Date       | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow   | 09:00 | 13:00 | COVID    | 5           | 1        |
          | 2 days from today | 09:00 | 13:00 | COVID    | 5           | 1        |
          | 3 days from today | 09:00 | 13:00 | COVID    | 5           | 1        |
        When I check hourly availability for 'COVID' between 'Tomorrow' and '3 days from today'
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 10:00 | 12    |
          | 10:00 | 11:00 | 12    |
          | 11:00 | 12:00 | 12    |
          | 12:00 | 13:00 | 12    |
        And the following availability is returned for '2 days from today'
          | From  | Until | Count |
          | 09:00 | 10:00 | 12    |
          | 10:00 | 11:00 | 12    |
          | 11:00 | 12:00 | 12    |
          | 12:00 | 13:00 | 12    |
        And the following availability is returned for '3 days from today'
          | From  | Until | Count |
          | 09:00 | 10:00 | 12    |
          | 10:00 | 11:00 | 12    |
          | 11:00 | 12:00 | 12    |
          | 12:00 | 13:00 | 12    |