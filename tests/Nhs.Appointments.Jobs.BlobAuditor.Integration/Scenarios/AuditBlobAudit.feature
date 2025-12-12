Feature: Blob Audit Audit Data

  Scenario: Files appear in Blob
    When I add a file to container
    Then files appear in blob
    And clean data 
    
  Scenario: Files appear in Blob
    When I add multiple files to container
    Then files appear in blob 
    And clean data 
    
  Scenario: Files appear in Blob
    When I add a file to container
    And I update that file
    And I update that file   
    Then files appear in blob 
    And clean data 
  
    
