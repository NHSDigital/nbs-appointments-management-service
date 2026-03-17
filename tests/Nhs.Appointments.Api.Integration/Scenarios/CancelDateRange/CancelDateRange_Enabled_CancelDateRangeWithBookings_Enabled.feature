Feature: Cancel date range

  Scenario: Cancel sessions in a date range with bookings
    Given the following default site exists in the system
      | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    When I cancel sessions and bookings for the default site within a date range
      | From     | To                | CancelBookings |
      | Tomorrow | 3 days from today | false          |
    Then the following cancel date range metrics should be returned
      | SessionCount | BookingCount | BookingsWithoutContactDetails |
      | 0            | 0            | 0                             |

  Scenario: Cancel date range bad request when from date is after to date
    Given the following default site exists in the system
      | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    When I cancel sessions and bookings for the default site within a date range
      | From              | To       | CancelBookings |
      | 3 days from today | Tomorrow | false          |
    Then the call should fail with 400

  Scenario: Cancel date range bad request when from date is in the past
    Given the following default site exists in the system
      | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    When I cancel sessions and bookings for the default site within a date range
      | From      | To       | CancelBookings |
      | Yesterday | Tomorrow | false          |
    Then the call should fail with 400

  Scenario: Cancel date range bad request when to date is in the past
    Given the following default site exists in the system
      | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    When I cancel sessions and bookings for the default site within a date range
      | From              | To        | CancelBookings |
      | 3 days from today | Yesterday | false          |
    Then the call should fail with 400

  Scenario: Cancel date range bad request when from date over 3 months after to date
    Given the following default site exists in the system
      | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    When I cancel sessions and bookings for the default site within a date range
      | From     | To                  | CancelBookings |
      | Tomorrow | 4 months from today | false          |
    Then the call should fail with 400

  Scenario: Cancel date range returns session count with zero bookings when cancel bookings false
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 3 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I cancel sessions and bookings for the default site within a date range
      | From     | To                | CancelBookings |
      | Tomorrow | 3 days from today | false          |
    Then the following cancel date range metrics should be returned
      | SessionCount | BookingCount | BookingsWithoutContactDetails |
      | 3            | 0            | 0                             |
    When I check daily availability for the default site between 'Tomorrow' and '3 days from today'
    Then the daily availability should be empty

  Scenario: Cancel date range returns session count with zero bookings when cancel bookings is true but there are no bookings
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 3 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I cancel sessions and bookings for the default site within a date range
      | From     | To                | CancelBookings |
      | Tomorrow | 3 days from today | true           |
    Then the following cancel date range metrics should be returned
      | SessionCount | BookingCount | BookingsWithoutContactDetails |
      | 3            | 0            | 0                             |
    When I check daily availability for the default site between 'Tomorrow' and '3 days from today'
    Then the daily availability should be empty

  Scenario: Cancel date range ensures already cancelled sessions are not returned in the session count
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 3 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I cancel the following session at the default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | RSV:Adult | 10           | 1       |
    When I cancel sessions and bookings for the default site within a date range
      | From     | To                | CancelBookings |
      | Tomorrow | 3 days from today | false          |
    Then the following cancel date range metrics should be returned
      | SessionCount | BookingCount | BookingsWithoutContactDetails |
      | 2            | 0            | 0                             |
    When I check daily availability for the default site between 'Tomorrow' and '3 days from today'
    Then the daily availability should be empty

  Scenario: Ensure bookings are cancelled after cancelling a date range with bookings
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 3 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    And the following bookings have been made at the default site
      | Date              | Time  | Duration  | Service   | Reference |
      | Tomorrow          | 09:00 | 10        | RSV:Adult | 357951    |
      | 2 days from today | 09:10 | 10        | RSV:Adult | 951753    |
      | 3 days from today | 09:10 | 10        | RSV:Adult | 852147    |
    When I cancel sessions and bookings for the default site within a date range
      | From     | To                | CancelBookings |
      | Tomorrow | 3 days from today | true           |
    Then the following cancel date range metrics should be returned
      | SessionCount | BookingCount | BookingsWithoutContactDetails |
      | 3            | 3            | 0                             |
    When I check daily availability for the default site between 'Tomorrow' and '3 days from today'
    Then the daily availability should be empty
    And the following bookings at the default site are now in the following state
      | Reference | Status    |
      | 357951    | Cancelled |
      | 951753    | Cancelled |
      | 852147    | Cancelled |

  Scenario: Ensure bookings that have already been cancelled aren't returned in the booking count
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 3 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    And the following bookings have been made at the default site
      | Date              | Time  | Duration  | Service   | Reference |
      | Tomorrow          | 09:00 | 10        | RSV:Adult | 987321    |
      | 2 days from today | 09:10 | 10        | RSV:Adult | 123789    |
      | 3 days from today | 09:10 | 10        | RSV:Adult | 654753    |
    When I cancel the booking at the default site with reference '987321'
    And I cancel sessions and bookings for the default site within a date range
      | From     | To                | CancelBookings |
      | Tomorrow | 3 days from today | true           |
    Then the following cancel date range metrics should be returned
      | SessionCount | BookingCount | BookingsWithoutContactDetails |
      | 3            | 2            | 0                             |
    When I check daily availability for the default site between 'Tomorrow' and '3 days from today'
    Then the daily availability should be empty
    And the following bookings at the default site are now in the following state
      | Reference | Status    |
      | 123789    | Cancelled |
      | 654753    | Cancelled |

  Scenario: Ensure provisional bookings aren't affected when cancelling a date range with bookings
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 3 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    And the following bookings have been made at the default site
      | Date              | Time  | Duration  | Service   | Reference |
      | Tomorrow          | 09:00 | 10        | RSV:Adult | 020103    |
      | 2 days from today | 09:10 | 10        | RSV:Adult | 302010    |
      | 3 days from today | 09:10 | 10        | RSV:Adult | 313233    |
    And the following provisional bookings have been made at the default site
      | Date              | Time  | Duration | Service | Reference |
      | 2 days from today | 09:20 | 10       | COVID   | 979899    |
    When I cancel sessions and bookings for the default site within a date range
      | From     | To                | CancelBookings |
      | Tomorrow | 3 days from today | true           |
    Then the following cancel date range metrics should be returned
      | SessionCount | BookingCount | BookingsWithoutContactDetails |
      | 3            | 3            | 0                             |
    When I check daily availability for the default site between 'Tomorrow' and '3 days from today'
    Then the daily availability should be empty
    And the following bookings at the default site are now in the following state
      | Reference | Status      |
      | 020103    | Cancelled   |
      | 302010    | Cancelled   |
      | 313233    | Cancelled   |
      | 979899    | Provisional |

  Scenario: Ensure bookings without contact details are included in the booking count when cancelling a date range with bookings
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 3 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    And the following bookings have been made at the default site
      | Date              | Time  | Duration  | Service   | Reference |
      | Tomorrow          | 09:00 | 10        | RSV:Adult | 369123    |
      | 2 days from today | 09:10 | 10        | RSV:Adult | 963321    |
      | 3 days from today | 09:10 | 10        | RSV:Adult | 654718    |
    And the following bookings without contact details exist at the default site
      | Date              | Time  | Duration  | Service | Reference |
      | Tomorrow          | 09:00 | 10        | FLU:2_3 | 876589    |
      | 2 days from today | 09:10 | 10        | FLU:2_3 | 141213    |
    When I cancel sessions and bookings for the default site within a date range
      | From     | To                | CancelBookings |
      | Tomorrow | 3 days from today | true           |
    Then the following cancel date range metrics should be returned
      | SessionCount | BookingCount | BookingsWithoutContactDetails |
      | 3            | 5            | 2                             |
    When I check daily availability for the default site between 'Tomorrow' and '3 days from today'
    Then the daily availability should be empty
    And the following bookings at the default site are now in the following state
      | Reference | Status    |
      | 369123    | Cancelled |
      | 963321    | Cancelled |
      | 654718    | Cancelled |
      | 876589    | Cancelled |
      | 141213    | Cancelled |

  Scenario: Ensure supported orphaned bookings are cancelled and included in the cancelled booking count
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 3 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    And the following bookings have been made at the default site
      | Date              | Time  | Duration  | Service   | Reference |
      | Tomorrow          | 09:00 | 10        | RSV:Adult | 101112    |
      | 2 days from today | 09:10 | 10        | RSV:Adult | 707172    |
      | 3 days from today | 09:10 | 10        | RSV:Adult | 515253    |
    And the following orphaned bookings exist at the default site
      | Date              | Time  | Duration | Service | Reference |
      | 2 days from today | 09:20 | 10       | COVID   | 989796    |
    When I cancel sessions and bookings for the default site within a date range
      | From     | To                | CancelBookings |
      | Tomorrow | 3 days from today | true           |
    Then the following cancel date range metrics should be returned
      | SessionCount | BookingCount | BookingsWithoutContactDetails |
      | 3            | 4            | 0                             |
    When I check daily availability for the default site between 'Tomorrow' and '3 days from today'
    Then the daily availability should be empty
    And the following bookings at the default site are now in the following state
      | Reference | Status    |
      | 101112    | Cancelled |
      | 707172    | Cancelled |
      | 515253    | Cancelled |
      | 989796    | Cancelled |

  Scenario: Cancel date range only removes sessions inside the requested range
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 5 days from today | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    And the following bookings have been made at the default site
      | Date              | Time  | Duration | Service   | Reference |
      | Tomorrow          | 09:00 | 10       | RSV:Adult | 11111     |
      | 5 days from today | 09:10 | 10       | RSV:Adult | 22222     |
    When I cancel sessions and bookings for the default site within a date range
      | From     | To                | CancelBookings |
      | Tomorrow | 2 days from today | false          |
    Then the following cancel date range metrics should be returned
      | SessionCount | BookingCount | BookingsWithoutContactDetails |
      | 2            | 0            | 0                              |
    And the following bookings at the default site are now in the following state
      | Reference | Status |
      | 11111     | Booked |
      | 22222     | Booked |

  Scenario: Cancel date range cancels orphaned bookings without sessions
    Given the following orphaned bookings exist at the default site
      | Date              | Time  | Duration | Service   | Reference |
      | Tomorrow          | 09:00 | 10       | RSV:Adult | 33333     |
      | 2 days from today | 09:10 | 10       | RSV:Adult | 44444     |
    When I cancel sessions and bookings for the default site within a date range
      | From     | To                | CancelBookings |
      | Tomorrow | 2 days from today | true           |
    Then the following cancel date range metrics should be returned
      | SessionCount | BookingCount | BookingsWithoutContactDetails |
      | 0            | 2            | 0                              |
    And the following bookings at the default site are now in the following state
      | Reference | Status    |
      | 33333     | Cancelled |
      | 44444     | Cancelled |

  Scenario: Cancelling the same date range twice should not double cancel bookings
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    And the following bookings have been made at the default site
      | Date     | Time  | Duration | Service   | Reference |
      | Tomorrow | 09:00 | 10       | RSV:Adult | 66666     |
    When I cancel sessions and bookings for the default site within a date range
      | From     | To       | CancelBookings |
      | Tomorrow | Tomorrow | true           |
    And I cancel sessions and bookings for the default site within a date range
      | From     | To       | CancelBookings |
      | Tomorrow | Tomorrow | true           |
    Then the following cancel date range metrics should be returned
      | SessionCount | BookingCount | BookingsWithoutContactDetails |
      | 0            | 0            | 0                             |

  Scenario: Cancel date range only cancels sessions within the range
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 5 days from today | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I cancel sessions and bookings for the default site within a date range
      | From     | To                | CancelBookings |
      | Tomorrow | 2 days from today | false          |
    Then the following cancel date range metrics should be returned
      | SessionCount | BookingCount | BookingsWithoutContactDetails |
      | 2            | 0            | 0                             |

  Scenario: Cancel date range where from and to dates are the same
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I cancel sessions and bookings for the default site within a date range
      | From     | To       | CancelBookings |
      | Tomorrow | Tomorrow | true           |
    Then the following cancel date range metrics should be returned
      | SessionCount | BookingCount | BookingsWithoutContactDetails |
      | 1            | 0            | 0                             |

  Scenario: Cancel date range does not count bookings already cancelled
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    And the following bookings exist at the default site
      | Date     | Time  | Duration | Service   | Reference | Status    |
      | Tomorrow | 09:00 | 10       | RSV:Adult | 111111    | Cancelled |
    When I cancel sessions and bookings for the default site within a date range
      | From     | To       | CancelBookings |
      | Tomorrow | Tomorrow | true           |
    Then the following cancel date range metrics should be returned
      | SessionCount | BookingCount | BookingsWithoutContactDetails |
      | 1            | 0            | 0                             |
