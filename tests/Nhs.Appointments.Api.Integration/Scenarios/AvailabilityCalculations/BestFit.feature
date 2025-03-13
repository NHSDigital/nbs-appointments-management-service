Feature: The Best Fit Problem

  Scenario: Control test - no best fit problem
    Given the site is configured for MYA
    When I create the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services |
      | Tomorrow | 09:00 | 10:00 | 10         | 1        | Blue     |
      | Tomorrow | 09:00 | 10:00 | 10         | 1        | Orange   |
      | Tomorrow | 09:00 | 10:00 | 10         | 1        | Blue     |
    Then I make the following bookings
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:00 | 10       | Blue    |
      | Tomorrow | 09:00 | 10       | Orange  |
      | Tomorrow | 09:00 | 10       | Blue    |
    When I query the current bookings
    Then the following bookings are returned
      | Date     | Time  | Duration | Service | Status    |
      | Tomorrow | 09:00 | 10       | Blue    | Supported |
      | Tomorrow | 09:00 | 10       | Orange  | Supported |
      | Tomorrow | 09:00 | 10       | Blue    | Supported |

  # Booking 2 (Orange) goes in session 3 (Orange,Blue), leaving no space for booking 3 (blue)
  # Booking 2 should instead go in session 2, leaving space for booking 3 to go in session 3
  @ignore
  Scenario: The best fit problem
    Given the site is configured for MYA
    When I create the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services     |
      | Tomorrow | 09:00 | 10:00 | 10         | 1        | Green,Blue   |
      | Tomorrow | 09:00 | 10:00 | 10         | 1        | Green,Orange |
      | Tomorrow | 09:00 | 10:00 | 10         | 1        | Orange,Blue  |
    Then I make the following bookings
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:00 | 10       | Blue    |
      | Tomorrow | 09:00 | 10       | Orange  |
      | Tomorrow | 09:00 | 10       | Blue    |
    When I query the current bookings
    Then the following bookings are returned
      | Date     | Time  | Duration | Service | Status    |
      | Tomorrow | 09:00 | 10       | Blue    | Supported |
      | Tomorrow | 09:00 | 10       | Orange  | Supported |
      | Tomorrow | 09:00 | 10       | Blue    | Supported |

  Scenario: Cancelling a session orphans the last created booking
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
    Then I cancel the following sessions
      | Date     | From  | Until | Blue        | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | Blue,Orange | 10          | 1        |
    When I query the current bookings
    Then the following bookings are returned
      | Date     | Time  | Duration | Service | Status    |
      | Tomorrow | 09:00 | 10       | Blue    | Supported |
      | Tomorrow | 09:00 | 10       | Orange  | Supported |
      | Tomorrow | 09:00 | 10       | Blue    | Orphaned  |

  Scenario: Re-supporting orphans prioritises first in
    Given the site is configured for MYA
    When I create the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services    |
      | Tomorrow | 09:00 | 10:00 | 10         | 1        | Green       |
      | Tomorrow | 09:00 | 10:00 | 10         | 2        | Blue,Orange |
    Then I make the following bookings
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:00 | 10       | Green   |
      | Tomorrow | 09:00 | 10       | Orange  |
      | Tomorrow | 09:00 | 10       | Blue    |
    Then I cancel the following sessions
      | Date     | From  | Until | Blue        | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | Blue,Orange | 10          | 2        |
    When I query the current bookings
    Then the following bookings are returned
      | Date     | Time  | Duration | Service | Status    |
      | Tomorrow | 09:00 | 10       | Green   | Supported |
      | Tomorrow | 09:00 | 10       | Orange  | Orphaned  |
      | Tomorrow | 09:00 | 10       | Blue    | Orphaned  |
    When I create the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services    |
      | Tomorrow | 09:00 | 10:00 | 10         | 1        | Blue,Orange |
    When I query the current bookings
    Then the following bookings are returned
      | Date     | Time  | Duration | Service | Status    |
      | Tomorrow | 09:00 | 10       | Green   | Supported |
      | Tomorrow | 09:00 | 10       | Orange  | Supported |
      | Tomorrow | 09:00 | 10       | Blue    | Orphaned  |

  Scenario: Appointments "shuffle" along as sessions are created
    Given the site is configured for MYA

    ## Step 1: Create 3 bookings
    When I create the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services                 |
      | Tomorrow | 09:00 | 10:00 | 10         | 3        | Green, Orange, Blue, Red |
    Then I make the following bookings
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:00 | 10       | Green   |
      | Tomorrow | 09:00 | 10       | Orange  |
      | Tomorrow | 09:00 | 10       | Blue    |
    When I query the current bookings
    Then the following bookings are returned
      | Date     | Time  | Duration | Service | Status    |
      | Tomorrow | 09:00 | 10       | Green   | Supported |
      | Tomorrow | 09:00 | 10       | Orange  | Supported |
      | Tomorrow | 09:00 | 10       | Blue    | Supported |

    ## Step 2: Remove support for all 3
    Then I cancel the following sessions
      | Date     | From  | Until | Blue                     | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | Green, Orange, Blue, Red | 10          | 3        |
    When I query the current bookings
    Then the following bookings are returned
      | Date     | Time  | Duration | Service | Status   |
      | Tomorrow | 09:00 | 10       | Green   | Orphaned |
      | Tomorrow | 09:00 | 10       | Orange  | Orphaned |
      | Tomorrow | 09:00 | 10       | Blue    | Orphaned |

    ## Step 3: Create availability for the 3rd booking
    When I create the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services |
      | Tomorrow | 09:00 | 10:00 | 10         | 1        | Blue     |
    When I query the current bookings
    Then the following bookings are returned
      | Date     | Time  | Duration | Service | Status    |
      | Tomorrow | 09:00 | 10       | Green   | Orphaned  |
      | Tomorrow | 09:00 | 10       | Orange  | Orphaned  |
      | Tomorrow | 09:00 | 10       | Blue    | Supported |

    ## Step 4: Create availability for the 2nd booking
    When I create the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services     |
      | Tomorrow | 09:00 | 10:00 | 10         | 1        | Blue, Orange |
    When I query the current bookings
    Then the following bookings are returned
      | Date     | Time  | Duration | Service | Status    |
      | Tomorrow | 09:00 | 10       | Green   | Orphaned  |
      | Tomorrow | 09:00 | 10       | Orange  | Supported |
      | Tomorrow | 09:00 | 10       | Blue    | Supported |

    ## Step 5: Create availability for the 3rd booking
    When I create the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services      |
      | Tomorrow | 09:00 | 10:00 | 10         | 1        | Orange, Green |
    When I query the current bookings
    Then the following bookings are returned
      | Date     | Time  | Duration | Service | Status    |
      | Tomorrow | 09:00 | 10       | Green   | Supported |
      | Tomorrow | 09:00 | 10       | Orange  | Supported |
      | Tomorrow | 09:00 | 10       | Blue    | Supported |

