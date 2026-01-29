Feature: Book an appointment

  Scenario: Confirm a provisional appointment
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    And the following bookings exist
      | Booking Type |
      | Provisional  |
    When I confirm the booking
    Then the call should be successful
    And the booking is no longer marked as provisional

  Scenario: Confirmation can record contact details
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    And the following bookings exist
      | Reference | Booking Type |
      | 1         | Provisional  |
    When I confirm the following bookings
      | Reference | Email         | Phone         | Landline    |
      | 1         | test@test.com | 07654 3210987 | 00001234567 |
    Then the call should be successful
    And following bookings should have the following contact details
      | Reference | Email         | Phone         | Landline    |
      | 1         | test@test.com | 07654 3210987 | 00001234567 |

    Scenario: Confirmation updates LastUpdatedBy property
      Given the following sessions exist for a created default site
        | Date     | From  | Until | Services  | Slot Length | Capacity |
        | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
      And a new api user 'nbs' is registered with a http client
      And the following bookings exist
        | Reference | Booking Type |
        | 1         | Provisional  |
      # default state is api@test in setup
      And the booking document with reference '1' has lastUpdatedBy 'api@test'
      When I confirm the following bookings for api user 'nbs'
        | Reference | Email         | Phone         | Landline    |
        | 1         | test@test.com | 07654 3210987 | 00001234567 |
      Then the call should be successful
      And following bookings should have the following contact details
        | Reference | Email         | Phone         | Landline    |
        | 1         | test@test.com | 07654 3210987 | 00001234567 |
      And the booking document with reference '1' has lastUpdatedBy 'api@nbs'

  Scenario: Cannot confirm an appointment that does not exist
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    When I confirm the booking
    Then the call should fail with 404

  Scenario: Cannot confirm a provisional appointment that has expired
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    And the following bookings exist
      | Booking Type       |
      | ExpiredProvisional |
    When I confirm the booking
    Then the call should fail with 410

  Scenario: A provisional booking expires
    Given the following sessions exist for a created default site
      | Date      | From  | Until | Services  | Slot Length | Capacity |
      | Yesterday | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    And the following bookings exist
      | Date      | Booking Type       |
      | Yesterday | ExpiredProvisional |
    When the provisional bookings are cleaned up
    Then the call should be successful
    And the booking should be deleted

  Scenario: Cannot confirm a non-provisional appointment
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    And the following bookings exist
      | Reference | Booking Type |
      | 1         | Confirmed    |
    When I confirm the following bookings
      | Reference |
      | 1         |
    Then the call should fail with 412
