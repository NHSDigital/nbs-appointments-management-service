Feature: Book an appointment

    Scenario: Confirm a provisional appointment
      Given the site is configured for MYA
      And the following sessions
        | Date       | From  | Until | Services | Slot Length | Capacity |
        | 2077-01-01 | 09:00 | 10:00 | COVID    | 5           | 1        |
      And the following provisional bookings have been made
        | Date       | Time  | Duration | Service | 
        | 2077-01-01 | 09:00 | 5        | COVID   |
      When I confirm the booking
      Then the call should be successful
      And the booking is no longer marked as provisional
