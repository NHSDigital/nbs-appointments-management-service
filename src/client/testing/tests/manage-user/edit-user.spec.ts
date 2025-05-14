import { test, expect } from '../../fixtures';
import { NotAuthorizedPage } from '@testing-page-objects';

test('A user edits the roles of another user', async ({
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

      return await usersPage.clickEditUserLink(newUserName);
    })
    .then(async manageUserPage => {
      await expect(manageUserPage.rolesStep.title).toBeVisible();
      await manageUserPage.rolesStep.appointmentManagerCheckbox.check();
      await manageUserPage.rolesStep.availabilityManagerCheckbox.uncheck();
      await manageUserPage.rolesStep.continueButton.click();

      await expect(manageUserPage.summaryStep.title).toBeVisible();
      return await manageUserPage.summaryStep.saveUserRoles();
    })
    .then(async usersPage => {
      await expect(
        usersPage.page
          .getByRole('row')
          .filter({ has: usersPage.page.getByText(newUserName) })
          .getByText(/Appointment manager/, {
            exact: true,
          }),
      ).toBeVisible();
    });
});

test('A user tries to remove all roles from another user', async ({
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

      return await usersPage.clickEditUserLink(newUserName);
    })
    .then(async manageUserPage => {
      await expect(manageUserPage.rolesStep.title).toBeVisible();
      await manageUserPage.rolesStep.appointmentManagerCheckbox.uncheck();
      await manageUserPage.rolesStep.availabilityManagerCheckbox.uncheck();
      await manageUserPage.rolesStep.continueButton.click();

      await expect(
        page.getByText('You have not selected any roles for this user'),
      ).toBeVisible();
    });
});

test('A user tries to edit their own roles', async ({
  page,
  getTestSite,
  signInToSite,
}) => {
  await signInToSite();
  await page.goto(
    `/manage-your-appointments/site/${getTestSite().id}/users/manage?user=zzz_test_user_1@nhs.net`,
  );
  await expect(new NotAuthorizedPage(page).title).toBeVisible();
});
