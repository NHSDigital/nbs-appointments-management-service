Feature: Book an appointment

    Scenario: Make a booking appointment
      Given the site is configured for MYA
      And the following sessions
        | Date       | From  | Until | Services | Slot Length | Capacity |
        | 2077-01-01 | 09:00 | 10:00 | COVID    | 5           | 1        |
      When I make the appointment with the following details
        | DateTime         | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData |
        | 2077-01-01 09:20 | 5        | COVID   | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | true           |
      Then a reference number is returned containing '000001' and the following booking is created
        | DateTime         | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | Provisional | AddtionalData |
        | 2077-01-01 09:20 | 5        | COVID   | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | No          | true          |

    Scenario: Make a provisional booking 
      Given the site is configured for MYA
      And the following sessions
        | Date       | From  | Until | Services | Slot Length | Capacity |
        | 2077-01-01 | 09:00 | 10:00 | COVID    | 5           | 1        |
      When I make a provisional appointment with the following details
        | DateTime         | Duration | Service | NhsNumber  | FirstName | LastName | DOB        |  AdditionalData |
        | 2077-01-01 09:20 | 5        | COVID   | 1234678891 | Test      | One      | 2000-02-01 |  true           |
      Then a reference number is returned containing '000001' and the following booking is created
        | DateTime         | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email | Phone | Provisional | AddtionalData |
        | 2077-01-01 09:20 | 5        | COVID   | 1234678891 | Test      | One      | 2000-02-01 |       |       | Yes         | true          |
              
    Scenario: Cannot book an appointment that is no longer available
      Given the following sessions
        | Date       | From  | Until | Services | Slot Length | Capacity |
        | 2077-01-01 | 09:00 | 10:00 | COVID    | 5           | 1        |
      And the following bookings have been made
        | Date       | Time  | Duration | Service |
        | 2077-01-01 | 09:45 | 5        | COVID   |
      When I make the appointment with the following details
        | DateTime         | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | Provisional | AddtionalData |
        | 2077-01-01 09:45 | 5        | COVID   | 1234678892 | Test      | Two      | 1990-02-01 | test@two.org | 0777777777 | No          | true          |
      Then I receive a message informing me that the appointment is no longer available

    Scenario: Cannot book an appointment that is outside the session
      Given the following sessions
        | Date       | From  | Until | Services | Slot Length | Capacity |
        | 2077-01-01 | 09:00 | 10:00 | COVID    | 5           | 1        |
      When I make the appointment with the following details
        | DateTime         | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email          | Phone      | Provisional | AddtionalData |
        | 2077-01-01 10:00 | 5        | COVID   | 1234678893 | Test      | Three    | 1980-02-01 | test@three.org | 0777777777 | No          | true          |
      Then I receive a message informing me that the appointment is no longer available
      