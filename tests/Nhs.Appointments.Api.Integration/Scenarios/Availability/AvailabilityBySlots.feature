Feature: Get available appointment slots

    Scenario: Slot availability is returned from session templates with 5 min appointments
        Given the following sessions
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 09:30 | COVID    | 5           | 1        |
        When I check slot availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 09:05 | 1     |
          | 09:05 | 09:10 | 1     |
          | 09:10 | 09:15 | 1     |
          | 09:15 | 09:20 | 1     |
          | 09:20 | 09:25 | 1     |
          | 09:25 | 09:30 | 1     |

    Scenario: Slot availability is returned from sessions with 5 min appointments and multiple capacity
        Given the following sessions
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 09:30 | COVID    | 10          | 2        |
        When I check slot availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 09:10 | 2     |
          | 09:10 | 09:20 | 2     |
          | 09:20 | 09:30 | 2     |

    Scenario: Slot availability is returned from overlapping sessions
        Given the following sessions
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 09:30 | COVID    | 10          | 2        |
          | Tomorrow | 09:00 | 09:30 | COVID    | 15          | 3        |
        When I check slot availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 09:10 | 2     |
          | 09:10 | 09:20 | 2     |
          | 09:20 | 09:30 | 2     |
          | 09:00 | 09:15 | 3     |
          | 09:15 | 09:30 | 3     |

    Scenario: Slot availability is returned for multiple days
        Given the following sessions
          | Date       | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow   | 09:00 | 09:30 | COVID    | 10          | 1        |
          | 2 days from today | 10:30 | 11:00 | COVID    | 10          | 1        |
          | 3 days from today | 09:00 | 09:30 | COVID    | 15          | 1        |
        When I check slot availability for 'COVID' between 'Tomorrow' and '3 days from today'
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 09:10 | 1     |
          | 09:10 | 09:20 | 1     |
          | 09:20 | 09:30 | 1     |
        And the following availability is returned for '2 days from today'
          | From  | Until | Count |
          | 10:30 | 10:40 | 1     |
          | 10:40 | 10:50 | 1     |
          | 10:50 | 11:00 | 1     |
        And the following availability is returned for '3 days from today'
          | From  | Until | Count |
          | 09:00 | 09:15 | 1     |
          | 09:15 | 09:30 | 1     |

    Scenario: Booked appointments reduce capcity of the correct slot based on duration
        Given the following sessions
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 09:30 | COVID    | 10          | 2        |
          | Tomorrow | 09:00 | 09:30 | COVID    | 15          | 3        |
        And the following bookings have been made
          | Date     | Time  | Duration | Service |
          | Tomorrow | 09:00 | 15       | COVID   |
        When I check slot availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 09:10 | 2     |
          | 09:10 | 09:20 | 2     |
          | 09:20 | 09:30 | 2     |
          | 09:00 | 09:15 | 2     |
          | 09:15 | 09:30 | 3     |

    Scenario: Booked appointments of other service types reduce capcity of the slots
        Given the following sessions
          | Date     | From  | Until | Services   | Slot Length | Capacity |
          | Tomorrow | 09:00 | 09:30 | COVID, FLU | 10          | 2        |
        And the following bookings have been made
          | Date     | Time  | Duration | Service |
          | Tomorrow | 09:00 | 10       | FLU     |
        When I check slot availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 09:10 | 1     |
          | 09:10 | 09:20 | 2     |
          | 09:20 | 09:30 | 2     |

    Scenario: Booked appointments of other service types don't reduce capcity with multiple sessions
        Given the following sessions
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 09:30 | COVID    | 10          | 2        |
          | Tomorrow | 09:00 | 09:30 | FLU      | 10          | 2        |
        And the following bookings have been made
          | Date     | Time  | Duration | Service |
          | Tomorrow | 09:00 | 10       | FLU     |
        When I check slot availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 09:10 | 2     |
          | 09:10 | 09:20 | 2     |
          | 09:20 | 09:30 | 2     |