Feature: The Best Fit Problem

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
    When I cancel the following sessions at the default site 
      | Date     | From  | Until | Blue        | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | Blue,Orange | 10          | 1        |
    Then the call should fail with 501
