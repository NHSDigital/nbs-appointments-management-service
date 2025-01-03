Feature: BookingExtracts
As a data analyst
I want data about the last 24 hours of booking updates
So that I can collate information about bookings across the nation

Scenario: Booking data is sent via MESH
	Given I have some bookings
  | Hours Since Creation | Booking Ref |
  | 23                   | AB-01-1234  |
  And the system is configured as follows
  | Target Mailbox | Workflow Id                     |
  | X26ABC2        | MYA_INBOUND_NATIONAL_BOOKINGS_1 |
  And the target mailbox is empty
	When the booking data extract runs
	Then booking data is available in the target mailbox
  | Booking Ref |
  | AB-01-1234  |
