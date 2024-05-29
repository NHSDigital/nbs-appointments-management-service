Feature: Get daily availability

  Background:
    Given The following service configuration
      | Code          | Duration |
      | COVID         | 5        |
      | FLU           | 8        |
      | COADMIN       | 10       |

  Scenario: Dates and availability are returned from session templates with 5 min appointments
    Given The following week templates
      | Name | Days | From  | Until | Services |
      | Test | All  | 09:00 | 17:00 | COVID    |
    And the following template assignments
      | Template | From       | Until      |
      | Test     | 2077-01-01 | 2077-01-31 |
    When I check daily availability for 'COVID' between '2077-01-01' and '2077-01-03'
    Then the following daily availability is returned
      | Date       | Am | Pm |
      | 2077-01-01 | 36 | 60 |
      | 2077-01-02 | 36 | 60 |
      | 2077-01-03 | 36 | 60 |
    
  Scenario: Dates and availability are returned from session templates with 10 min appointments
    Given The following week templates
      | Name | Days | From  | Until | Services |
      | Test | All  | 09:00 | 10:00 | COADMIN  |
    And the following template assignments
      | Template | From       | Until      |
      | Test     | 2077-01-01 | 2077-01-31 |
    When I check daily availability for 'COADMIN' between '2077-01-01' and '2077-01-03'
    Then the following daily availability is returned
      | Date       | Am | Pm |
      | 2077-01-01 | 6 | 0   |
      | 2077-01-02 | 6 | 0   |
      | 2077-01-03 | 6 | 0   |

  Scenario: Dates and availability are returned from session templates with 8 min appointments
    Given The following week templates
      | Name | Days | From  | Until | Services |
      | Test | All  | 09:00 | 10:00 | FLU      |
    And the following template assignments
      | Template | From       | Until      |
      | Test     | 2077-01-01 | 2077-01-31 |
    When I check daily availability for 'FLU' between '2077-01-01' and '2077-01-03'
    Then the following daily availability is returned
      | Date       | Am | Pm |
      | 2077-01-01 | 7 | 0   |
      | 2077-01-02 | 7 | 0   |
      | 2077-01-03 | 7 | 0   |

  Scenario: Templates can specify certain days
    Given The following week templates
      | Name | Days   | From  | Until | Services |
      | Test | Friday | 09:00 | 10:00 | COVID    |
    And the following template assignments
      | Template | From       | Until      |
      | Test     | 2077-01-01 | 2077-01-31 |
    When I check daily availability for 'COVID' between '2077-01-01' and '2077-01-03'
    Then the following daily availability is returned
      | Date       | Am | Pm |
      | 2077-01-01 | 12 | 0  |
      | 2077-01-02 | 0  | 0  |
      | 2077-01-03 | 0  | 0  |

  Scenario: Availability is split between AM and PM
    Given The following week templates
      | Name | Days | From  | Until | Services |
      | Test | All  | 11:00 | 13:00 | COVID    |
    And the following template assignments
      | Template | From       | Until      |
      | Test     | 2077-01-01 | 2077-01-31 |
    When I check daily availability for 'COVID' between '2077-01-01' and '2077-01-03'
    Then the following daily availability is returned
      | Date       | Am | Pm |
      | 2077-01-01 | 12 | 12 |
      | 2077-01-02 | 12 | 12 |
      | 2077-01-03 | 12 | 12 |

  Scenario: Bookings take up availability
    Given The following week templates
      | Name | Days | From  | Until | Services |
      | Test | All  | 09:00 | 10:00 | COVID    |
    And the following template assignments
      | Template | From       | Until      |
      | Test     | 2077-01-01 | 2077-01-31 |
    And the following bookings have been made
      | Date       | Time  | Duration | Service |
      | 2077-01-02 | 09:20 | 5        | COVID   |
    When I check daily availability for 'COVID' between '2077-01-01' and '2077-01-03'
    Then the following daily availability is returned
      | Date       | Am | Pm |
      | 2077-01-01 | 12 | 0  |
      | 2077-01-02 | 11 | 0  |
      | 2077-01-03 | 12 | 0  |

  Scenario: Availability requests check for bookings for all service types configured at the requested site 
    Given The following week templates
      | Name | Days | From  | Until | Services  |
      | Test | All  | 09:00 | 10:00 | COVID,FLU |
    And the following template assignments
      | Template | From       | Until      |
      | Test     | 2077-01-01 | 2077-01-31 |
    And the following bookings have been made
      | Date       | Time  | Duration | Service |
      | 2077-01-02 | 09:00 | 5        | COVID   |
    When I check daily availability for 'FLU' between '2077-01-01' and '2077-01-03'
    Then the following daily availability is returned
      | Date       | Am | Pm |
      | 2077-01-01 | 7  | 0  |
      | 2077-01-02 | 6  | 0  |
      | 2077-01-03 | 7  | 0  |
    
  Scenario: Bookings are not checked if availability is requested for a service type not configured at that site
    Given The following week templates
      | Name | Days | From  | Until | Services          |
      | Test | All  | 09:00 | 10:00 | COVID,FLU,COADMIN |
    And the following template assignments
      | Template | From       | Until      |
      | Test     | 2077-01-01 | 2077-01-31 |
    And the following bookings have been made
      | Date       | Time  | Duration | Service |
      | 2077-01-01 | 09:20 | 5        | COVID   |
    When I check daily availability for 'OTHER_SERVICE_TYPE' between '2077-01-01' and '2077-01-01'
    Then the request is successful and no availability is returned
 
  Scenario: Blocks too small to fit appointment are not reported as availability
    Given The following week templates
      | Name | Days | From  | Until | Services |
      | Test | All  | 09:00 | 10:00 | COADMIN  |
    And the following template assignments
      | Template | From       | Until      |
      | Test     | 2077-01-01 | 2077-01-31 |
    And the following bookings have been made
      | Date       | Time  | Duration | Service |
      | 2077-01-02 | 09:45 | 10       | COVID   |
    When I check daily availability for 'COADMIN' between '2077-01-01' and '2077-01-03'
    Then the following daily availability is returned
      | Date       | Am | Pm |
      | 2077-01-01 | 6  | 0  |
      | 2077-01-02 | 4  | 0  |
      | 2077-01-03 | 6  | 0  |

  Scenario: Error returned when an invalid availability query request is sent
    When I send an invalid availability query request
    Then a bad request error is returned