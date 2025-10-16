# Description

Please include a summary of the change and which issue is fixed. Please also include relevant motivation and context. List any dependencies that are required for this change.

Fixes # (issue)

# Checklist:

- [ ] My work is behind a feature toggle (if appropriate)
- [ ] If my work is behind a feature toggle, I've added a full suite of tests for both the ON and OFF state
- [ ] The ticket number is in the Pull Request title, with format "APPT-XXX: My Title Here"
- [ ] I have ran npm tsc / lint (in the future these will be ran automatically)
- [ ] My code generates no new .NET warnings (in the future these will be treated as errors)
- [ ] If I've added a new Function, it is disabled in all but one of the terraform groups (e.g. http_functions)
- [ ] If I've added a new Function, it has both unit and integration tests. Any request body validators have unit tests also
- [ ] If I've made UI changes, I've added appropriate Playwright and Jest tests
- [ ] If I have added a new end-point, I've added the appropriate annotations and I have tested that the Swagger documentation reflects my changes
