Feature: Book an appointment

    Scenario: Confirm a provisional appointment
        Given the site is configured for MYA
        And the following sessions
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
        And the following provisional bookings have been made
          | Date     | Time  | Duration | Service |
          | Tomorrow | 09:00 | 5        | COVID   |
        When I confirm the booking
        Then the call should be successful
        And the booking is no longer marked as provisional

    Scenario: Confirmation can record contact details
        Given the site is configured for MYA
        And the following sessions
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
        And the following provisional bookings have been made
          | Date     | Time  | Duration | Service |
          | Tomorrow | 09:00 | 5        | COVID   |
        When I confirm the booking with the following contact information
          | Email         | Phone         |
          | test@test.com | 07654 3210987 |
        Then the call should be successful
        And the booking should have stored my contact details as follows
          | Email         | Phone         |
          | test@test.com | 07654 3210987 |

    Scenario: Cannot confirm an appointment that does not exist
        Given the site is configured for MYA
        And the following sessions
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
        When I confirm the booking
        Then the call should fail with 404

    Scenario: Cannot confirm a provisional appointment that has expired
        Given the site is configured for MYA
        And the following sessions
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
        And the following expired provisional bookings have been made
          | Date     | Time  | Duration | Service |
          | Tomorrow | 09:00 | 5        | COVID   |
        When I confirm the booking
        Then the call should fail with 410

    Scenario: A provisional booking expires
        Given the site is configured for MYA
        And the following sessions
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Yesterday | 09:00 | 10:00 | COVID    | 5           | 1        |
        And the following provisional bookings have been made
          | Date     | Time  | Duration | Service |
          | Yesterday | 09:00 | 5        | COVID   |
        When the provisional bookings are cleaned up
        Then the call should be successful
        And the booking should be deleted