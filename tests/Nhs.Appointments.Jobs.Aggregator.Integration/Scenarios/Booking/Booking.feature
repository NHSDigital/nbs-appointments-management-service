Feature: Booking Aggregations

  Scenario: Creating a booking will Aggregate
  Given I have a Booking
  When inserted into the database
  And I wait for the aggregation to be created
  Then the aggregation should be as expected
  
  Scenario: Cancelling a booking will Aggregate
  Given I have a Booking
  When inserted into the database
  And I wait for the aggregation to be created
  Then the aggregation should be as expected
  When I cancel the booking
  And I wait for the aggregation to be updated
  Then the aggregation should be as expected
    
