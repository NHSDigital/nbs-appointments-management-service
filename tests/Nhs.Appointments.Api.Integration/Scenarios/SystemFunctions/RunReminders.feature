Feature: RunReminders

    @ignore
    Scenario: Running reminders sends reminders for upcoming appointments	
        Given the site is configured for MYA
        And the following bookings have been made
            | Date              | Time  | Duration | Service |
            | 2 days from today | 09:20 | 5        | COVID   |
        When the reminders job runs
        Then the following notifications are sent out
            | Recipient | Notification         |
            | email     | COVID Email Reminder |
            | phone     | COVID SMS Reminder   |

     Scenario: Running reminders does not send reminders for recently made appointments	
        Given the site is configured for MYA
        And the following recent bookings have been made
            | Date              | Time  | Duration | Service |
            | 2 days from today | 09:20 | 5        | COVID   |
        When the reminders job runs
        Then no notifications are sent out            

     Scenario: Running reminders targets correct document types
        Given the site is configured for MYA
        And the following bookings have been made
            | Date              | Time  | Duration | Service |
            | Tomorrow          | 12:00 | 5        | COVID   |
            | 2 days from today | 17:20 | 5        | COVID   |
        And there are audit entries in the database
        When the reminders job runs
        Then there are no errors
    
    Scenario: Running reminders does not send reminders for past appointments	
        Given the site is configured for MYA
        And the following bookings have been made
            | Date              | Time  | Duration | Service |
            | 2 days before today | 09:20 | 5        | COVID   |
        When the reminders job runs
        Then no notifications are sent out

    Scenario: Running reminders does not send reminders for future appointments outside the reminder window
        Given the site is configured for MYA
        And the following bookings have been made
            | Date              | Time  | Duration | Service |
            | 4 days from today | 09:20 | 5        | COVID   |
        When the reminders job runs
        Then no notifications are sent out
    
    Scenario: Running reminders does not send reminders when a reminder has already been sent
        Given the site is configured for MYA    
        And the following bookings have been made
            | Date              | Time  | Duration | Service |
            | 2 days from today | 09:20 | 5        | COVID   |
        And those appointments have already had notifications sent
        When the reminders job runs
        Then no notifications are sent out
