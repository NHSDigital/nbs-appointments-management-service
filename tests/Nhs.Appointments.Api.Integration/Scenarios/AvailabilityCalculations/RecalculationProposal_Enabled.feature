Feature: Availability Recalculation Proposal Enabled

  Scenario: Update a single session on a single day
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
      | Type        | RequestFrom | RequestTo | From  | Until | Services   | SlotLength | Capacity |
      | Matcher     | Tomorrow    | Tomorrow  | 09:00 | 10:00 | Green,Blue | 10         | 1        |
      | Replacement |             |           | 09:00 | 10:00 | Green      | 10         | 1        |
    Then the following count is returned
      | supported   | 2 |
      | unsupported | 1 |

  Scenario: Update a session across multiple days
    Given the site is configured for MYA
    When I create the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services    |
      | Today +1 | 09:00 | 10:00 | 10         | 1        | Green,Blue  |
      | Today +1 | 09:00 | 10:00 | 10         | 1        | Blue,Orange |
      | Today +1 | 09:00 | 10:00 | 10         | 1        | Orange,Blue |
      | Today +2 | 09:00 | 10:00 | 10         | 1        | Green,Blue  |
      | Today +2 | 09:00 | 10:00 | 10         | 1        | Blue,Orange |
      | Today +2 | 09:00 | 10:00 | 10         | 1        | Orange,Blue |
      | Today +3 | 09:00 | 10:00 | 10         | 1        | Green,Blue  |
      | Today +3 | 09:00 | 10:00 | 10         | 1        | Blue,Orange |
      | Today +3 | 09:00 | 10:00 | 10         | 1        | Orange,Blue |
    Then I make the following bookings
      | Date     | Time  | Duration | Service |
      | Today +1 | 09:00 | 10       | Blue    |
      | Today +1 | 09:00 | 10       | Orange  |
      | Today +1 | 09:00 | 10       | Blue    |
      | Today +2 | 09:00 | 10       | Blue    |
      | Today +2 | 09:00 | 10       | Orange  |
      | Today +2 | 09:00 | 10       | Blue    |
      | Today +3 | 09:00 | 10       | Blue    |
      | Today +3 | 09:00 | 10       | Orange  |
      | Today +3 | 09:00 | 10       | Blue    |
    When I request the availability proposal for potential availability change
      | Type        | RequestFrom | RequestTo | From  | Until | Services   | SlotLength | Capacity |
      | Matcher     | Today +1    | Today +3  | 09:00 | 10:00 | Green,Blue | 10         | 1        |
      | Replacement |             |           | 09:00 | 10:00 | Green      | 10         | 1        |
    Then the following count is returned
      | supported   | 6 |
      | unsupported | 3 |

  Scenario: Cancel a session across multiple days
    Given the site is configured for MYA
    When I create the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services    |
      | Today +1 | 09:00 | 10:00 | 10         | 1        | Green,Blue  |
      | Today +1 | 09:00 | 10:00 | 10         | 1        | Blue,Orange |
      | Today +1 | 09:00 | 10:00 | 10         | 1        | Orange,Blue |
      | Today +2 | 09:00 | 10:00 | 10         | 1        | Green,Blue  |
      | Today +2 | 09:00 | 10:00 | 10         | 1        | Blue,Orange |
      | Today +2 | 09:00 | 10:00 | 10         | 1        | Orange,Blue |
      | Today +3 | 09:00 | 10:00 | 10         | 1        | Green,Blue  |
      | Today +3 | 09:00 | 10:00 | 10         | 1        | Blue,Orange |
      | Today +3 | 09:00 | 10:00 | 10         | 1        | Orange,Blue |
    Then I make the following bookings
      | Date     | Time  | Duration | Service |
      | Today +1 | 09:00 | 10       | Blue    |
      | Today +1 | 09:00 | 10       | Orange  |
      | Today +1 | 09:00 | 10       | Blue    |
      | Today +2 | 09:00 | 10       | Blue    |
      | Today +2 | 09:00 | 10       | Orange  |
      | Today +2 | 09:00 | 10       | Blue    |
      | Today +3 | 09:00 | 10       | Blue    |
      | Today +3 | 09:00 | 10       | Orange  |
      | Today +3 | 09:00 | 10       | Blue    |
    When I request the availability proposal for potential availability change
      | Type        | RequestFrom | RequestTo | From  | Until | Services   | SlotLength | Capacity |
      | Matcher     | Today +1    | Today +3  | 09:00 | 10:00 | Green,Blue | 10         | 1        |
    Then the following count is returned
      | supported   | 6 |
      | unsupported | 3 |

  Scenario: Cancel a session on a single day
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
      | Type        | RequestFrom | RequestTo | From  | Until | Services   | SlotLength | Capacity |
      | Matcher     | Tomorrow    | Tomorrow  | 09:00 | 10:00 | Green,Blue | 10         | 1        |
    Then the following count is returned
      | supported   | 2 |
      | unsupported | 1 |

  Scenario: Cancel all sessions on a single day
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
      | Type        | RequestFrom | RequestTo | From  | Until | Services   | SlotLength | Capacity |
    Then the following count is returned
      | supported   | 0 |
      | unsupported | 3 |

  Scenario: Cancel all sessions across multiple days
    Given the site is configured for MYA
    When I create the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services    |
      | Today +1 | 09:00 | 10:00 | 10         | 1        | Green,Blue  |
      | Today +1 | 09:00 | 10:00 | 10         | 1        | Blue,Orange |
      | Today +1 | 09:00 | 10:00 | 10         | 1        | Orange,Blue |
      | Today +2 | 09:00 | 10:00 | 10         | 1        | Green,Blue  |
      | Today +2 | 09:00 | 10:00 | 10         | 1        | Blue,Orange |
      | Today +2 | 09:00 | 10:00 | 10         | 1        | Orange,Blue |
      | Today +3 | 09:00 | 10:00 | 10         | 1        | Green,Blue  |
      | Today +3 | 09:00 | 10:00 | 10         | 1        | Blue,Orange |
      | Today +3 | 09:00 | 10:00 | 10         | 1        | Orange,Blue |
    Then I make the following bookings
      | Date     | Time  | Duration | Service |
      | Today +1 | 09:00 | 10       | Blue    |
      | Today +1 | 09:00 | 10       | Orange  |
      | Today +1 | 09:00 | 10       | Blue    |
      | Today +2 | 09:00 | 10       | Blue    |
      | Today +2 | 09:00 | 10       | Orange  |
      | Today +2 | 09:00 | 10       | Blue    |
      | Today +3 | 09:00 | 10       | Blue    |
      | Today +3 | 09:00 | 10       | Orange  |
      | Today +3 | 09:00 | 10       | Blue    |
    When I request the availability proposal for potential availability change
      | Type        | RequestFrom | RequestTo | From  | Until | Services   | SlotLength | Capacity |
      |             | Today +1    | Today +3  |       |       |            |            |          |
    Then the following count is returned
      | supported   | 0 |
      | unsupported | 9 |
