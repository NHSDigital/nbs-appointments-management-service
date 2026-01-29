Feature: RunReminders
  
    Scenario: Running reminders sends reminders for upcoming appointments - Covid 19
        Given the site is configured for MYA
        And a new api user 'reminderjob' is registered with a http client
        And I have Clinical Services
          | Id              | Label               | ServiceType      | Url                                |
          | RSV:Adult       | RSV Adult           | RSV              | https://www.nhs.uk/book-rsv        |
          | COVID:5_11      | COVID 5-11          | COVID-19         | https://www.nhs.uk/bookcovid       |
          | COVID:12_17     | COVID 12-17         | COVID-19         | https://www.nhs.uk/bookcovid       |
          | COVID:18+       | COVID 18+           | COVID-19         | https://www.nhs.uk/bookcovid       |
          | FLU:18_64       | Flu 18-64           | flu              | https://www.nhs.uk/bookflu         |
          | FLU:65+         | Flu 65+             | flu              | https://www.nhs.uk/bookflu         |
          | COVID_FLU:18_64 | Flu and COVID 18-64 | COVID-19 and flu | https://www.nhs.uk/get-vaccination |
          | COVID_FLU:65+   | Flu and COVID 65+   | COVID-19 and flu | https://www.nhs.uk/get-vaccination |
          | FLU:2_3         | Flu 2-3             | flu              | https://www.nhs.uk/bookflu         |
        And the following bookings have been made
            | Date              | Time  | Duration | Service     | Reference   |
            | 2 days from today | 09:20 | 5        | COVID:12_17 | 56345-09354 |
        When the reminders job runs for api user 'reminderjob'
        Then the following notifications are sent out
            | Contact Type | TemplateId                             | Vaccine  | Url                          | Reference   | 
            | email        | 36b50be4-fad7-440e-8958-b0dc66efe33a   | COVID-19 | https://www.nhs.uk/bookcovid | 56345-09354 |
            | phone        | c452be27-5bae-422d-9691-6bb955dbc51f   | COVID-19 | https://www.nhs.uk/bookcovid | 56345-09354 |
        # Because we trigger the reminder job manually in this test, the user is the API user who invoked it
        And the booking document with reference '56345-09354' has lastUpdatedBy 'api@reminderjob'
      
    Scenario: Running reminders sends reminders for upcoming appointments - Flu
        Given the site is configured for MYA
        And a new api user 'reminderjob' is registered with a http client
        And I have Clinical Services
          | Id              | Label               | ServiceType      | Url                                |
          | RSV:Adult       | RSV Adult           | RSV              | https://www.nhs.uk/book-rsv        |
          | COVID:5_11      | COVID 5-11          | COVID-19         | https://www.nhs.uk/bookcovid       |
          | COVID:12_17     | COVID 12-17         | COVID-19         | https://www.nhs.uk/bookcovid       |
          | COVID:18+       | COVID 18+           | COVID-19         | https://www.nhs.uk/bookcovid       |
          | FLU:18_64       | Flu 18-64           | flu              | https://www.nhs.uk/bookflu         |
          | FLU:65+         | Flu 65+             | flu              | https://www.nhs.uk/bookflu         |
          | COVID_FLU:18_64 | Flu and COVID 18-64 | COVID-19 and flu | https://www.nhs.uk/get-vaccination |
          | COVID_FLU:65+   | Flu and COVID 65+   | COVID-19 and flu | https://www.nhs.uk/get-vaccination |
          | FLU:2_3         | Flu 2-3             | flu              | https://www.nhs.uk/bookflu         |
        And the following bookings have been made
          | Date               | Time  | Duration | Service | Reference   |
          | 1 days from today  | 17:30 | 5        | FLU:2_3 | 98067-23487 |
        # default state is api@test
        And the booking document with reference '98067-23487' has lastUpdatedBy 'api@test'
        When the reminders job runs for api user 'reminderjob'
        Then the following notifications are sent out
          | Contact Type | TemplateId                             | Vaccine | Url                          | Reference   |
          | email        | 36b50be4-fad7-440e-8958-b0dc66efe33a   | flu     | https://www.nhs.uk/bookflu   | 98067-23487 | 
          | phone        | c452be27-5bae-422d-9691-6bb955dbc51f   | flu     | https://www.nhs.uk/bookflu   | 98067-23487 |
        # Because we trigger the reminder job manually in this test, the user is the API user who invoked it
        And the booking document with reference '98067-23487' has lastUpdatedBy 'api@reminderjob'

    Scenario: Running reminders does not send a reminder if the service is not supported
        Given the site is configured for MYA
        And a new api user 'reminderjob' is registered with a http client
        And I have Clinical Services
          | Id              | Label               | ServiceType      | Url                                |
          | RSV:Adult       | RSV Adult           | RSV              | https://www.nhs.uk/book-rsv        |
          | COVID:5_11      | COVID 5-11          | COVID-19         | https://www.nhs.uk/bookcovid       |
          | COVID:12_17     | COVID 12-17         | COVID-19         | https://www.nhs.uk/bookcovid       |
          | COVID:18+       | COVID 18+           | COVID-19         | https://www.nhs.uk/bookcovid       |
          | FLU:18_64       | Flu 18-64           | flu              | https://www.nhs.uk/bookflu         |
          | FLU:65+         | Flu 65+             | flu              | https://www.nhs.uk/bookflu         |
          | COVID_FLU:18_64 | Flu and COVID 18-64 | COVID-19 and flu | https://www.nhs.uk/get-vaccination |
          | COVID_FLU:65+   | Flu and COVID 65+   | COVID-19 and flu | https://www.nhs.uk/get-vaccination |
          | FLU:2_3         | Flu 2-3             | flu              | https://www.nhs.uk/bookflu         |
        And the following bookings have been made
          | Date               | Time  | Duration  | Service   | Reference   |
          | 1 days from today  | 17:30 | 5         | FLU:94-95 | 68754-23487 |
        # default state is api@test
        And the booking document with reference '68754-23487' has lastUpdatedBy 'api@test'
        When the reminders job runs for api user 'reminderjob'
        Then no notifications are sent out
        # Questionable existing behaviour due to how we set the 'ReminderSent' flag to represent the event being created, rather than successful consumption and notification actually sent
        # So even though the notification isn't sent, the lastUpdatedBy is the Reminder Job
        And the booking document with reference '68754-23487' has lastUpdatedBy 'api@reminderjob'

    Scenario: Running reminders does not send reminders for recently made appointments	
        Given the site is configured for MYA
        And a new api user 'reminderjob' is registered with a http client
        And I have Clinical Services
          | Id              | Label               | ServiceType      | Url                                |
          | RSV:Adult       | RSV Adult           | RSV              | https://www.nhs.uk/book-rsv        |
          | COVID:5_11      | COVID 5-11          | COVID-19         | https://www.nhs.uk/bookcovid       |
          | COVID:12_17     | COVID 12-17         | COVID-19         | https://www.nhs.uk/bookcovid       |
          | COVID:18+       | COVID 18+           | COVID-19         | https://www.nhs.uk/bookcovid       |
          | FLU:18_64       | Flu 18-64           | flu              | https://www.nhs.uk/bookflu         |
          | FLU:65+         | Flu 65+             | flu              | https://www.nhs.uk/bookflu         |
          | COVID_FLU:18_64 | Flu and COVID 18-64 | COVID-19 and flu | https://www.nhs.uk/get-vaccination |
          | COVID_FLU:65+   | Flu and COVID 65+   | COVID-19 and flu | https://www.nhs.uk/get-vaccination |
          | FLU:2_3         | Flu 2-3             | flu              | https://www.nhs.uk/bookflu         |
        And the following recent bookings have been made
            | Date              | Time  | Duration | Service |
            | 2 days from today | 09:20 | 5        | COVID:12_17   |
        When the reminders job runs for api user 'reminderjob'
        Then no notifications are sent out            

    Scenario: Running reminders targets correct document types
        Given the site is configured for MYA
        And a new api user 'reminderjob' is registered with a http client
        And I have Clinical Services
          | Id              | Label               | ServiceType      | Url                                |
          | RSV:Adult       | RSV Adult           | RSV              | https://www.nhs.uk/book-rsv        |
          | COVID:5_11      | COVID 5-11          | COVID-19         | https://www.nhs.uk/bookcovid       |
          | COVID:12_17     | COVID 12-17         | COVID-19         | https://www.nhs.uk/bookcovid       |
          | COVID:18+       | COVID 18+           | COVID-19         | https://www.nhs.uk/bookcovid       |
          | FLU:18_64       | Flu 18-64           | flu              | https://www.nhs.uk/bookflu         |
          | FLU:65+         | Flu 65+             | flu              | https://www.nhs.uk/bookflu         |
          | COVID_FLU:18_64 | Flu and COVID 18-64 | COVID-19 and flu | https://www.nhs.uk/get-vaccination |
          | COVID_FLU:65+   | Flu and COVID 65+   | COVID-19 and flu | https://www.nhs.uk/get-vaccination |
          | FLU:2_3         | Flu 2-3             | flu              | https://www.nhs.uk/bookflu         |
        And the following bookings have been made
            | Date              | Time  | Duration | Service |
            | Tomorrow          | 12:00 | 5        | COVID:12_17   |
            | 2 days from today | 17:20 | 5        | COVID:12_17   |
        And there are audit entries in the database
        When the reminders job runs for api user 'reminderjob'
        Then there are no errors
    
    Scenario: Running reminders does not send reminders for past appointments	
        Given the site is configured for MYA
        And a new api user 'reminderjob' is registered with a http client
        And I have Clinical Services
          | Id              | Label               | ServiceType      | Url                                |
          | RSV:Adult       | RSV Adult           | RSV              | https://www.nhs.uk/book-rsv        |
          | COVID:5_11      | COVID 5-11          | COVID-19         | https://www.nhs.uk/bookcovid       |
          | COVID:12_17     | COVID 12-17         | COVID-19         | https://www.nhs.uk/bookcovid       |
          | COVID:18+       | COVID 18+           | COVID-19         | https://www.nhs.uk/bookcovid       |
          | FLU:18_64       | Flu 18-64           | flu              | https://www.nhs.uk/bookflu         |
          | FLU:65+         | Flu 65+             | flu              | https://www.nhs.uk/bookflu         |
          | COVID_FLU:18_64 | Flu and COVID 18-64 | COVID-19 and flu | https://www.nhs.uk/get-vaccination |
          | COVID_FLU:65+   | Flu and COVID 65+   | COVID-19 and flu | https://www.nhs.uk/get-vaccination |
          | FLU:2_3         | Flu 2-3             | flu              | https://www.nhs.uk/bookflu         |
        And the following bookings have been made
            | Date              | Time  | Duration | Service |
            | 2 days before today | 09:20 | 5        | COVID:12_17   |
        When the reminders job runs for api user 'reminderjob'
        Then no notifications are sent out

    Scenario: Running reminders does not send reminders for future appointments outside the reminder window
        Given the site is configured for MYA
        And a new api user 'reminderjob' is registered with a http client
        And I have Clinical Services
          | Id              | Label               | ServiceType      | Url                                |
          | RSV:Adult       | RSV Adult           | RSV              | https://www.nhs.uk/book-rsv        |
          | COVID:5_11      | COVID 5-11          | COVID-19         | https://www.nhs.uk/bookcovid       |
          | COVID:12_17     | COVID 12-17         | COVID-19         | https://www.nhs.uk/bookcovid       |
          | COVID:18+       | COVID 18+           | COVID-19         | https://www.nhs.uk/bookcovid       |
          | FLU:18_64       | Flu 18-64           | flu              | https://www.nhs.uk/bookflu         |
          | FLU:65+         | Flu 65+             | flu              | https://www.nhs.uk/bookflu         |
          | COVID_FLU:18_64 | Flu and COVID 18-64 | COVID-19 and flu | https://www.nhs.uk/get-vaccination |
          | COVID_FLU:65+   | Flu and COVID 65+   | COVID-19 and flu | https://www.nhs.uk/get-vaccination |
          | FLU:2_3         | Flu 2-3             | flu              | https://www.nhs.uk/bookflu         |
        And the following bookings have been made
            | Date              | Time  | Duration | Service |
            | 4 days from today | 09:20 | 5        | COVID:12_17   |
        When the reminders job runs for api user 'reminderjob'
        Then no notifications are sent out
    
    Scenario: Running reminders does not send reminders when a reminder has already been sent
        Given the site is configured for MYA
        And a new api user 'reminderjob' is registered with a http client
        And I have Clinical Services
          | Id              | Label               | ServiceType      | Url                                |
          | RSV:Adult       | RSV Adult           | RSV              | https://www.nhs.uk/book-rsv        |
          | COVID:5_11      | COVID 5-11          | COVID-19         | https://www.nhs.uk/bookcovid       |
          | COVID:12_17     | COVID 12-17         | COVID-19         | https://www.nhs.uk/bookcovid       |
          | COVID:18+       | COVID 18+           | COVID-19         | https://www.nhs.uk/bookcovid       |
          | FLU:18_64       | Flu 18-64           | flu              | https://www.nhs.uk/bookflu         |
          | FLU:65+         | Flu 65+             | flu              | https://www.nhs.uk/bookflu         |
          | COVID_FLU:18_64 | Flu and COVID 18-64 | COVID-19 and flu | https://www.nhs.uk/get-vaccination |
          | COVID_FLU:65+   | Flu and COVID 65+   | COVID-19 and flu | https://www.nhs.uk/get-vaccination |
          | FLU:2_3         | Flu 2-3             | flu              | https://www.nhs.uk/bookflu         |
        And the following bookings have been made
            | Date              | Time  | Duration | Service |
            | 2 days from today | 09:20 | 5        | COVID:12_17   |
        And those appointments have already had notifications sent
        When the reminders job runs for api user 'reminderjob'
        Then no notifications are sent out
