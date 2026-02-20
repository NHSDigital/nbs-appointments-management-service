Feature: Appointment cancellation

  Scenario: Cancel a booking appointment
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made at the default site
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:20 | 5        | COVID   |
    When I cancel the first booking without a site parameter
    Then the first booking at the default site has been 'Cancelled'
    And an audit function document was created for user 'api@test' and function 'CancelBookingFunction' and no site

  Scenario: Cancel a booking appointment updates LastUpdatedBy property
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And I register and use a http client with details
      | User Id  | Role                         | Scope  |
      | mya_user | system:integration-test-user | global |
    And the following bookings have been made at the default site
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 43567-29374 |
    #Verify default setup
    And the booking at the default site with reference '43567-29374' has lastUpdatedBy 'api@test'
    When I cancel the booking at the default site with reference '43567-29374'
    Then the booking at the default site with reference '43567-29374' has been 'Cancelled'
    And the booking at the default site with reference '43567-29374' has lastUpdatedBy 'api@mya_user'

  Scenario: Cancel a booking appointment and provide the site parameter
    Given I set a single siteId for the test to be '6e3348bf-3509-45f2-887c-4f9651501f05'
    And the following sessions exist for a created default site
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made at the default site
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:20 | 5        | COVID   |
    # Wait for setup aggregation to be processed
    And an aggregation is created for the default site for 'Tomorrow' with '0' cancelled bookings, maximumCapacity '12', and with service details
      | Service  | Bookings    | Orphaned  | RemainingCapacity |
      | COVID    | 1           | 0         | 11                |
    When I cancel the first confirmed booking at the default site
    Then the first booking at the default site has been 'Cancelled'
    And an aggregation for the default site updated recently for 'Tomorrow' with '1' cancelled bookings, maximumCapacity '12', and with service details
      | Service  | Bookings    | Orphaned  | RemainingCapacity |
      | COVID    | 0           | 0         | 12                |
    And an audit function document for the default site was created for user 'api@test' and function 'CancelBookingFunction'
    
  Scenario: Cancel a booking appointment and provide the wrong site parameter
    Given the following sessions exist for a created site '26c7d74c-06b6-4375-8008-f2ea312b1a69'
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made at site '26c7d74c-06b6-4375-8008-f2ea312b1a69'
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:20 | 5        | COVID   |
    When I cancel the first confirmed booking at site '51c7d74c-06b6-4375-8008-f2ea312b1a69'
    Then the call should fail with 400

  Scenario: Cancel a booking appointment which can be replaced by an orphaned appointment
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made at the default site
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 68374-29374 |
    And the following orphaned bookings exist at the default site
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 85032-19283 |
    When I cancel the booking at the default site with reference '68374-29374'
    Then the booking at the default site with reference '68374-29374' has been 'Cancelled'
    And the booking at the default site with reference '68374-29374' has availability status 'Unknown'
    And the booking at the default site with reference '85032-19283' has status 'Booked'
    And the booking at the default site with reference '85032-19283' has availability status 'Supported'
    And an audit function document for the default site was created for user 'api@test' and function 'CancelBookingFunction'

  Scenario: Cancel a booking appointment without cancellation reason
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made at the default site
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:20 | 5        | COVID   |
    When I cancel the first booking without a site parameter
    Then the first booking at the default site has been 'Cancelled'
    And the default cancellation reason has been used for the first booking at the default site

  Scenario: Cancel a booking appointment with invalid cancellation reason
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made at the default site
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:20 | 5        | COVID   |
    When I cancel the first confirmed booking at the default site with cancellation reason 'InvalidStatus'
    Then the call should fail with 400

  Scenario: Cancel a booking appointment with valid cancellation reason
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made at the default site
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:20 | 5        | COVID   |
    When I cancel the first confirmed booking at the default site with cancellation reason 'CancelledBySite'
    Then the first booking at the default site has been 'Cancelled'
    And 'CancelledBySite' cancellation reason has been used for the first booking at the default site

  Scenario: Cancel a booking appointment with AutoCancelled cancellation reason
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made at the default site
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:20 | 5        | COVID   |
    When I cancel the first confirmed booking at the default site with cancellation reason 'CancelledByService'
    Then the first booking at the default site has been 'Cancelled'
    And 'CancelledByService' cancellation reason has been used for the first booking at the default site

  Scenario: Cancel a booking appointment which can be replaced by an orphaned appointment of a different service
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID, FLU | 5           | 1        |
    And the following bookings have been made at the default site
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 68374-29374 |
    And the following orphaned bookings exist at the default site
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | FLU     | 85032-19283 |
    When I cancel the booking at the default site with reference '68374-29374'
    Then the booking at the default site with reference '68374-29374' has been 'Cancelled'
    And the booking at the default site with reference '68374-29374' has availability status 'Unknown'
    And the booking at the default site with reference '85032-19283' has status 'Booked'
    And the booking at the default site with reference '85032-19283' has availability status 'Supported'
    And an audit function document for the default site was created for user 'api@test' and function 'CancelBookingFunction'

  Scenario: Greedy allocation alphabetical - suboptimal - Cancel a booking appointment can be replaced by an orphaned appointment
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID, RSV | 5           | 1        |
      | Tomorrow | 09:00 | 10:00 | COVID, FLU | 5           | 1        |
    And the following bookings have been made at the default site
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 56345-09354 |
      | Tomorrow | 09:20 | 5        | COVID   | 68374-29374 |
    And the following orphaned bookings exist at the default site
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | FLU     | 84023-77342 |
      | Tomorrow | 09:20 | 5        | RSV     | 75392-09012 |
    When I cancel the booking at the default site with reference '56345-09354'
    Then the booking at the default site with reference '56345-09354' has been 'Cancelled'
    And the booking at the default site with reference '56345-09354' has availability status 'Unknown'
    And the booking at the default site with reference '68374-29374' has status 'Booked'
    And the booking at the default site with reference '68374-29374' has availability status 'Supported'
    And the booking at the default site with reference '84023-77342' has status 'Booked'
    And the booking at the default site with reference '84023-77342' has availability status 'Orphaned'
    And the booking at the default site with reference '75392-09012' has status 'Booked'
    And the booking at the default site with reference '75392-09012' has availability status 'Supported'
    And an audit function document for the default site was created for user 'api@test' and function 'CancelBookingFunction'

  Scenario: Greedy allocation alphabetical - optimal - Cancel a booking appointment can be replaced by an orphaned appointment
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID, RSV | 5           | 1        |
      | Tomorrow | 09:00 | 10:00 | COVID, FLU | 5           | 1        |
    And the following bookings have been made at the default site
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 56345-09354 |
      | Tomorrow | 09:20 | 5        | COVID   | 68374-29374 |
    And the following orphaned bookings exist at the default site
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | RSV     | 75392-09012 |
      | Tomorrow | 09:20 | 5        | FLU     | 84023-77342 |
    When I cancel the booking at the default site with reference '56345-09354'
    Then the booking at the default site with reference '56345-09354' has been 'Cancelled'
    And the booking at the default site with reference '56345-09354' has availability status 'Unknown'
    And the booking at the default site with reference '68374-29374' has status 'Booked'
    And the booking at the default site with reference '68374-29374' has availability status 'Supported'
    And the booking at the default site with reference '75392-09012' has status 'Booked'
    And the booking at the default site with reference '75392-09012' has availability status 'Supported'
    And the booking at the default site with reference '84023-77342' has status 'Booked'
    And the booking at the default site with reference '84023-77342' has availability status 'Orphaned'
    And an audit function document for the default site was created for user 'api@test' and function 'CancelBookingFunction'

  Scenario: Greedy allocation by service length - suboptimal - Cancel a booking appointment can be replaced by an orphaned appointment
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services                        | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID, RSV, COVID-16, COVID-75  | 5           | 1        |
      | Tomorrow | 09:00 | 10:00 | COVID, FLU                      | 5           | 1        |
    And the following bookings have been made at the default site
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 56345-09354 |
      | Tomorrow | 09:20 | 5        | COVID   | 68374-29374 |
    And the following orphaned bookings exist at the default site
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | FLU     | 84023-77342 |
      | Tomorrow | 09:20 | 5        | RSV     | 75392-09012 |
    When I cancel the booking at the default site with reference '56345-09354'
    Then the booking at the default site with reference '56345-09354' has been 'Cancelled'
    And the booking at the default site with reference '56345-09354' has availability status 'Unknown'
    And the booking at the default site with reference '68374-29374' has status 'Booked'
    And the booking at the default site with reference '68374-29374' has availability status 'Supported'
    And the booking at the default site with reference '84023-77342' has status 'Booked'
    And the booking at the default site with reference '84023-77342' has availability status 'Orphaned'
    And the booking at the default site with reference '75392-09012' has status 'Booked'
    And the booking at the default site with reference '75392-09012' has availability status 'Supported'
    And an audit function document for the default site was created for user 'api@test' and function 'CancelBookingFunction'

  Scenario: Greedy allocation by service length - optimal - Cancel a booking appointment can be replaced by an orphaned appointment
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services                        | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID, RSV, COVID-16, COVID-75  | 5           | 1        |
      | Tomorrow | 09:00 | 10:00 | COVID, FLU                      | 5           | 1        |
    And the following bookings have been made at the default site
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 56345-09354 |
      | Tomorrow | 09:20 | 5        | COVID   | 68374-29374 |
    And the following orphaned bookings exist at the default site
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | RSV     | 75392-09012 |
      | Tomorrow | 09:20 | 5        | FLU     | 84023-77342 |
    When I cancel the booking at the default site with reference '56345-09354'
    Then the booking at the default site with reference '56345-09354' has been 'Cancelled'
    And the booking at the default site with reference '56345-09354' has availability status 'Unknown'
    And the booking at the default site with reference '68374-29374' has status 'Booked'
    And the booking at the default site with reference '68374-29374' has availability status 'Supported'
    And the booking at the default site with reference '75392-09012' has status 'Booked'
    And the booking at the default site with reference '75392-09012' has availability status 'Supported'
    And the booking at the default site with reference '84023-77342' has status 'Booked'
    And the booking at the default site with reference '84023-77342' has availability status 'Orphaned'
    And an audit function document for the default site was created for user 'api@test' and function 'CancelBookingFunction'
