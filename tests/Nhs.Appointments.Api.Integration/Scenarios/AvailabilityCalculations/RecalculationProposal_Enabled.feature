Feature: Availability Recalculation Proposal Enabled

  Scenario: Recalculate bookings for proposes availability change
    Given the site is configured for MYA
    When I create the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services    |
      | Tomorrow | 09:00 | 10:00 | 10         | 1        | Green,Blue  |
      | Tomorrow | 09:00 | 10:00 | 10         | 1        | Blue,Orange |
      | Tomorrow | 09:00 | 10:00 | 10         | 1        | Orange,Blue |
    Then I make the following bookings
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:00 | 10       | Blue    |
      | Tomorrow | 09:00 | 10       | Orange  |
      | Tomorrow | 09:00 | 10       | Blue    |
    When I request the availability proposal for potential availability change
      | From  | Until | Services   | SlotLength | Capacity |
      | 09:00 | 10:00 | Green,Blue | 10         | 1        |
      | 09:00 | 10:00 | Green      | 10         | 1        |
    Then the following count is returned
      | supported   | 2 |
      | unsupported | 1 |
