Feature: Blob Audit Core Data

  Scenario: Files appear in Blob
    When I add a file to 'core_data' container
    Then a lease token is present in 'blob_audit' container with application 'core_auditor'  
    And files appear in blob 
    
  Scenario: Files appear in Blob
    When I add multiple files to 'core_data' container
    Then a lease token is present in 'blob_audit' container with application 'core_auditor'  
    And files appear in blob 
    
  Scenario: Files appear in Blob
    When I add a file to 'core_data' container
    And I update that file
    And I update that file 
    Then a lease token is present in 'blob_audit' container with application 'core_auditor'  
    And files appear in blob 
  
    
