Feature: Appointment cancellation

  Scenario: Cancel a booking appointment
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:20 | 5        | COVID   |
    When I cancel the appointment without a site parameter
    Then the booking has been 'Cancelled'
    And an audit function document was created for user 'api@test' and function 'CancelBookingFunction' and no site

  Scenario: Cancel a booking appointment updates LastUpdatedBy property
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And I register and use a http client with details
      | User Id  | Role                         | Scope  |
      | mya_user | system:integration-test-user | global |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 43567-29374 |
    #Verify default setup
    And the booking document with reference '43567-29374' has lastUpdatedBy 'api@test'
    When I cancel the appointment with reference '43567-29374'
    Then the booking with reference '43567-29374' has been 'Cancelled'
    And the booking document with reference '43567-29374' has lastUpdatedBy 'api@mya_user'

  Scenario: Cancel a booking appointment and provide the site parameter
    Given the following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 6e3348bf-3509-45f2-887c-4f9651501f06 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for existing site '6e3348bf-3509-45f2-887c-4f9651501f06'
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made for site '6e3348bf-3509-45f2-887c-4f9651501f06'
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:20 | 5        | COVID   |
    When I cancel the appointment with site parameter '6e3348bf-3509-45f2-887c-4f9651501f06'
    Then the booking at site '6e3348bf-3509-45f2-887c-4f9651501f06' has been 'Cancelled'
    And an audit function document was created for
      | User     | Function Name         | Site                                 |
      | api@test | CancelBookingFunction | 6e3348bf-3509-45f2-887c-4f9651501f06 |

  Scenario: Cancel a booking appointment and provide the wrong site parameter
    Given the following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 1bb81f6c-0e7d-4032-baea-bc32ea80d176 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for existing site '1bb81f6c-0e7d-4032-baea-bc32ea80d176'
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made for site '1bb81f6c-0e7d-4032-baea-bc32ea80d176'
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:20 | 5        | COVID   |
    When I cancel the appointment with site parameter '51c7d74c-06b6-4375-8008-f2ea312b1a69'
    Then the call should fail with 400

  Scenario: Cancel a booking appointment which can be replaced by an orphaned appointment
    Given the following sessions exist for a created default site
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
    And the booking with reference '68374-29374' has availability status 'Unknown'
    And the booking with reference '85032-19283' has status 'Booked'
    And the booking with reference '85032-19283' has availability status 'Supported'
    And an audit function document was created for user 'api@test' and function 'CancelBookingFunction'

  Scenario: Cancel a booking appointment without cancellation reason
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:20 | 5        | COVID   |
    When I cancel the appointment without a site parameter
    Then the booking has been 'Cancelled'
    And default cancellation reason has been used

  Scenario: Cancel a booking appointment with invalid cancellation reason
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:20 | 5        | COVID   |
    When I cancel the appointment with cancellation reason 'InvalidStatus'
    Then the call should fail with 400

  Scenario: Cancel a booking appointment with valid cancellation reason
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:20 | 5        | COVID   |
    When I cancel the appointment with cancellation reason 'CancelledBySite'
    Then the booking has been 'Cancelled'
    And 'CancelledBySite' cancellation reason has been used

  Scenario: Cancel a booking appointment with AutoCancelled cancellation reason
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:20 | 5        | COVID   |
    When I cancel the appointment with cancellation reason 'CancelledByService'
    Then the booking has been 'Cancelled'
    And 'CancelledByService' cancellation reason has been used

  Scenario: Cancel a booking appointment which can be replaced by an orphaned appointment of a different service
    Given the following sessions exist for a created default site
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
    Given the following sessions exist for a created default site
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
    Given the following sessions exist for a created default site
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
    Given the following sessions exist for a created default site
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
    Given the following sessions exist for a created default site
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
