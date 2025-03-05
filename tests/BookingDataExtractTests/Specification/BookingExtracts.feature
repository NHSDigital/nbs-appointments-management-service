Feature: BookingExtracts
  As a data analyst
  I want data about the last 24 hours of booking updates
  So that I can collate information about bookings across the nation

  Scenario: Recent booking data is sent via MESH
    Given I have some bookings
      | Created On       | Booking Ref | Status |
      | 2026-01-02 17:45 | AB-01-1234  | Booked |
      | 2026-01-02 18:45 | AB-01-1235  | Booked |
    And the system is configured as follows
      | Target Mailbox | Workflow Id                     |
      | X26ABC2        | MYA_INBOUND_NATIONAL_BOOKINGS_1 |
    And the target mailbox is empty
    When the booking data extract runs on '2026-01-03 15:45'
    Then booking data is available in the target mailbox
      | Booking Ref |
      | AB-01-1234  |
      | AB-01-1235  |

  Scenario: Older booking data is not sent
    Given I have some bookings
      | Created On       | Booking Ref | Status |
      | 2026-01-01 15:45 | AB-01-1234  | Booked |
      | 2026-01-03 16:45 | AB-01-1235  | Booked |
    And the system is configured as follows
      | Target Mailbox | Workflow Id                     |
      | X26ABC2        | MYA_INBOUND_NATIONAL_BOOKINGS_1 |
    And the target mailbox is empty
    When the booking data extract runs on '2026-01-04 15:45'
    Then booking data is available in the target mailbox
      | Booking Ref |
      | AB-01-1235  |

  Scenario: Cancelled booking data is sent
    Given I have some bookings
      | Created On       | Booking Ref | Status    |
      | 2026-01-04 16:45 | AB-01-1234  | Cancelled |
      | 2026-01-04 16:45 | AB-01-1235  | Booked    |
    And the system is configured as follows
      | Target Mailbox | Workflow Id                     |
      | X26ABC2        | MYA_INBOUND_NATIONAL_BOOKINGS_1 |
    And the target mailbox is empty
    When the booking data extract runs on '2026-01-05 15:45'
    Then booking data is available in the target mailbox
      | Booking Ref |
      | AB-01-1234  |
      | AB-01-1235  |

  Scenario: Orphaned booking data is sent
    Given I have some bookings
      | Created On       | Booking Ref | Status |
      | 2026-01-08 16:45 | AB-01-1234  | Booked |
      | 2026-01-08 16:45 | AB-01-1235  | Booked |
    And the system is configured as follows
      | Target Mailbox | Workflow Id                     |
      | X26ABC2        | MYA_INBOUND_NATIONAL_BOOKINGS_1 |
    And the target mailbox is empty
    When the booking data extract runs on '2026-01-09 15:45'
    Then booking data is available in the target mailbox
      | Booking Ref |
      | AB-01-1234  |
      | AB-01-1235  |

  Scenario: Provisional booking data is not sent
    Given I have some bookings
      | Created On       | Booking Ref | Status      |
      | 2026-01-05 16:48 | AB-01-1234  | Provisional |
      | 2026-01-05 16:49 | AB-01-1235  | Booked      |
    And the system is configured as follows
      | Target Mailbox | Workflow Id                     |
      | X26ABC2        | MYA_INBOUND_NATIONAL_BOOKINGS_1 |
    And the target mailbox is empty
    When the booking data extract runs on '2026-01-06 15:45'
    Then booking data is available in the target mailbox
      | Booking Ref |
      | AB-01-1235  |

  Scenario: An empty file is sent when no records are available
    Given I have some bookings
      | Created On | Booking Ref | Status |
    And the system is configured as follows
      | Target Mailbox | Workflow Id                     |
      | X26ABC2        | MYA_INBOUND_NATIONAL_BOOKINGS_1 |
    And the target mailbox is empty
    When the booking data extract runs on '2026-01-08 15:45'
    Then an empty file is recieved

  Scenario: File is sent with the correct name
    Given I have some bookings
      | Created On | Booking Ref | Status |
    And the system is configured as follows
      | Target Mailbox | Workflow Id                     |
      | X26ABC2        | MYA_INBOUND_NATIONAL_BOOKINGS_1 |
    And the target mailbox is empty
    When the booking data extract runs on '2026-01-07 15:45'
    Then a file is recieved with the name 'MYA_booking_20260107T154500.parquet'
