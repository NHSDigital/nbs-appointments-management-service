Feature: Book an appointment with multiple service configuration

    Scenario: Only one booked booking can be supported when two services are offered
        Given the site is configured for MYA
        And the following sessions
          | Date     | From  | Until | Services      | Slot Length | Capacity |
          | Tomorrow | 11:00 | 12:00 | FLU, COVID    | 5           | 1        |
        When I make the appointment with the following details
          | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
          | Tomorrow | 11:20 | 5        | FLU     | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | true           | 00001234567 |
        Then a reference number is returned and the following booking is created
          | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | Provisional | AdditionalData | Landline    |
          | Tomorrow | 11:20 | 5        | FLU     | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | No          | true           | 00001234567 |
        When I make the appointment with the following details
          | Date     | Time  | Duration | Service   | NhsNumber    | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
          | Tomorrow | 11:20 | 5        | COVID     | 1234678892   | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | true           | 00001234567 |
        Then I receive a message informing me that the appointment is no longer available

    Scenario: Only one provisional booking can be supported when two services are offered
      Given the site is configured for MYA
      And the following sessions
        | Date     | From  | Until | Services      | Slot Length | Capacity |
        | Tomorrow | 11:00 | 12:00 | FLU, COVID    | 5           | 1        |
      When I make a provisional appointment with the following details
        | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | FLU     | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | true           | 00001234567 |
      Then a reference number is returned and the following booking is created
        | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | Provisional | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | FLU     | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | Yes          | true           | 00001234567 |
      When I make a provisional appointment with the following details
        | Date     | Time  | Duration | Service   | NhsNumber    | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | COVID     | 1234678892   | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | true           | 00001234567 |
      Then I receive a message informing me that the appointment is no longer available

    Scenario: Both bookings can be supported when two services are offered and the capacity is two
      Given the site is configured for MYA
      And the following sessions
        | Date     | From  | Until | Services      | Slot Length | Capacity |
        | Tomorrow | 11:00 | 12:00 | FLU, COVID    | 5           | 2        |
      When I make the appointment with the following details
        | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | FLU     | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | true           | 00001234567 |
      Then a reference number is returned and the following booking is created
        | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | Provisional | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | FLU     | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | No          | true           | 00001234567 |
      When I make the appointment with the following details
        | Date     | Time  | Duration | Service   | NhsNumber    | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | COVID     | 1234678892   | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | true           | 00001234567 |
      Then a reference number is returned and the following booking is created
        | Date     | Time  | Duration | Service   | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | Provisional | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | COVID     | 1234678892 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | No          | true           | 00001234567 |

    Scenario: Greedy booking allocation, booked under-utilisation due to service length logic
      Given the site is configured for MYA
      And the following sessions
        | Date     | From  | Until | Services              | Slot Length | Capacity |
        | Tomorrow | 11:00 | 12:00 | FLU, COVID-18, RSV    | 5           | 1        |
        | Tomorrow | 11:00 | 12:00 | FLU, COVID-75         | 5           | 1        |
      When I make the appointment with the following details
        | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | FLU     | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | true           | 00001234567 |
      Then a reference number is returned and the following booking is created
        | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | Provisional | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | FLU     | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | No          | true           | 00001234567 |
      When I make the appointment with the following details
        | Date     | Time  | Duration | Service        | NhsNumber    | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | COVID-75       | 1234678892   | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | true           | 00001234567 |
      Then I receive a message informing me that the appointment is no longer available

    Scenario: Greedy booking allocation, booked under-utilisation due to alphabetical deterministic logic
      Given the site is configured for MYA
      And the following sessions
        | Date     | From  | Until | Services              | Slot Length | Capacity |
        | Tomorrow | 11:00 | 12:00 | FLU, COVID-75         | 5           | 1        |
        | Tomorrow | 11:00 | 12:00 | FLU, COVID-18         | 5           | 1        |
      When I make the appointment with the following details
        | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | FLU     | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | true           | 00001234567 |
      Then a reference number is returned and the following booking is created
        | Date     | Time  | Duration | Service  | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | Provisional | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | FLU      | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | No          | true           | 00001234567 |
      When I make the appointment with the following details
        | Date     | Time  | Duration | Service        | NhsNumber    | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | COVID-18       | 1234678892   | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | true           | 00001234567 |
      Then I receive a message informing me that the appointment is no longer available
      
    Scenario: Greedy booking allocation, provisional under-utilisation due to service length logic
      Given the site is configured for MYA
      And the following sessions
        | Date     | From  | Until | Services              | Slot Length | Capacity |
        | Tomorrow | 11:00 | 12:00 | FLU, COVID-18, RSV    | 5           | 1        |
        | Tomorrow | 11:00 | 12:00 | FLU, COVID-75         | 5           | 1        |
      When I make a provisional appointment with the following details
        | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | FLU     | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | true           | 00001234567 |
      Then a reference number is returned and the following booking is created
        | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | Provisional | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | FLU     | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | Yes          | true           | 00001234567 |
      When I make a provisional appointment with the following details
        | Date     | Time  | Duration | Service        | NhsNumber    | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | COVID-75       | 1234678892   | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | true           | 00001234567 |
      Then I receive a message informing me that the appointment is no longer available

    Scenario: Greedy booking allocation, provisional under-utilisation due to alphabetical deterministic logic
      Given the site is configured for MYA
      And the following sessions
        | Date     | From  | Until | Services      | Slot Length | Capacity |
        | Tomorrow | 11:00 | 12:00 | FLU, COVID-75 | 5           | 1        |
        | Tomorrow | 11:00 | 12:00 | FLU, COVID-18 | 5           | 1        |
      When I make a provisional appointment with the following details
        | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | FLU     | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | true           | 00001234567 |
      Then a reference number is returned and the following booking is created
        | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | Provisional | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | FLU     | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | Yes          | true           | 00001234567 |
      When I make a provisional appointment with the following details
        | Date     | Time  | Duration | Service        | NhsNumber    | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | COVID-18       | 1234678892   | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | true           | 00001234567 |
      Then I receive a message informing me that the appointment is no longer available

    Scenario: Greedy booking allocation, booked optimal-utilisation due to service length logic
      Given the site is configured for MYA
      And the following sessions
        | Date     | From  | Until | Services              | Slot Length | Capacity |
        | Tomorrow | 11:00 | 12:00 | FLU, COVID-18, RSV    | 5           | 1        |
        | Tomorrow | 11:00 | 12:00 | FLU, COVID-75         | 5           | 1        |
      When I make the appointment with the following details
        | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | FLU     | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | true           | 00001234567 |
      Then a reference number is returned and the following booking is created
        | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | Provisional | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | FLU     | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | No          | true           | 00001234567 |
      When I make a provisional appointment with the following details
        | Date     | Time  | Duration | Service        | NhsNumber    | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | COVID-18       | 1234678892   | Test      | Two      | 2000-02-01 | test@two.org | 0123456789 | true           | 00001234568 |
      Then a reference number is returned and the following booking is created
        | Date     | Time  | Duration | Service      | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | Provisional  | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | COVID-18     | 1234678892 | Test      | Two      | 2000-02-01 | test@two.org | 0123456789 | Yes          | true           | 00001234568 |

    Scenario: Greedy booking allocation, booked optimal-utilisation due to alphabetical deterministic logic
      Given the site is configured for MYA
      And the following sessions
        | Date     | From  | Until | Services      | Slot Length | Capacity |
        | Tomorrow | 11:00 | 12:00 | FLU, COVID-75 | 5           | 1        |
        | Tomorrow | 11:00 | 12:00 | FLU, COVID-18 | 5           | 1        |
      When I make the appointment with the following details
        | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | FLU     | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | true           | 00001234567 |
      Then a reference number is returned and the following booking is created
        | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | Provisional | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | FLU     | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | No          | true           | 00001234567 |
      When I make a provisional appointment with the following details
        | Date     | Time  | Duration | Service        | NhsNumber    | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | COVID-75       | 1234678892   | Test      | Two      | 2000-02-01 | test@two.org | 0123456789 | true           | 00001234568 |
      Then a reference number is returned and the following booking is created
        | Date     | Time  | Duration | Service      | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | Provisional  | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | COVID-75     | 1234678892 | Test      | Two      | 2000-02-01 | test@two.org | 0123456789 | Yes          | true           | 00001234568 |
      
    Scenario: Greedy booking allocation, provisional optimal-utilisation due to service length logic
      Given the site is configured for MYA
      And the following sessions
        | Date     | From  | Until | Services              | Slot Length | Capacity |
        | Tomorrow | 11:00 | 12:00 | FLU, COVID-18, RSV    | 5           | 1        |
        | Tomorrow | 11:00 | 12:00 | FLU, COVID-75         | 5           | 1        |
      When I make a provisional appointment with the following details
        | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | FLU     | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | true           | 00001234567 |
      Then a reference number is returned and the following booking is created
        | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | Provisional | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | FLU     | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | Yes          | true           | 00001234567 |
      When I make a provisional appointment with the following details
        | Date     | Time  | Duration | Service        | NhsNumber    | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | COVID-18       | 1234678892   | Test      | Two      | 2000-02-01 | test@two.org | 0123456789 | true           | 00001234568 |
      Then a reference number is returned and the following booking is created
        | Date     | Time  | Duration | Service      | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | Provisional  | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | COVID-18     | 1234678892 | Test      | Two      | 2000-02-01 | test@two.org | 0123456789 | Yes          | true           | 00001234568 |

    Scenario: Greedy booking allocation, provisional optimal-utilisation due to alphabetical deterministic logic
      Given the site is configured for MYA
      And the following sessions
        | Date     | From  | Until | Services      | Slot Length | Capacity |
        | Tomorrow | 11:00 | 12:00 | FLU, COVID-75 | 5           | 1        |
        | Tomorrow | 11:00 | 12:00 | FLU, COVID-18 | 5           | 1        |
      When I make a provisional appointment with the following details
        | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | FLU     | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | true           | 00001234567 |
      Then a reference number is returned and the following booking is created
        | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | Provisional | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | FLU     | 1234678891 | Test      | One      | 2000-02-01 | test@one.org | 0123456789 | Yes          | true           | 00001234567 |
      When I make a provisional appointment with the following details
        | Date     | Time  | Duration | Service        | NhsNumber    | FirstName | LastName | DOB        | Email        | Phone      | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | COVID-75       | 1234678892   | Test      | Two      | 2000-02-01 | test@two.org | 0123456789 | true           | 00001234568 |
      Then a reference number is returned and the following booking is created
        | Date     | Time  | Duration | Service      | NhsNumber  | FirstName | LastName | DOB        | Email        | Phone      | Provisional  | AdditionalData | Landline    |
        | Tomorrow | 11:20 | 5        | COVID-75     | 1234678892 | Test      | Two      | 2000-02-01 | test@two.org | 0123456789 | Yes          | true           | 00001234568 |
      
    Scenario: Cannot book an appointment for a service that is not covered in the session
        Given the site is configured for MYA
        Given the following sessions
          | Date     | From  | Until | Services           | Slot Length | Capacity |
          | Tomorrow | 09:00 | 10:00 | COVID-18, COVID-64 | 5           | 1        |
        When I make the appointment with the following details
          | Date     | Time  | Duration | Service    | NhsNumber  | FirstName | LastName | DOB        | Email          | Phone      | Provisional | AdditionalData | Landline    |
          | Tomorrow | 09:00 | 5        | COVID-75   | 1234678893 | Test      | Three    | 1980-02-01 | test@three.org | 0777777777 | No          | true           | 00001234567 |
        Then I receive a message informing me that the appointment is no longer available

    Scenario: Cannot book an appointment for a service that is not covered in a session in the time range
      Given the site is configured for MYA
      Given the following sessions
        | Date     | From  | Until | Services | Slot Length | Capacity |
        | Tomorrow | 10:00 | 11:00 | COVID-18 | 5           | 1        |
        | Tomorrow | 09:00 | 10:00 | COVID-75 | 5           | 1        |
      When I make the appointment with the following details
        | Date     | Time  | Duration | Service    | NhsNumber  | FirstName | LastName | DOB        | Email          | Phone      | Provisional | AdditionalData | Landline    |
        | Tomorrow | 10:00 | 5        | COVID-75   | 1234678893 | Test      | Three    | 1980-02-01 | test@three.org | 0777777777 | No          | true           | 00001234567 |
      Then I receive a message informing me that the appointment is no longer available

    Scenario: Cannot book an appointment for a service that is covered in a session in the time range, but with a different slot length
      Given the site is configured for MYA
      Given the following sessions
        | Date     | From  | Until | Services           | Slot Length | Capacity |
        | Tomorrow | 10:00 | 11:00 | COVID-18, COVID-75 | 5           | 1        |
      When I make the appointment with the following details
        | Date     | Time  | Duration | Service    | NhsNumber  | FirstName | LastName | DOB        | Email          | Phone      | Provisional | AdditionalData | Landline    |
        | Tomorrow | 10:10 | 10       | COVID-75   | 1234678893 | Test      | Three    | 1980-02-01 | test@three.org | 0777777777 | No          | true           | 00001234567 |
      Then I receive a message informing me that the appointment is no longer available
