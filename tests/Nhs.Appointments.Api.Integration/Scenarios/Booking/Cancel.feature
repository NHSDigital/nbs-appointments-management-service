﻿Feature: Appointment cancellation

  Scenario: Cancel a booking appointment
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:20 | 5        | COVID   |
    When I cancel the appointment
    Then the booking has been 'Cancelled'

  Scenario: Cancel a booking appointment which can be replaced by an orphaned appointment
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 68374-29374 |
    And the following orphaned bookings exist
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 85032-19283 |
    When I cancel the appointment with reference '68374-29374'
    Then the booking with reference '68374-29374' has been 'Cancelled'
    Then the booking with reference '85032-19283' has been 'Booked'
