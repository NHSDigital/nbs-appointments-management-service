Feature: Get Clinical Services

  Scenario: Get Clinical Services
    And the following clinical services exist
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
      | COVID_RSV:18+   | RSV and COVID 18+   | RSV and COVID-19 | https://www.nhs.uk/book-rsv        |
    When I request Clinical Services
    Then the request should return Clinical Services
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
      | COVID_RSV:18+   | RSV and COVID 18+   | RSV and COVID-19 | https://www.nhs.uk/book-rsv        |
    Then the request should be successful

  Scenario: Clinical Services are returned in the prioritized sequence
    And the following clinical services exist
      | Id              | Label               | ServiceType      | Url                                |
      | FLU:2_3         | Flu 2-3             | flu              | https://www.nhs.uk/bookflu         |
      | FLU:18_64       | Flu 18-64           | flu              | https://www.nhs.uk/bookflu         |
      | FLU:65+         | Flu 65+             | flu              | https://www.nhs.uk/bookflu         |
      | COVID:5_11      | COVID 5-11          | COVID-19         | https://www.nhs.uk/bookcovid       |
      | COVID:12_17     | COVID 12-17         | COVID-19         | https://www.nhs.uk/bookcovid       |
      | COVID:18+       | COVID 18+           | COVID-19         | https://www.nhs.uk/bookcovid       |
      | COVID_FLU:18_64 | Flu and COVID 18-64 | COVID-19 and flu | https://www.nhs.uk/get-vaccination |
      | COVID_FLU:65+   | Flu and COVID 65+   | COVID-19 and flu | https://www.nhs.uk/get-vaccination |
      | RSV:Adult       | RSV Adult           | RSV              | https://www.nhs.uk/book-rsv        |
      | COVID_RSV:18+   | RSV and COVID 18+   | RSV and COVID-19 | https://www.nhs.uk/book-rsv        |
      
    When I request Clinical Services
    Then the request should return Clinical Services in exact order
      | Id              |
      | FLU:2_3         |
      | FLU:18_64       |
      | FLU:65+         |
      | COVID:5_11      |
      | COVID:12_17     |
      | COVID:18+       |
      | COVID_FLU:18_64 |
      | COVID_FLU:65+   |
      | RSV:Adult       |
      | COVID_RSV:18+   |
      
    Then the request should be successful
