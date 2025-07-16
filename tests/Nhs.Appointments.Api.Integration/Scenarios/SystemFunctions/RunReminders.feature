Feature: RunReminders

    Scenario: Running reminders sends reminders for upcoming appointments - Covid 19
        Given the site is configured for MYA
        And I have Clinical Services
          | Service     | ServiceType | Url                          |
          | RSV:Adult   | RSV         | https://www.nhs.uk/book-rsv  |
          | COVID:12_17 | COVID-19    | https://www.nhs.uk/bookcovid |
          | FLU:2_3     | flu         | https://www.nhs.uk/bookflu   |
        And the following bookings have been made
            | Date              | Time  | Duration | Service     | Reference   |
            | 2 days from today | 09:20 | 5        | COVID:12_17 | 56345-09354 |
        When the reminders job runs
        Then the following notifications are sent out
            | Contact Type | TemplateId                             | Vaccine  | Url                          | Reference   | 
            | email        | 36b50be4-fad7-440e-8958-b0dc66efe33a   | COVID-19 | https://www.nhs.uk/bookcovid | 56345-09354 |
            | phone        | c452be27-5bae-422d-9691-6bb955dbc51f   | COVID-19 | https://www.nhs.uk/bookcovid | 56345-09354 |

    Scenario: Running reminders sends reminders for upcoming appointments - Flu
        Given the site is configured for MYA
        And I have Clinical Services
          | Service     | ServiceType | Url                          |
          | RSV:Adult   | RSV         | https://www.nhs.uk/book-rsv  |
          | COVID:12_17 | COVID-19    | https://www.nhs.uk/bookcovid |
          | FLU:2_3     | flu         | https://www.nhs.uk/bookflu   |
        And the following bookings have been made
          | Date               | Time  | Duration | Service | Reference   |
          | 1 days from today  | 17:30 | 5        | FLU:2_3 | 98067-23487 |
        When the reminders job runs
        Then the following notifications are sent out
          | Contact Type | TemplateId                             | Vaccine | Url                          | Reference   |
          | email        | 36b50be4-fad7-440e-8958-b0dc66efe33a   | flu     | https://www.nhs.uk/bookflu   | 98067-23487 | 
          | phone        | c452be27-5bae-422d-9691-6bb955dbc51f   | flu     | https://www.nhs.uk/bookflu   | 98067-23487 |

    Scenario: Running reminders does not send a reminder if the service is not supported
        Given the site is configured for MYA
        And I have Clinical Services
          | Service     | ServiceType | Url                          |
          | RSV:Adult   | RSV         | https://www.nhs.uk/book-rsv  |
          | COVID:12_17 | COVID-19    | https://www.nhs.uk/bookcovid |
          | FLU:2_3     | flu         | https://www.nhs.uk/bookflu   |
        And the following bookings have been made
          | Date               | Time  | Duration  | Service   | Reference   |
          | 1 days from today  | 17:30 | 5         | FLU:94-95 | 68754-23487 |
        When the reminders job runs
        Then no notifications are sent out

    Scenario: Running reminders does not send reminders for recently made appointments	
        Given the site is configured for MYA
        And I have Clinical Services
          | Service     | ServiceType | Url                          |
          | RSV:Adult   | RSV         | https://www.nhs.uk/book-rsv  |
          | COVID:12_17 | COVID-19    | https://www.nhs.uk/bookcovid |
          | FLU:2_3     | flu         | https://www.nhs.uk/bookflu   |
        And the following recent bookings have been made
            | Date              | Time  | Duration | Service |
            | 2 days from today | 09:20 | 5        | COVID:12_17   |
        When the reminders job runs
        Then no notifications are sent out            

    Scenario: Running reminders targets correct document types
        Given the site is configured for MYA
        And I have Clinical Services
          | Service     | ServiceType | Url                          |
          | RSV:Adult   | RSV         | https://www.nhs.uk/book-rsv  |
          | COVID:12_17 | COVID-19    | https://www.nhs.uk/bookcovid |
          | FLU:2_3     | flu         | https://www.nhs.uk/bookflu   |
        And the following bookings have been made
            | Date              | Time  | Duration | Service |
            | Tomorrow          | 12:00 | 5        | COVID:12_17   |
            | 2 days from today | 17:20 | 5        | COVID:12_17   |
        And there are audit entries in the database
        When the reminders job runs
        Then there are no errors
    
    Scenario: Running reminders does not send reminders for past appointments	
        Given the site is configured for MYA
        And I have Clinical Services
          | Service     | ServiceType | Url                          |
          | RSV:Adult   | RSV         | https://www.nhs.uk/book-rsv  |
          | COVID:12_17 | COVID-19    | https://www.nhs.uk/bookcovid |
          | FLU:2_3     | flu         | https://www.nhs.uk/bookflu   |
        And the following bookings have been made
            | Date              | Time  | Duration | Service |
            | 2 days before today | 09:20 | 5        | COVID:12_17   |
        When the reminders job runs
        Then no notifications are sent out

    Scenario: Running reminders does not send reminders for future appointments outside the reminder window
        Given the site is configured for MYA
        And I have Clinical Services
          | Service     | ServiceType | Url                          |
          | RSV:Adult   | RSV         | https://www.nhs.uk/book-rsv  |
          | COVID:12_17 | COVID-19    | https://www.nhs.uk/bookcovid |
          | FLU:2_3     | flu         | https://www.nhs.uk/bookflu   |
        And the following bookings have been made
            | Date              | Time  | Duration | Service |
            | 4 days from today | 09:20 | 5        | COVID:12_17   |
        When the reminders job runs
        Then no notifications are sent out
    
    Scenario: Running reminders does not send reminders when a reminder has already been sent
        Given the site is configured for MYA    
        And I have Clinical Services
          | Service     | ServiceType | Url                          |
          | RSV:Adult   | RSV         | https://www.nhs.uk/book-rsv  |
          | COVID:12_17 | COVID-19    | https://www.nhs.uk/bookcovid |
          | FLU:2_3     | flu         | https://www.nhs.uk/bookflu   |
        And the following bookings have been made
            | Date              | Time  | Duration | Service |
            | 2 days from today | 09:20 | 5        | COVID:12_17   |
        And those appointments have already had notifications sent
        When the reminders job runs
        Then no notifications are sent out
