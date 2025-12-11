Feature: Blob Audit Audit Data

  Scenario: Files appear in Blob
    When I add a file to 'audit_data' container
    Then a lease token is present in 'blob_audit' container with application 'audit_auditor'  
    And files appear in blob 
    
  Scenario: Files appear in Blob
    When I add multiple files to 'audit_data' container
    Then a lease token is present in 'blob_audit' container with application 'audit_auditor'  
    And files appear in blob 
    
  Scenario: Files appear in Blob
    When I add a file to 'core_data' container
    And I update that file
    And I update that file 
    Then a lease token is present in 'blob_audit' container with application 'audit_auditor'  
    And files appear in blob 
  
    
