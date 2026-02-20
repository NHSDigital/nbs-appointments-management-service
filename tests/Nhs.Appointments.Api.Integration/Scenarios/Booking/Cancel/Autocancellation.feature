Feature: Appointment cancellation

  Scenario: Can cancel a booking when I provide no details for cancellation reason or additional data
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    And the following bookings exist at the default site
      | Reference | Booking Type |
      | 1         | Confirmed    |
    When I cancel the following bookings
      | Reference |
      | 1         |
    Then the following bookings at the default site are now in the following state
      | Reference | Status    | Cancellation reason |
      | 1         | Cancelled | CancelledByCitizen  |
      
  Scenario: Can set additional data when cancelling a booking and providing no site parameter
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    And the following bookings exist at the default site
      | Reference | Booking Type |
      | 1         | Confirmed    |
    When I cancel the following bookings
      | Reference | Cancellation reason | AdditionalData 1   |
      | 1         | CancelledByService  | selfReferral,false |
    Then the following bookings at the default site are now in the following state
      | Reference | Status    | Cancellation reason | AdditionalData 1   |
      | 1         | Cancelled | CancelledByService  | selfReferral,false |

  Scenario: Can set additional data when cancelling a booking and providing a site parameter
    Given I set the default siteId to be 'c305e084-ad5a-4bc3-a567-bd0ffbb23e57'
    And the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    And the following bookings exist at the default site
      | Reference | Booking Type |
      | 1         | Confirmed    |
    When I cancel the following bookings
      | Reference | Cancellation reason | AdditionalData 1   | Site                                 |
      | 1         | CancelledByService  | selfReferral,false | c305e084-ad5a-4bc3-a567-bd0ffbb23e57 |
    Then the following bookings at the default site are now in the following state
      | Reference | Status    | Cancellation reason | AdditionalData 1   |
      | 1         | Cancelled | CancelledByService  | selfReferral,false |

    Scenario: Booking not found when cancelling a booking and providing the wrong site parameter
      Given the following sites exist in the system
        | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
        | c305e084-ad5a-4bc3-a567-bd0ffbb23e56 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
      And the following sessions exist for existing site 'c305e084-ad5a-4bc3-a567-bd0ffbb23e56'
        | Date     | From  | Until | Services  | Slot Length | Capacity |
        | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
      And the following bookings exist at site 'c305e084-ad5a-4bc3-a567-bd0ffbb23e56'
        | Reference | Booking Type |
        | 1         | Confirmed    |
      When I cancel the following bookings
        | Reference | Cancellation reason | AdditionalData 1   | Site                                 |
        | 1         | CancelledByService  | selfReferral,false | d632e084-ad5a-4bc3-a567-bd0ffbb23e57 |
      Then a bad request error is returned

  Scenario: Can patch additional data when cancelling a booking
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    And the following bookings exist at the default site
      | Reference | Booking Type | AdditionalData 1  | AdditionalData 2  | AdditionalData 3                          |
      | 1         | Confirmed    | selfReferral,true | isAppBooking,true | callCentreHandlerEmail,fred.jones@nhs.net |
    When I cancel the following bookings
      | Reference | Cancellation reason | AdditionalData 1   |
      | 1         | CancelledByService  | selfReferral,false |
    Then the following bookings at the default site are now in the following state
      | Reference | Status    | Cancellation reason | AdditionalData 1   | AdditionalData 2  | AdditionalData 3                          |
      | 1         | Cancelled | CancelledByService  | selfReferral,false | isAppBooking,true | callCentreHandlerEmail,fred.jones@nhs.net |
