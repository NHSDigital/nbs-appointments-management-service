Feature: Change Session Uplifted Journey

# Wildcard cancellation not yet implemented
  @ignore
  Scenario: Cancels All Sessions In A Single Day
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:45 | 5        | COVID   | 68537-44913 |
    When I cancel all sessions in between 'Tomorrow' and 'Tomorrow'
    Then the booking with reference '68537-44913' has been 'Cancelled'
    And there are no sessions for 'Tomorrow'

# Wildcard cancellation not yet implemented
  @ignore
  Scenario: Cancels All Sessions Over Multiple Days
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | COVID    | 5           | 1        |
      | 2 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date              | Time  | Duration | Service | Reference   |
      | Tomorrow          | 09:45 | 5        | COVID   | 68537-44913 |
      | 2 days from today | 09:20 | 5        | COVID   | 12345-09876 |
    When I cancel all sessions in between 'Tomorrow' and '2 days from today'
    Then the booking with reference '68537-44913' has been 'Cancelled'
    And the booking with reference '12345-09876' has been 'Cancelled'
    And there are no sessions for 'Tomorrow'
    And there are no sessions for '2 days from today'

  Scenario: Edit a single session on a single day orphans newly orphaned booking
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:45 | 5        | COVID   | 68537-44913 |
    When I replace the session with the following and set newlyUnsupportedBookingAction to 'Orphan'
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 12:00 | 17:00 | COVID    | 5           | 1        |
    Then the session 'Tomorrow' should have been updated
    Then the booking with reference '68537-44913' has status 'Booked'
    Then the booking with reference '68537-44913' has availability status 'Orphaned'

  Scenario: Edit a single session on a single day cancels newly orphaned booking
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:45 | 5        | COVID   | 68537-44913 |
    When  I replace the session with the following and set newlyUnsupportedBookingAction to 'Cancel'
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 12:00 | 17:00 | COVID    | 5           | 1        |
    Then the session 'Tomorrow' should have been updated
    Then the booking with reference '68537-44913' has status 'Cancelled'

  Scenario: Cancel a single session on a single day orphans newly orphaned booking
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
      | Tomorrow | 09:00 | 16:00 | RSV      | 10          | 2        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:45 | 5        | COVID   | 68537-44913 |
    When I cancel the following session using the edit endpoint and set newlyUnsupportedBookingAction to 'Orphan'
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    Then the session 'Tomorrow' no longer exists
    Then the booking with reference '68537-44913' has status 'Booked'
    Then the booking with reference '68537-44913' has availability status 'Orphaned'

  Scenario: Cancel a single session on a single day cancels newly orphaned booking
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
      | Tomorrow | 09:00 | 16:00 | RSV      | 10          | 2        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:45 | 5        | COVID   | 68537-44913 |
    When I cancel the following session using the edit endpoint and set newlyUnsupportedBookingAction to 'Cancel'
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    Then the session 'Tomorrow' no longer exists
    Then the booking with reference '68537-44913' has status 'Cancelled'

  Scenario: Cancel multiple sessions over multiple days orphans newly orphaned bookings
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | COVID    | 5           | 1        |
      | 2 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
      | 3 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date              | Time  | Duration | Service | Reference   |
      | Tomorrow          | 09:45 | 5        | COVID   | 68537-44913 |
      | 2 days from today | 09:45 | 5        | COVID   | 12345-12345 |
      | 3 days from today | 09:45 | 5        | COVID   | 54321-54321 |
    When I cancel the sessions matching this between 'Tomorrow' and '3 days from now' and set newlyUnsupportedBookingAction to 'Orphan'
      | From  | Until | Services | Slot Length | Capacity |
      | 09:00 | 10:00 | COVID    | 5           | 1        |
    Then the sessions between 'Tomorrow' and '3 days from now' no longer exist
    And the booking with reference '68537-44913' has status 'Booked'
    And the booking with reference '68537-44913' has availability status 'Orphaned'
    Then the booking with reference '12345-12345' has status 'Booked'
    And the booking with reference '12345-12345' has availability status 'Orphaned'
    Then the booking with reference '54321-54321' has status 'Booked'
    And the booking with reference '54321-54321' has availability status 'Orphaned'

  Scenario: Cancel multiple sessions over multiple days cancels newly orphaned bookings
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | COVID    | 5           | 1        |
      | 2 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
      | 3 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date              | Time  | Duration | Service | Reference   |
      | Tomorrow          | 09:45 | 5        | COVID   | 68537-44913 |
      | 2 days from today | 09:45 | 5        | COVID   | 12345-12345 |
      | 3 days from today | 09:45 | 5        | COVID   | 54321-54321 |
    When I cancel the sessions matching this between 'Tomorrow' and '3 days from now' and set newlyUnsupportedBookingAction to 'Cancel'
      | From  | Until | Services | Slot Length | Capacity |
      | 09:00 | 10:00 | COVID    | 5           | 1        |
    Then the sessions between 'Tomorrow' and '3 days from now' no longer exist
    And the booking with reference '68537-44913' has status 'Cancelled'
    Then the booking with reference '12345-12345' has status 'Cancelled'
    Then the booking with reference '54321-54321' has status 'Cancelled'

  Scenario: Edit multiple sessions over multiple days orphans newly orphaned bookings
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | COVID    | 5           | 1        |
      | 2 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
      | 3 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date              | Time  | Duration | Service | Reference   |
      | Tomorrow          | 09:45 | 5        | COVID   | 68537-44913 |
      | 2 days from today | 09:45 | 5        | COVID   | 12345-12345 |
      | 3 days from today | 09:45 | 5        | COVID   | 54321-54321 |
    When I replace multiple sessions between 'Tomorrow' and '3 days from now' with this session and set newlyUnsupportedBookingAction to 'Orphan'
      | From  | Until | Services | Slot Length | Capacity |
      | 11:00 | 16:00 | COVID    | 5           | 1        |
    Then the sessions between 'Tomorrow' and '3 days from now' should have been updated
    And the booking with reference '68537-44913' has status 'Booked'
    And the booking with reference '68537-44913' has availability status 'Orphaned'
    Then the booking with reference '12345-12345' has status 'Booked'
    And the booking with reference '12345-12345' has availability status 'Orphaned'
    Then the booking with reference '54321-54321' has status 'Booked'
    And the booking with reference '54321-54321' has availability status 'Orphaned'

  Scenario: Edit multiple sessions over multiple days cancels newly orphaned bookings
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | COVID    | 5           | 1        |
      | 2 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
      | 3 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date              | Time  | Duration | Service | Reference   |
      | Tomorrow          | 09:45 | 5        | COVID   | 68537-44913 |
      | 2 days from today | 09:45 | 5        | COVID   | 12345-12345 |
      | 3 days from today | 09:45 | 5        | COVID   | 54321-54321 |
    When I replace multiple sessions between 'Tomorrow' and '3 days from now' with this session and set newlyUnsupportedBookingAction to 'Cancel'
      | From  | Until | Services | Slot Length | Capacity |
      | 11:00 | 16:00 | COVID    | 5           | 1        |
    Then the sessions between 'Tomorrow' and '3 days from now' should have been updated
    And the booking with reference '68537-44913' has status 'Cancelled'
    Then the booking with reference '12345-12345' has status 'Cancelled'
    Then the booking with reference '54321-54321' has status 'Cancelled'

