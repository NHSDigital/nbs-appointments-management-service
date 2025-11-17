Feature: Availability Edit Proposal Enabled

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
      | newlySupportedBookingsCount   | 0 |
      | newlyUnsupportedBookingsCount    | 1 |

  Scenario: Update a single session on a single day - metrics dont include currently orphaned bookings
    Given the site is configured for MYA
    When I create the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services    |
      | Tomorrow | 09:00 | 10:00 | 10         | 1        | Green,Blue  |
    And the following bookings have been made
      | Date     | Time  | Duration  | Service | Reference   |
      | Tomorrow | 09:20 | 10        | Blue    | 65734-19232 |
    And the following orphaned bookings exist
      | Date     | Time  | Duration  | Service | Reference   |
      | Tomorrow | 09:20 | 10        | Yellow  | 61865-10293 |
    When I request the availability proposal for potential availability change
      | Type        | RequestFrom | RequestTo | From  | Until | Services   | SlotLength | Capacity |
      | Matcher     | Tomorrow    | Tomorrow  | 09:00 | 10:00 | Green,Blue | 10         | 1        |
      | Replacement |             |           | 09:00 | 10:00 | Green      | 10         | 1        |
    Then the following count is returned
      | newlySupportedBookingsCount   | 0 |
      | newlyUnsupportedBookingsCount    | 1 |

  Scenario: Update a single session on a single day - metrics include swapped over support
    Given the site is configured for MYA
    When I create the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services    |
      | Tomorrow | 09:00 | 10:00 | 10         | 3        | Green,Blue  |
    And the following bookings have been made
      | Date     | Time  | Duration  | Service | Reference   |
      | Tomorrow | 09:20 | 10        | Blue    | 65734-19232 |
      | Tomorrow | 09:20 | 10        | Blue    | 75734-19232 |
      | Tomorrow | 09:20 | 10        | Blue    | 85734-19232 |
    And the following orphaned bookings exist
      | Date     | Time  | Duration  | Service | Reference   |
      | Tomorrow | 09:20 | 10        | Green   | 61865-10294 |
      | Tomorrow | 09:20 | 10        | Green   | 61865-10295 |
      | Tomorrow | 09:20 | 10        | Green   | 61865-10296 |
    When I request the availability proposal for potential availability change
      | Type        | RequestFrom | RequestTo | From  | Until | Services   | SlotLength | Capacity |
      | Matcher     | Tomorrow    | Tomorrow  | 09:00 | 10:00 | Green,Blue | 10         | 3        |
      | Replacement |             |           | 09:00 | 10:00 | Green      | 10         | 3        |
    Then the following count is returned
      | newlySupportedBookingsCount   | 3 |
      | newlyUnsupportedBookingsCount    | 3 |

  Scenario: Update a single session on a single day - metrics show greedy inefficiency
    Given the site is configured for MYA
    When I create the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services    |
      | Tomorrow | 09:00 | 10:00 | 10         | 3        | A,B,C,E,F   |
      | Tomorrow | 09:00 | 10:00 | 10         | 3        | A,B,D,E,F  |
    And the following bookings have been made
      | Date     | Time  | Duration  | Service | Reference   |
      | Tomorrow | 09:20 | 10        | A       | 65734-19232 |
      | Tomorrow | 09:20 | 10        | A       | 75734-19232 |
      | Tomorrow | 09:20 | 10        | A       | 85734-19232 |
      | Tomorrow | 09:20 | 10        | D       | 95734-19232 |
      | Tomorrow | 09:20 | 10        | D       | 05734-19232 |
      | Tomorrow | 09:20 | 10        | D       | 15734-19232 |
    When I request the availability proposal for potential availability change
      | Type        | RequestFrom | RequestTo | From  | Until | Services   | SlotLength | Capacity |
      | Matcher     | Tomorrow    | Tomorrow  | 09:00 | 10:00 | A,B,D,E,F  | 10         | 3        |
      | Replacement |             |           | 09:00 | 10:00 | A,B,D,E    | 10         | 3        |
    Then the following count is returned
      | newlySupportedBookingsCount   | 0 |
      | newlyUnsupportedBookingsCount    | 3 |

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
      | newlySupportedBookingsCount   | 0 |
      | newlyUnsupportedBookingsCount    | 3 |

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
      | newlySupportedBookingsCount   | 0 |
      | newlyUnsupportedBookingsCount    | 3 |

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
      | newlySupportedBookingsCount   | 0 |
      | newlyUnsupportedBookingsCount    | 1 |
    
# Wildcard cancellation not yet implemented
  @ignore
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
      | newlySupportedBookingsCount   | 0 |
      | newlyUnsupportedBookingsCount    | 3 |

# Wildcard cancellation not yet implemented
  @ignore
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
      | newlySupportedBookingsCount   | 0 |
      | newlyUnsupportedBookingsCount | 9 |
