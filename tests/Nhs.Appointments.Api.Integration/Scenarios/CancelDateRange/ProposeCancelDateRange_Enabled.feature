Feature: Propose cancel a date range

  Scenario: Propose cancel a date range
    Given the following default site exists in the system
      | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    When I propose cancelling sessions and bookings for the default site within a date range
      | From     | To                |
      | Tomorrow | 3 days from today |
    Then the following proposed cancel a date range metrics should be returned
      | SessionCount | BookingCount |
      | 0            | 0            |

  Scenario: Propose cancel a date range bad request when from date is after to date
    Given the following default site exists in the system
      | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    When I propose cancelling sessions and bookings for the default site within a date range
      | From              | To       |
      | 3 days from today | Tomorrow |
    Then the call should fail with 400

  Scenario: Propose cancel a date range bad request when from date is in the past
    Given the following default site exists in the system
      | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    When I propose cancelling sessions and bookings for the default site within a date range
      | From      | To       |
      | Yesterday | Tomorrow |
    Then the call should fail with 400

  Scenario: Propose cancel a date range bad request when to date is in the past
    Given the following default site exists in the system
      | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    When I propose cancelling sessions and bookings for the default site within a date range
      | From     | To        |
      | Tomorrow | Yesterday |
    Then the call should fail with 400

  Scenario: Propose cancel a date range bad request when from date over 3 months after to date
    Given the following default site exists in the system
      | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    When I propose cancelling sessions and bookings for the default site within a date range
      | From     | To                  |
      | Tomorrow | 4 months from today |
    Then the call should fail with 400

  Scenario: Propose cancel a date range returns session count with zero bookings
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 3 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I propose cancelling sessions and bookings for the default site within a date range
      | From     | To                |
      | Tomorrow | 3 days from today |
    Then the following proposed cancel a date range metrics should be returned
      | SessionCount | BookingCount |
      | 3            | 0            |

  Scenario: Propose cancel a date range returns session and booking count
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 3 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    And the following bookings have been made at the default site
      | Date              | Time  | Duration  | Service   |
      | Tomorrow          | 09:00 | 10        | RSV:Adult |
      | 2 days from today | 09:10 | 10        | RSV:Adult |
      | 3 days from today | 09:10 | 10        | RSV:Adult |
    When I propose cancelling sessions and bookings for the default site within a date range
      | From     | To                |
      | Tomorrow | 3 days from today |
    Then the following proposed cancel a date range metrics should be returned
      | SessionCount | BookingCount |
      | 3            | 3            |

  Scenario: Does not include cancelled bookings in the booking count
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 3 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    And the following bookings have been made at the default site
      | Date              | Time  | Duration  | Service   | Reference   |
      | Tomorrow          | 09:00 | 10        | RSV:Adult | 24680-97531 |
      | 2 days from today | 09:10 | 10        | RSV:Adult | 12345-67890 |
      | 3 days from today | 09:10 | 10        | RSV:Adult | 54321-09876 |
    When I cancel the booking at the default site with reference '24680-97531'
    When I propose cancelling sessions and bookings for the default site within a date range
      | From     | To                |
      | Tomorrow | 3 days from today |
    Then the following proposed cancel a date range metrics should be returned
      | SessionCount | BookingCount |
      | 3            | 2            |

  Scenario: Does not include cancelled sessions in the session count
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 3 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I cancel the following session at the default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | RSV:Adult | 10           | 1        |
    When I propose cancelling sessions and bookings for the default site within a date range
      | From     | To                |
      | Tomorrow | 3 days from today |
    Then the following proposed cancel a date range metrics should be returned
      | SessionCount | BookingCount |
      | 2            | 0            |

  Scenario: Propose cancel a date range counts multiple bookings within a session
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | RSV:Adult | 10          | 5        |
    And the following bookings have been made at the default site
      | Date     | Time  | Duration | Service   |
      | Tomorrow | 09:00 | 10       | RSV:Adult |
      | Tomorrow | 09:10 | 10       | RSV:Adult |
      | Tomorrow | 09:20 | 10       | RSV:Adult |
    When I propose cancelling sessions and bookings for the default site within a date range
      | From     | To       |
      | Tomorrow | Tomorrow |
    Then the following proposed cancel a date range metrics should be returned
      | SessionCount | BookingCount |
      | 1            | 3            |

  Scenario: Propose cancel a date range only counts sessions within the specified range
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 5 days from today | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    And the following bookings have been made at the default site
      | Date              | Time  | Duration | Service   |
      | Tomorrow          | 09:00 | 10       | RSV:Adult |
      | 2 days from today | 09:10 | 10       | RSV:Adult |
      | 5 days from today | 09:10 | 10       | RSV:Adult |
    When I propose cancelling sessions and bookings for the default site within a date range
      | From     | To                |
      | Tomorrow | 2 days from today |
    Then the following proposed cancel a date range metrics should be returned
      | SessionCount | BookingCount |
      | 2            | 2            |

  Scenario: Propose cancel a date range for a single day
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    And the following bookings have been made at the default site
      | Date     | Time  | Duration | Service   |
      | Tomorrow | 09:00 | 10       | RSV:Adult |
    When I propose cancelling sessions and bookings for the default site within a date range
      | From     | To       |
      | Tomorrow | Tomorrow |
    Then the following proposed cancel a date range metrics should be returned
      | SessionCount | BookingCount |
      | 1            | 1            |


  Scenario: Propose cancel a date range counts bookings across multiple services
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services                       | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult, COVID_FLU:65+, FLU:2_3       | 10          | 2        |
    And the following bookings have been made at the default site
      | Date     | Time  | Duration | Service        |
      | Tomorrow | 09:00 | 10       | RSV:Adult      |
      | Tomorrow | 09:10 | 10       | COVID_FLU:65+      |
      | Tomorrow | 09:20 | 10       | FLU:2_3        |
    When I propose cancelling sessions and bookings for the default site within a date range
      | From     | To       |
      | Tomorrow | Tomorrow |
    Then the following proposed cancel a date range metrics should be returned
      | SessionCount | BookingCount |
      | 1            | 3            |


  Scenario: Propose cancel a date range counts overlapping sessions correctly
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 2        |
      | Tomorrow | 11:00 | 15:00 | RSV:Adult | 10          | 2        |
    And the following bookings have been made at the default site
      | Date     | Time  | Duration | Service   |
      | Tomorrow | 09:00 | 10       | RSV:Adult |
      | Tomorrow | 11:10 | 10       | RSV:Adult |
      | Tomorrow | 14:20 | 10       | RSV:Adult |
    When I propose cancelling sessions and bookings for the default site within a date range
      | From     | To       |
      | Tomorrow | Tomorrow |
    Then the following proposed cancel a date range metrics should be returned
      | SessionCount | BookingCount |
      | 2            | 3            |