#  Use the greedy model allocation inefficiency to prove users may cancel bookings unrelated to their session edit action 
#  The propose metric does not inform them of the potential danger...
#  This would be resolved with a best fit solution...
  Scenario: Greedy allocation shows that unintended bookings can get cancelled during the two-step propose and edit journey
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length  | Capacity |
      | Tomorrow          | 09:00 | 09:10 | A,B,C,E    | 10           | 5        |
      | Tomorrow          | 09:00 | 09:10 | A,B,D,F    | 10           | 5        |
      | Tomorrow          | 09:00 | 09:10 | D,F        | 10           | 4        |
    And the following bookings have been made
      | Date              | Time  | Duration | Service | Reference    |
      | Tomorrow          | 09:00 | 10       | A       | 324524-00001 |
      | Tomorrow          | 09:00 | 10       | A       | 324524-00002 |
      | Tomorrow          | 09:00 | 10       | A       | 324524-00003 |
      | Tomorrow          | 09:00 | 10       | A       | 324524-00004 |
      | Tomorrow          | 09:00 | 10       | A       | 324524-00005 |
      | Tomorrow          | 09:00 | 10       | D       | 324524-00006 |
      | Tomorrow          | 09:00 | 10       | D       | 324524-00007 |
      | Tomorrow          | 09:00 | 10       | D       | 324524-00008 |
      | Tomorrow          | 09:00 | 10       | F       | 324524-00009 |
      | Tomorrow          | 09:00 | 10       | F       | 324524-00010 |
#   Step 1 Action: User 1 proposes an edit to one of the sessions (remove service 'F' from the services list)
    When I request the availability proposal for potential availability change
      | Type        | RequestFrom | RequestTo | From  | Until | Services   | SlotLength | Capacity |
      | Matcher     | Tomorrow    | Tomorrow  | 09:00 | 09:10 | A,B,D,F    | 10         | 5        |
      | Replacement |             |           | 09:00 | 09:10 | A,B,D      | 10         | 5        |
