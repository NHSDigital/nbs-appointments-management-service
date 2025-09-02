Feature: Query for bookings

  Scenario: Get all bookings
    Given the following sessions
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | COVID, FLU | 5           | 1        |
      | 2 days from today | 09:00 | 10:00 | COVID, FLU | 10          | 1        |
    And the following bookings exist
      | Date              | Time  | Duration | Service |
      | Tomorrow          | 09:00 | 5        | COVID   |
      | 2 days from today | 09:20 | 10       | FLU     |
    When I query for bookings using the following parameters
      | From (Day) | From (Time) | Until (Day)       | Until (Time) |
      | Tomorrow   | 09:00       | 2 days from today | 09:20        |
    Then the following bookings are returned
      | Date              | Time  | Duration | Service |
      | Tomorrow          | 09:00 | 5        | COVID   |
      | 2 days from today | 09:20 | 10       | FLU     |

  Scenario: Query by time range (inclusive)
    Given the following sessions
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | RSV:Adult | 10          | 5        |
    And the following bookings exist
      | Reference | Time  |
      | 1         | 09:00 |
      | 2         | 09:10 |
      | 3         | 09:20 |
      | 4         | 09:30 |
      | 5         | 09:40 |
    When I query for bookings using the following parameters
      | From (Day) | From (Time) | Until (Day) | Until (Time) |
      | Tomorrow   | 09:10       | Tomorrow    | 09:30        |
    Then the following bookings are returned
      | Reference | Time  |
      | 2         | 09:10 |
      | 3         | 09:20 |
      | 4         | 09:30 |

  Scenario: Query by status
    Given the following sessions
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | RSV:Adult | 10          | 5        |
    And the following bookings exist
      | Reference | Status      |
      | 1         | Booked      |
      | 2         | Cancelled   |
      | 3         | Unknown     |
      | 4         | Provisional |
      | 5         | Booked      |
    When I query for bookings using the following parameters
      | From (Day) | From (Time) | Until (Day) | Until (Time) | Statuses               |
      | Tomorrow   | 09:00       | Tomorrow    | 17:30        | Cancelled, Provisional |
    Then the following bookings are returned
      | Reference | Status      |
      | 2         | Cancelled   |
      | 4         | Provisional |

  Scenario: Query by cancellation reason
    Given the following sessions
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | RSV:Adult | 10          | 5        |
    And the following bookings exist
      | Reference | Cancellation Reason  |
      | 1         | CancelledBySite      |
      | 2         | CancelledBySite      |
      | 3         | RescheduledByCitizen |
      | 4         | CancelledBySite      |
      | 5         | CancelledByCitizen   |
    When I query for bookings using the following parameters
      | From (Day) | From (Time) | Until (Day) | Until (Time) | Cancellation Reason  |
      | Tomorrow   | 09:00       | Tomorrow    | 17:30        | RescheduledByCitizen |
    Then the following bookings are returned
      | Reference | Cancellation Reason |
      | 3         | CancelledBySite     |

  Scenario: Query by cancellation notification status
    Given the following sessions
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | RSV:Adult | 10          | 5        |
    And the following bookings exist
      | Reference | Cancellation Notification Status |
      | 1         | Unknown                          |
      | 2         | Notified                         |
      | 3         | AutomaticNotificationFailed      |
      | 4         | Unnotified                       |
      | 5         | Notified                         |
    When I query for bookings using the following parameters
      | From (Day) | From (Time) | Until (Day) | Until (Time) | Cancellation Notification Status        |
      | Tomorrow   | 09:00       | Tomorrow    | 17:30        | Unnotified, AutomaticNotificationFailed |
    Then the following bookings are returned
      | Reference | Cancellation Notification Status |
      | 3         | AutomaticNotificationFailed      |
      | 4         | Unnotified                       |

  Scenario: Query by a combination of filters
    Given the following sessions
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | RSV:Adult | 10          | 5        |
    And the following bookings exist
      | Reference | Time  | Status      | Cancellation Reason  | Cancellation Notification Status |
      | 1         | 09:00 | Cancelled   | CancelledBySite      | Unnotified                       |
      | 2         | 09:00 | Provisional | null                 | Unknown                          |
      | 3         | 09:10 | Booked      | null                 | Unknown                          |
      | 4         | 09:20 | Cancelled   | RescheduledByCitizen | null                             |
      | 5         | 09:30 | Cancelled   | CancelledByCitizen   | Notified                         |
      | 6         | 09:40 | Cancelled   | CancelledBySite      | Unknown                          |
      | 7         | 09:50 | Cancelled   | CancelledBySite      | AutomaticNotificationFailed      |
      | 8         | 09:50 | Cancelled   | CancelledBySite      | Unnotified                       |
      | 9         | 10:00 | Booked      | null                 | Unknown                          |
      | 10        | 10:10 | Cancelled   | CancelledBySite      | AutomaticNotificationFailed      |
    When I query for bookings using the following parameters
      | From (Day) | From (Time) | Until (Day) | Until (Time) | Statuses  | Cancellation Reason | Cancellation Notification Status        |
      | Tomorrow   | 09:30       | Tomorrow    | 10:00        | Cancelled | CancelledBySite     | Unnotified, AutomaticNotificationFailed |
    Then the following bookings are returned
      | Reference | Time  | Status    | Cancellation Reason | Cancellation Notification Status |
      | 7         | 09:50 | Cancelled | CancelledBySite     | AutomaticNotificationFailed      |
      | 8         | 09:50 | Cancelled | CancelledBySite     | Unnotified                       |
