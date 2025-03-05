Feature: CapacityExtracts
  As a data analyst
  I want data about the next 90 days of capacity
  So that I can collate information about capacity across the nation

  Scenario: Recent capacity data is sent via MESH
    Given I have some capacity
      | From             | Until            |
      | 2026-01-02 09:00 | 2026-01-02 09:05 |
      | 2026-01-02 09:05 | 2026-01-02 09:10 |
    And the system is configured as follows
      | Target Mailbox | Workflow Id                              |
      | X26ABC2        | MYA_INBOUND_NATIONAL_BOOKINGS_CAPACITY_1 |
    And the target mailbox is empty
    When the capacity data extract runs on '2026-01-01 15:45'
    Then capacity data is available in the target mailbox
      | Date       | Time  | SlotLength |
      | 02/01/2026 | 09:00 | 05         |
      | 02/01/2026 | 09:05 | 05         |

  Scenario: Older capacity data is not sent
    Given I have some capacity
      | From             | Until            |
      | 2026-01-01 09:00 | 2026-01-01 09:05 |
      | 2026-01-02 09:05 | 2026-01-02 09:10 |
    And the system is configured as follows
      | Target Mailbox | Workflow Id                              |
      | X26ABC2        | MYA_INBOUND_NATIONAL_BOOKINGS_CAPACITY_1 |
    And the target mailbox is empty
    When the capacity data extract runs on '2026-01-02 02:00'
    Then capacity data is available in the target mailbox
      | Date       | Time  | SlotLength |
      | 02/01/2026 | 09:00 | 05         |

  Scenario: An empty file is sent when no records are available
    Given I have some bookings
      | From | Until |
    And the system is configured as follows
      | Target Mailbox | Workflow Id                              |
      | X26ABC2        | MYA_INBOUND_NATIONAL_BOOKINGS_CAPACITY_1 |
    And the target mailbox is empty
    When the capacity data extract runs on '2026-01-08 15:45'
    Then an empty file is recieved

  Scenario: File is sent with the correct name
    Given I have some bookings
      | From | Until |
    And the system is configured as follows
      | Target Mailbox | Workflow Id                              |
      | X26ABC2        | MYA_INBOUND_NATIONAL_BOOKINGS_CAPACITY_1 |
    And the target mailbox is empty
    When the capacity data extract runs on '2026-01-07 15:45'
    Then a file is recieved with the name 'MYA_BookingCapacity_20260107T15450000.parquet'