#   Step 1 Result: User 1 is told that only 1 orphaned booking will be cancelled should they choose to continue with cancellation
    Then the following count is returned
      | newlySupportedBookingsCount   | 0 |
      | newlyUnsupportedBookingsCount    | 1 |
  # Step 2 Action: Meanwhile... User 2 proposes to delete one of the sessions (remove session 'D,F')
    When I request the availability proposal for potential availability change
      | Type        | RequestFrom | RequestTo | From  | Until | Services   | SlotLength | Capacity |
      | Matcher     | Tomorrow    | Tomorrow  | 09:00 | 09:10 | D,F        | 10         | 4        |
#   Step 2 Result: User 2 is told that ZERO orphaned bookings will be cancelled should they choose to continue
    Then the following count is returned
      | newlySupportedBookingsCount   | 0 |
      | newlyUnsupportedBookingsCount    | 0 |
#   Step 3 Action: User 2 confirms the session cancellation as nothing is affected
    When I cancel the following session using the edit endpoint and set newlyUnsupportedBookingAction to 'Cancel'
      | Date     | From  | Until | Services | Slot Length  | Capacity |
      | Tomorrow | 09:00 | 09:10 | D,F      | 10           | 4        |
#   Step 3 Result: Confirm all TEN bookings are still supported after User 2's session deletion action
#   This confirms the proposal metric offered to User 2 was accurate! that no bookings are orphaned or cancelled
    Then the booking with reference '324524-00001' has status 'Booked'
    And the booking with reference '324524-00001' has availability status 'Supported'
    Then the booking with reference '324524-00002' has status 'Booked'
    And the booking with reference '324524-00002' has availability status 'Supported'
    Then the booking with reference '324524-00003' has status 'Booked'
    And the booking with reference '324524-00003' has availability status 'Supported'
    Then the booking with reference '324524-00004' has status 'Booked'
    And the booking with reference '324524-00004' has availability status 'Supported'
    Then the booking with reference '324524-00005' has status 'Booked'
    And the booking with reference '324524-00005' has availability status 'Supported'
    Then the booking with reference '324524-00006' has status 'Booked'
    And the booking with reference '324524-00006' has availability status 'Supported'
    Then the booking with reference '324524-00007' has status 'Booked'
    And the booking with reference '324524-00007' has availability status 'Supported'
    Then the booking with reference '324524-00008' has status 'Booked'
    And the booking with reference '324524-00008' has availability status 'Supported'
    Then the booking with reference '324524-00009' has status 'Booked'
    And the booking with reference '324524-00009' has availability status 'Supported'
    Then the booking with reference '324524-00010' has status 'Booked'
    And the booking with reference '324524-00010' has availability status 'Supported'
#   Step 4 Action: User 1 (unaware that another user action has occurred), continues with attempting to cancel the ONE booking that was proposed in 'Step 1'...
    When I replace a session with a replacement and set newlyUnsupportedBookingAction to 'Cancel'
      | Type        | RequestFrom | RequestTo | From  | Until | Services   | SlotLength | Capacity |
      | Matcher     | Tomorrow    | Tomorrow  | 09:00 | 09:10 | A,B,D,F    | 10         | 5        |
      | Replacement |             |           | 09:00 | 09:10 | A,B,D      | 10         | 5        |
