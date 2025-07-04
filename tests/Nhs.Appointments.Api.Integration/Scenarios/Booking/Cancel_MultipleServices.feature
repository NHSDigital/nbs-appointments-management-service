﻿Feature: Appointment cancellation for multiple services

  Scenario: Cancel a booking appointment which can be replaced by an orphaned appointment of a different service
    Given the following sessions
      | Date     | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID, FLU | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 68374-29374 |
    And the following orphaned bookings exist
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | FLU     | 85032-19283 |
    When I cancel the appointment with reference '68374-29374'
    Then the booking with reference '68374-29374' has been 'Cancelled'
    And the booking with reference '68374-29374' has availability status 'Unknown'
    And the booking with reference '85032-19283' has status 'Booked'
    And the booking with reference '85032-19283' has availability status 'Supported'
    And an audit function document was created for user 'api@test' and function 'CancelBookingFunction'

  Scenario: Greedy allocation alphabetical - suboptimal - Cancel a booking appointment can be replaced by an orphaned appointment
    Given the following sessions
      | Date     | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID, RSV | 5           | 1        |
      | Tomorrow | 09:00 | 10:00 | COVID, FLU | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 56345-09354 |
      | Tomorrow | 09:20 | 5        | COVID   | 68374-29374 |
    And the following orphaned bookings exist
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | FLU     | 84023-77342 |
      | Tomorrow | 09:20 | 5        | RSV     | 75392-09012 |
    When I cancel the appointment with reference '56345-09354'
    Then the booking with reference '56345-09354' has been 'Cancelled'
    And the booking with reference '56345-09354' has availability status 'Unknown'
    And the booking with reference '68374-29374' has status 'Booked'
    And the booking with reference '68374-29374' has availability status 'Supported'
    And the booking with reference '84023-77342' has status 'Booked'
    And the booking with reference '84023-77342' has availability status 'Orphaned'
    And the booking with reference '75392-09012' has status 'Booked'
    And the booking with reference '75392-09012' has availability status 'Supported'
    And an audit function document was created for user 'api@test' and function 'CancelBookingFunction'

  Scenario: Greedy allocation alphabetical - optimal - Cancel a booking appointment can be replaced by an orphaned appointment
    Given the following sessions
      | Date     | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID, RSV | 5           | 1        |
      | Tomorrow | 09:00 | 10:00 | COVID, FLU | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 56345-09354 |
      | Tomorrow | 09:20 | 5        | COVID   | 68374-29374 |
    And the following orphaned bookings exist
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | RSV     | 75392-09012 |
      | Tomorrow | 09:20 | 5        | FLU     | 84023-77342 |
    When I cancel the appointment with reference '56345-09354'
    Then the booking with reference '56345-09354' has been 'Cancelled'
    And the booking with reference '56345-09354' has availability status 'Unknown'
    And the booking with reference '68374-29374' has status 'Booked'
    And the booking with reference '68374-29374' has availability status 'Supported'
    And the booking with reference '75392-09012' has status 'Booked'
    And the booking with reference '75392-09012' has availability status 'Supported'
    And the booking with reference '84023-77342' has status 'Booked'
    And the booking with reference '84023-77342' has availability status 'Orphaned'
    And an audit function document was created for user 'api@test' and function 'CancelBookingFunction'

  Scenario: Greedy allocation by service length - suboptimal - Cancel a booking appointment can be replaced by an orphaned appointment
    Given the following sessions
      | Date     | From  | Until | Services                        | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID, RSV, COVID-16, COVID-75  | 5           | 1        |
      | Tomorrow | 09:00 | 10:00 | COVID, FLU                      | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 56345-09354 |
      | Tomorrow | 09:20 | 5        | COVID   | 68374-29374 |
    And the following orphaned bookings exist
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | FLU     | 84023-77342 |
      | Tomorrow | 09:20 | 5        | RSV     | 75392-09012 |
    When I cancel the appointment with reference '56345-09354'
    Then the booking with reference '56345-09354' has been 'Cancelled'
    And the booking with reference '56345-09354' has availability status 'Unknown'
    And the booking with reference '68374-29374' has status 'Booked'
    And the booking with reference '68374-29374' has availability status 'Supported'
    And the booking with reference '84023-77342' has status 'Booked'
    And the booking with reference '84023-77342' has availability status 'Orphaned'
    And the booking with reference '75392-09012' has status 'Booked'
    And the booking with reference '75392-09012' has availability status 'Supported'
    And an audit function document was created for user 'api@test' and function 'CancelBookingFunction'
    
  Scenario: Greedy allocation by service length - optimal - Cancel a booking appointment can be replaced by an orphaned appointment
    Given the following sessions
      | Date     | From  | Until | Services                        | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID, RSV, COVID-16, COVID-75  | 5           | 1        |
      | Tomorrow | 09:00 | 10:00 | COVID, FLU                      | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 56345-09354 |
      | Tomorrow | 09:20 | 5        | COVID   | 68374-29374 |
    And the following orphaned bookings exist
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | RSV     | 75392-09012 |
      | Tomorrow | 09:20 | 5        | FLU     | 84023-77342 |
    When I cancel the appointment with reference '56345-09354'
    Then the booking with reference '56345-09354' has been 'Cancelled'
    And the booking with reference '56345-09354' has availability status 'Unknown'
    And the booking with reference '68374-29374' has status 'Booked'
    And the booking with reference '68374-29374' has availability status 'Supported'
    And the booking with reference '75392-09012' has status 'Booked'
    And the booking with reference '75392-09012' has availability status 'Supported'
    And the booking with reference '84023-77342' has status 'Booked'
    And the booking with reference '84023-77342' has availability status 'Orphaned'
    And an audit function document was created for user 'api@test' and function 'CancelBookingFunction'
