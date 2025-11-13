Feature: Appointment cancellation

  Scenario: Can set additional data when cancelling a booking and providing no site parameter
    Given the following sessions
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    And the following bookings exist
      | Reference | Booking Type |
      | 1         | Confirmed    |
    When I cancel the following bookings
      | Reference | Cancellation reason | AdditionalData 1   |
      | 1         | CancelledByService  | selfReferral,false |
    Then the following bookings are now in the following state
      | Reference | Status    | Cancellation reason | AdditionalData 1   |
      | 1         | Cancelled | CancelledByService  | selfReferral,false |

  Scenario: Can set additional data when cancelling a booking and providing a site parameter
    Given the following sessions exist for site 'c305e084-ad5a-4bc3-a567-bd0ffbb23e57'
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    And the following bookings exist
      | Reference | Booking Type | Site                                 |
      | 1         | Confirmed    | c305e084-ad5a-4bc3-a567-bd0ffbb23e57 |
    When I cancel the following bookings
      | Reference | Cancellation reason | AdditionalData 1   | Site                                 |
      | 1         | CancelledByService  | selfReferral,false | c305e084-ad5a-4bc3-a567-bd0ffbb23e57 |
    Then the following bookings are now in the following state
      | Reference | Status    | Cancellation reason | AdditionalData 1   | Site                                 |
      | 1         | Cancelled | CancelledByService  | selfReferral,false | c305e084-ad5a-4bc3-a567-bd0ffbb23e57 |

  Scenario: Can patch additional data when cancelling a booking
    Given the following sessions
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    And the following bookings exist
      | Reference | Booking Type | AdditionalData 1  | AdditionalData 2  | AdditionalData 3                          |
      | 1         | Confirmed    | selfReferral,true | isAppBooking,true | callCentreHandlerEmail,fred.jones@nhs.net |
    When I cancel the following bookings
      | Reference | Cancellation reason | AdditionalData 1   |
      | 1         | CancelledByService  | selfReferral,false |
    Then the following bookings are now in the following state
      | Reference | Status    | Cancellation reason | AdditionalData 1   | AdditionalData 2  | AdditionalData 3                          |
      | 1         | Cancelled | CancelledByService  | selfReferral,false | isAppBooking,true | callCentreHandlerEmail,fred.jones@nhs.net |