#   Step 4 Result: Confirm FIVE bookings have been CANCELLED when the user thought they were only cancelling ONE
#   In a BEST FIT scenario, you would expect TWO bookings (both 'F') to be cancelled, the 3 'D' bookings SHOULD HAVE FOUND A HOME (but Greedy inefficiency has left them orphaned)
#   They have now been cancelled due to Greedy allocation inefficiency!!
    Then the booking with reference '324524-00001' has status 'Booked'
    And the booking with reference '324524-00001' has availability status 'Supported'
    Then the booking with reference '324524-00002' has status 'Booked'
    And the booking with reference '324524-00002' has availability status 'Supported'
    Then the booking with reference '324524-00003' has status 'Booked'
    And the booking with reference '324524-00003' has availability status 'Supported'
    Then the booking with reference '324524-00004' has status 'Booked'
    And the booking with reference '324524-00004' has availability status 'Supported'
    Then the booking with reference '324524-00005' has status 'Booked'
    And the booking with reference '324524-00005' has availability status 'Supported'
    Then the booking with reference '324524-00006' has status 'Cancelled'
    Then the booking with reference '324524-00007' has status 'Cancelled'
    Then the booking with reference '324524-00008' has status 'Cancelled'
    Then the booking with reference '324524-00009' has status 'Cancelled'
    Then the booking with reference '324524-00010' has status 'Cancelled'


  Scenario: Multiple consecutive patches on a single document should return a consistent outcome
    Given the following sessions exist for a created default site
      | Date                | From  | Until | Services   | Slot Length  | Capacity |
      | 10 days from today  | 09:00 | 09:10 | A,B,C,D    | 10           | 5        |
      | 10 days from today  | 09:00 | 09:10 | A,B,C,E    | 10           | 5        |
      | 10 days from today  | 09:00 | 09:10 | A,B,C,F    | 10           | 5        |
      | 10 days from today  | 09:00 | 09:10 | A,B        | 10           | 5        |
      | 10 days from today  | 09:00 | 09:10 | A,B,C      | 10           | 5        |
      | 10 days from today  | 09:00 | 09:10 | A          | 10           | 5        |
      | 10 days from today  | 09:00 | 09:10 | B          | 10           | 5        |
      | 10 days from today  | 09:00 | 09:10 | C          | 10           | 5        |
      | 10 days from today  | 09:00 | 09:10 | D          | 10           | 5        |
      | 10 days from today  | 09:00 | 09:10 | E          | 10           | 5        |
    And the following bookings have been made
      | Date                | Time  | Duration | Service | Reference    |
      | 10 days from today  | 09:00 | 10       | A       | 324524-00001 |
      | 10 days from today  | 09:00 | 10       | B       | 324524-00002 |
      | 10 days from today  | 09:00 | 10       | C       | 324524-00003 |
      | 10 days from today  | 09:00 | 10       | D       | 324524-00004 |
      | 10 days from today  | 09:00 | 10       | E       | 324524-00005 |
      | 10 days from today  | 09:00 | 10       | F       | 324524-00006 |
    When I replace a session with a replacement and set newlyUnsupportedBookingAction to 'Cancel'
      | Type        | RequestFrom           | RequestTo           | From  | Until | Services   | SlotLength | Capacity |
      | Matcher     | 10 days from today    | 10 days from today  | 09:00 | 09:10 | A,B,C,D    | 10         | 5        |
      | Replacement |                       |                     | 09:00 | 09:10 | B,C,D      | 10         | 5        |
    And I replace a session with a replacement and set newlyUnsupportedBookingAction to 'Cancel'
      | Type        | RequestFrom           | RequestTo           | From  | Until | Services   | SlotLength | Capacity |
      | Matcher     | 10 days from today    | 10 days from today  | 09:00 | 09:10 | A,B,C,E    | 10         | 5        |
      | Replacement |                       |                     | 09:00 | 09:10 | B,C,E      | 10         | 5        |
    And I replace a session with a replacement and set newlyUnsupportedBookingAction to 'Cancel'
      | Type        | RequestFrom           | RequestTo           | From  | Until | Services   | SlotLength | Capacity |
      | Matcher     | 10 days from today    | 10 days from today  | 09:00 | 09:10 | A,B,C,F    | 10         | 5        |
      | Replacement |                       |                     | 09:00 | 09:10 | B,C,F      | 10         | 5        |
    And I cancel the following session using the edit endpoint and set newlyUnsupportedBookingAction to 'Cancel'
      | Date               | From  | Until | Services | Slot Length  | Capacity |
      | 10 days from today | 09:00 | 09:10 | C        | 10           | 5        |
    And I cancel the following session using the edit endpoint and set newlyUnsupportedBookingAction to 'Cancel'
      | Date               | From  | Until | Services | Slot Length  | Capacity |
      | 10 days from today | 09:00 | 09:10 | D        | 10           | 5        |
    And I replace a session with a replacement and set newlyUnsupportedBookingAction to 'Cancel'
      | Type        | RequestFrom           | RequestTo           | From  | Until | Services   | SlotLength | Capacity |
      | Matcher     | 10 days from today    | 10 days from today  | 09:00 | 09:10 | A,B,C      | 10         | 5        |
      | Replacement |                       |                     | 09:00 | 09:10 | A,B,C,D,E  | 10         | 5        |
    Then the following updated sessions exist for a created default site
      | Date                | From  | Until | Services   | Slot Length  | Capacity |
      | 10 days from today  | 09:00 | 09:10 | B,C,D      | 10           | 5        |
      | 10 days from today  | 09:00 | 09:10 | B,C,E      | 10           | 5        |
      | 10 days from today  | 09:00 | 09:10 | B,C,F      | 10           | 5        |
      | 10 days from today  | 09:00 | 09:10 | A,B        | 10           | 5        |
      | 10 days from today  | 09:00 | 09:10 | A,B,C,D,E  | 10           | 5        |
      | 10 days from today  | 09:00 | 09:10 | A          | 10           | 5        |
      | 10 days from today  | 09:00 | 09:10 | B          | 10           | 5        |
      | 10 days from today  | 09:00 | 09:10 | E          | 10           | 5        |
