Feature: Cancel date range

  Scenario: Cancel sessions in a date range
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

  Scenario: Cancel date range returns 501 response when cancel bookings is true
    Given the following default site exists in the system
      | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    When I cancel sessions and bookings for the default site within a date range
      | From     | To       | CancelBookings |
      | Tomorrow | 2 days from today | true           |
    Then the call should fail with 501

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

  Scenario: Cancel date range excludes cancelled sessions from the session count
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

  Scenario: Cancel date range retains bookings when cancel bookings is false
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 3 days from today | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    And the following bookings exist at the default site
      | Date              | Time  | Duration  | Service   | Reference   |
      | Tomorrow          | 09:00 | 10        | RSV:Adult | 42685-95135 |
      | 2 days from today | 09:10 | 10        | RSV:Adult | 04283-97513 |
      | 3 days from today | 09:10 | 10        | RSV:Adult | 38172-95124 |
    When I cancel sessions and bookings for the default site within a date range
      | From     | To                | CancelBookings |
      | Tomorrow | 3 days from today | false          |
    Then the following cancel date range metrics should be returned
      | SessionCount | BookingCount | BookingsWithoutContactDetails |
      | 3            | 0           | 0                             |
    When I check daily availability for the default site between 'Tomorrow' and '3 days from today'
    Then the daily availability should be empty
    Then the following bookings at the default site are now in the following state
      | Reference   | Status |
      | 42685-95135 | Booked |
      | 04283-97513 | Booked |
      | 38172-95124 | Booked |
