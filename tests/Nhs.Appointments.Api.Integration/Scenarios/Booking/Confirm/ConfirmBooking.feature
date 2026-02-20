Feature: Book an appointment

  Scenario: Confirm a provisional appointment
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    And the following bookings exist at the default site
      | Booking Type |
      | Provisional  |
    When I confirm the booking
    Then the call should be successful
    And the first provisional booking at the default site is marked as booked

  Scenario: Confirmation can record contact details
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    And the following bookings exist at the default site
      | Reference | Booking Type |
      | 1         | Provisional  |
    When I confirm the following bookings
      | Reference | Email         | Phone         | Landline    |
      | 1         | test@test.com | 07654 3210987 | 00001234567 |
    Then the call should be successful
    And following bookings at the default site should have the following contact details
      | Reference | Email         | Phone         | Landline    |
      | 1         | test@test.com | 07654 3210987 | 00001234567 |

  Scenario: Confirmation updates LastUpdatedBy property
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    And I register and use a http client with details
      | User Id  | Role                         | Scope  |
      | nbs      | system:integration-test-user | global |
    And the following bookings exist at the default site
      | Reference | Booking Type |
      | 1         | Provisional  |
    # default state is api@test in setup
    And the booking at the default site with reference '1' has lastUpdatedBy 'api@test'
    When I confirm the following bookings
      | Reference | Email         | Phone         | Landline    |
      | 1         | test@test.com | 07654 3210987 | 00001234567 |
    Then the call should be successful
    And following bookings at the default site should have the following contact details
      | Reference | Email         | Phone         | Landline    |
      | 1         | test@test.com | 07654 3210987 | 00001234567 |
    And the booking at the default site with reference '1' has lastUpdatedBy 'api@nbs'

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
    And the following bookings exist at the default site
      | Booking Type       |
      | ExpiredProvisional |
    When I confirm the booking
    Then the call should fail with 410

  Scenario: A provisional booking expires
    Given the following sessions exist for a created default site
      | Date      | From  | Until | Services  | Slot Length | Capacity |
      | Yesterday | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    And the following bookings exist at the default site
      | Date      | Booking Type       |
      | Yesterday | ExpiredProvisional |
    When the provisional bookings are cleaned up
    Then the call should be successful
    And the first provisional booking at the default site should be deleted

  Scenario: Cannot confirm a non-provisional appointment
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    And the following bookings exist at the default site
      | Reference | Booking Type |
      | 1         | Confirmed    |
    When I confirm the following bookings
      | Reference |
      | 1         |
    Then the call should fail with 412

  Scenario: JB:Confirm provisional appointments
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    And the following bookings exist at the default site
      | Reference | Booking Type |
      | 1         | Provisional  |
      | 2         | Provisional  |
      | 3         | Provisional  |
      | 4         | Provisional  |
    When I confirm the following bookings
      | Reference | Related bookings |
      | 1         | 2, 3, 4          |
    Then the call should be successful
    And the following provisional bookings at the default site are marked as booked
      | Reference |
      | 1         |
      | 2         |
      | 3         |
      | 4         |

   Scenario: JB:Confirmation can record contact details
     Given the following sessions exist for a created default site
       | Date     | From  | Until | Services  | Slot Length | Capacity |
       | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
     And the following bookings exist at the default site
       | Reference | Booking Type |
       | 1         | Provisional  |
       | 2         | Provisional  |
       | 3         | Provisional  |
     When I confirm the following bookings
       | Reference | Related bookings | Email         | Phone         | Landline    |
       | 1         | 2, 3             | test@test.com | 07654 3210987 | 00001234567 |
     Then the call should be successful
     And following bookings at the default site should have the following contact details
       | Reference | Email         | Phone         | Landline    |
       | 1         | test@test.com | 07654 3210987 | 00001234567 |
       | 2         | test@test.com | 07654 3210987 | 00001234567 |
       | 3         | test@test.com | 07654 3210987 | 00001234567 |

   Scenario: JB:Cannot confirm appointments that do not exist
     Given the following sessions exist for a created default site
       | Date     | From  | Until | Services  | Slot Length | Capacity |
       | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
     When I confirm the following bookings
       | Reference |
       | 1         |
     Then the call should fail with 404

   Scenario: JB:Cannot confirm a provisional appointment that has expired
     Given the following sessions exist for a created default site
       | Date     | From  | Until | Services  | Slot Length | Capacity |
       | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
     And the following bookings exist at the default site
       | Booking Type       |
       | ExpiredProvisional |
       | ExpiredProvisional |
     When I confirm the following bookings
       | Reference | Related bookings |
       | 1         | 2                |
     Then the call should fail with 410

   Scenario: JB:A provisional booking expires
     Given the following sessions exist for a created default site
       | Date      | From  | Until | Services  | Slot Length | Capacity |
       | Yesterday | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
     And the following bookings exist at the default site
       | Date      | Booking Type       |
       | Yesterday | ExpiredProvisional |
       | Yesterday | ExpiredProvisional |
     When the provisional bookings are cleaned up
     Then the call should be successful
     And the first provisional booking at the default site should be deleted

   Scenario: JB:Cannot confirm a non-provisional appointment
     Given the following sessions exist for a created default site
       | Date     | From  | Until | Services  | Slot Length | Capacity |
       | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
     And the following bookings exist at the default site
       | Reference | Date     | Booking Type |
       | 1         | Tomorrow | Confirmed    |
       | 2         | Tomorrow | Confirmed    |
     When I confirm the following bookings
       | Reference | Related bookings |
       | 1         | 2                |
     Then the call should fail with 410

  Scenario: JB:Confirmation can record batch size
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    And the following bookings exist at the default site
      | Reference | Booking Type |
      | 1         | Provisional  |
      | 2         | Provisional  |
      | 3         | Provisional  |
    When I confirm the following bookings
      | Reference | Related bookings | Email         | Phone         | Landline    |
      | 1         | 2, 3             | test@test.com | 07654 3210987 | 00001234567 |
    Then the call should be successful
    And following bookings at the default site should have the following batch size
      | Reference | Booking batch size |
      | 1         | 3                  |
      | 2         | 3                  |
      | 3         | 3                  |
