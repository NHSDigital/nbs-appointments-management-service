Feature: Get daily availability

    Scenario: Dates and availability are returned from sessions with 5 min appointments
        Given the following sessions
          | Date        | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow    | 09:00 | 17:00 | COVID    | 5           | 1        |
          | 2 days from today | 09:00 | 17:00 | COVID    | 5           | 1        |
          | 3 days from today | 09:00 | 17:00 | COVID    | 5           | 1        |
        When I check daily availability for 'COVID' between 'Tomorrow' and '3 days from today'
        Then the following daily availability is returned
          | Date        | Am | Pm |
          | Tomorrow    | 36 | 60 |
          | 2 days from today | 36 | 60 |
          | 3 days from today | 36 | 60 |

    Scenario: Dates and availability are returned from sessions with 5 min appointments and multiple capacity
        Given the following sessions
          | Date        | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow    | 09:00 | 17:00 | COVID    | 5           | 2        |
          | 2 days from today | 09:00 | 17:00 | COVID    | 5           | 3        |
          | 3 days from today | 09:00 | 17:00 | COVID    | 5           | 4        |
        When I check daily availability for 'COVID' between 'Tomorrow' and '3 days from today'
        Then the following daily availability is returned
          | Date        | Am  | Pm  |
          | Tomorrow    | 72  | 120 |
          | 2 days from today | 108 | 180 |
          | 3 days from today | 144 | 240 |

    Scenario: Dates and availability are returned from overlapping sessions
        Given the following sessions
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 2        |
          | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 3        |
        When I check daily availability for 'COVID' between 'Tomorrow' and '3 days from today'
        Then the following daily availability is returned
          | Date        | Am | Pm |
          | Tomorrow    | 60 | 0  |
          | 2 days from today | 0  | 0  |
          | 3 days from today | 0  | 0  |

    Scenario: Bookings take up availability
        Given the following sessions
          | Date        | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow    | 09:00 | 10:00 | COVID    | 5           | 1        |
          | 2 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
          | 3 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
        And the following bookings have been made
          | Date        | Time  | Duration | Service |
          | 2 days from today | 09:20 | 5        | COVID   |
        When I check daily availability for 'COVID' between 'Tomorrow' and '3 days from today'
        Then the following daily availability is returned
          | Date        | Am | Pm |
          | Tomorrow    | 12 | 0  |
          | 2 days from today | 11 | 0  |
          | 3 days from today | 12 | 0  |

    Scenario: Provisional bookings take up availability
        Given the following sessions
          | Date        | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow    | 09:00 | 10:00 | COVID    | 5           | 1        |
          | 2 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
          | 3 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
        And the following provisional bookings have been made
          | Date        | Time  | Duration | Service |
          | 2 days from today | 09:20 | 5        | COVID   |
        When I check daily availability for 'COVID' between 'Tomorrow' and '3 days from today'
        Then the following daily availability is returned
          | Date        | Am | Pm |
          | Tomorrow    | 12 | 0  |
          | 2 days from today | 11 | 0  |
          | 3 days from today | 12 | 0  |

    Scenario: Expired provisional bookings don't take up availability
        Given the following sessions
          | Date        | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow    | 09:00 | 10:00 | COVID    | 5           | 1        |
          | 2 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
          | 3 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
        And the following expired provisional bookings have been made
          | Date        | Time  | Duration | Service |
          | 2 days from today | 09:20 | 5        | COVID   |
        When I check daily availability for 'COVID' between 'Tomorrow' and '3 days from today'
        Then the following daily availability is returned
          | Date        | Am | Pm |
          | Tomorrow    | 12 | 0  |
          | 2 days from today | 12 | 0  |
          | 3 days from today | 12 | 0  |