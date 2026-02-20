Feature: Availability Edit Proposal Disabled

  Scenario: Recalculate availability change proposal
    Given the default site exists
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
    When I propose an availability edit at the default site with the change
      | Type        | RequestFrom | RequestTo | From  | Until | Services   | SlotLength | Capacity |
      | Matcher     | Tomorrow    | Tomorrow  | 09:00 | 10:00 | Green,Blue | 10         | 1        |
      | Replacement |             |           | 09:00 | 10:00 | Green      | 10         | 1        |
    Then the call should fail with 501

