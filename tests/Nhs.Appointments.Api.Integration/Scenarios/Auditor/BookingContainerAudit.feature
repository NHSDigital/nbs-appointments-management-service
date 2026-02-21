Feature: Audit Trail Synchronization - Availability, Booking

  Scenario: Create Availability, file appears in Blob
    Given there is no existing availability for a created default site
    When I apply the following availability template
      | From     | Until    | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services | Mode     |
      | Tomorrow | Tomorrow | Relative | 09:00    | 10:00     | 5          | 1        | COVID    | Additive |
    Then the request is successful and the following daily availability sessions are created
      | Date              | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the availability should be audited in StorageAccount

  Scenario: Cancel Availability, file appears in Blob
    Given there is no existing availability for a created default site
    When I apply the following availability template
      | From     | Until    | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services | Mode     |
      | Tomorrow | Tomorrow | Relative | 09:00    | 10:00     | 5          | 1        | COVID    | Additive |
    Then the request is successful and the following daily availability sessions are created
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    When I cancel the following session
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the availability should be audited in StorageAccount

  Scenario: Create, reschedule and confirm Booking, files appears in Blob
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    When I make a provisional appointment with the following details
      | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email             | Phone      |
      | Tomorrow | 09:20 | 5        | COVID   | 1234678891 | Test      | One      | 2000-02-01 | testEmail@nhs.net | 0777777777 |
    And a sanitized version of the booking should be audited in StorageAccount
    And the booking index should be audited in StorageAccount
    When I make a provisional appointment with the following details
      | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email             | Phone      |
      | Tomorrow | 09:30 | 5        | COVID   | 1234678891 | Test      | One      | 2000-02-01 | testEmail@nhs.net | 0777777777 |
    And a sanitized version of the booking should be audited in StorageAccount
    And the booking index should be audited in StorageAccount
    And I confirm the rescheduled booking
    Then the rescheduled booking is no longer marked as provisional
    And a sanitized version of the booking should be audited in StorageAccount
    And the booking index should be audited in StorageAccount

