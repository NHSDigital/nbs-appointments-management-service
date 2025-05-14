import { test, expect } from '../../fixtures';

test('The current user creates a new NHSMail user with some roles', async ({
  newUserName,
  signInToSite,
}) => {
  await signInToSite()
    .then(sitePage => sitePage.clickManageUsersCard())
    .then(manageUsersPage => manageUsersPage.clickAddUser())
    .then(async manageUserPage => {
      await expect(manageUserPage.emailStep.title).toBeVisible();
      await manageUserPage.emailStep.emailInput.fill(newUserName);
      await manageUserPage.emailStep.continueButton.click();

      await expect(manageUserPage.rolesStep.title).toBeVisible();
      await manageUserPage.rolesStep.appointmentManagerCheckbox.check();
      await manageUserPage.rolesStep.availabilityManagerCheckbox.check();
      await manageUserPage.rolesStep.continueButton.click();

      await expect(manageUserPage.summaryStep.title).toBeVisible();

      return await manageUserPage.summaryStep.saveUserRoles();
    })
    .then(async usersPage => {
      await expect(
        usersPage.page.getByRole('cell', { name: newUserName }),
      ).toBeVisible();

      await expect(
        usersPage.page
          .getByRole('row')
          .filter({ has: usersPage.page.getByText(newUserName) })
          .getByText(/Appointment manager | Availability manager/, {
            exact: true,
          }),
      ).toBeVisible();
    });
});

test('The current user creates a new Okta user with some roles', async ({
  externalUserName,
  signInToSite,
}) => {
  await signInToSite()
    .then(sitePage => sitePage.clickManageUsersCard())
    .then(manageUsersPage => manageUsersPage.clickAddUser())
    .then(async manageUserPage => {
      await expect(manageUserPage.emailStep.title).toBeVisible();
      await manageUserPage.emailStep.emailInput.fill(externalUserName);
      await manageUserPage.emailStep.continueButton.click();

      await expect(manageUserPage.namesStep.title).toBeVisible();
      await manageUserPage.namesStep.firstNameInput.fill('Elizabeth');
      await manageUserPage.namesStep.lastNameInput.fill('Kensington-Jones');
      await manageUserPage.namesStep.continueButton.click();

      await expect(manageUserPage.rolesStep.title).toBeVisible();
      await manageUserPage.rolesStep.appointmentManagerCheckbox.check();
      await manageUserPage.rolesStep.availabilityManagerCheckbox.check();
      await manageUserPage.rolesStep.continueButton.click();

      await expect(manageUserPage.summaryStep.title).toBeVisible();
      await expect(manageUserPage.summaryStep.nameSummary).toHaveText(
        'Elizabeth Kensington-Jones',
      );
      await expect(manageUserPage.summaryStep.rolesSummary).toHaveText(
        'Appointment manager, Availability manager',
      );
      await expect(manageUserPage.summaryStep.emailAddressSummary).toHaveText(
        externalUserName,
      );

      return await manageUserPage.summaryStep.saveUserRoles();
    })
    .then(async usersPage => {
      await expect(
        usersPage.page.getByRole('cell', { name: externalUserName }),
      ).toBeVisible();

      await expect(
        usersPage.page
          .getByRole('row')
          .filter({ has: usersPage.page.getByText(externalUserName) })
          .getByText(/Appointment manager | Availability manager/, {
            exact: true,
          }),
      ).toBeVisible();
    });
});

test('The current user tries to create a new user without any roles', async ({
  page,
  newUserName,
  signInToSite,
}) => {
  await signInToSite()
    .then(sitePage => sitePage.clickManageUsersCard())
    .then(manageUsersPage => manageUsersPage.clickAddUser())
    .then(async manageUserPage => {
      await expect(manageUserPage.emailStep.title).toBeVisible();
      await manageUserPage.emailStep.emailInput.fill(newUserName);
      await manageUserPage.emailStep.continueButton.click();

      await expect(manageUserPage.rolesStep.title).toBeVisible();
      await manageUserPage.rolesStep.continueButton.click();

      expect(
        page.getByText('You have not selected any roles for this user'),
      ).toBeVisible();
    });
});

test('The current user creates a new user but enters the email of an existing user, and are able to edit this user', async ({
  signInToSite,
}) => {
  await signInToSite()
    .then(sitePage => sitePage.clickManageUsersCard())
    .then(manageUsersPage => manageUsersPage.clickAddUser())
    .then(async manageUserPage => {
      await expect(manageUserPage.emailStep.title).toBeVisible();
      await manageUserPage.emailStep.emailInput.fill('zzz_test_user_3@nhs.net');
      await manageUserPage.emailStep.continueButton.click();

      await expect(manageUserPage.rolesStep.title).toBeVisible();
      await expect(
        manageUserPage.rolesStep.availabilityManagerCheckbox,
      ).toBeChecked();
    });
});
