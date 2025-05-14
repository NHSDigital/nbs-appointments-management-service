import { test, expect } from '../../fixtures';
import {
  ManageUserPage,
  NotAuthorizedPage,
  LoginPage,
} from '@testing-page-objects';

let put: ManageUserPage;

test.beforeEach(async ({ page, getTestSite }) => {
  put = await new LoginPage(page)
    .logInWithNhsMail()
    .then(oAuthPage => oAuthPage.signIn())
    .then(siteSelectionPage => siteSelectionPage.selectSite(getTestSite()))
    .then(sitePage => sitePage.clickManageUsersCard())
    .then(manageUsersPage => manageUsersPage.clickAddUser());
});

test('A user edits the roles of another user', async ({ newUserName }) => {
  // Arrange: Create new user
  // TODO: Use seed data instead!
  await expect(put.emailStep.title).toBeVisible();
  await put.emailStep.emailInput.fill(newUserName);
  await put.emailStep.continueButton.click();

  await expect(put.rolesStep.title).toBeVisible();
  await put.rolesStep.appointmentManagerCheckbox.check();
  await put.rolesStep.availabilityManagerCheckbox.check();
  await put.rolesStep.continueButton.click();

  await expect(put.summaryStep.title).toBeVisible();
  const usersPage = await put.summaryStep.saveUserRoles();

  await expect(
    usersPage.page.getByRole('cell', { name: newUserName }),
  ).toBeVisible();

  // Act: Edit the new user's roles
  put = await usersPage.clickEditUserLink(newUserName);

  await expect(put.rolesStep.title).toBeVisible();
  await put.rolesStep.appointmentManagerCheckbox.check();
  await put.rolesStep.availabilityManagerCheckbox.uncheck();
  await put.rolesStep.continueButton.click();

  await expect(put.summaryStep.title).toBeVisible();
  await put.summaryStep.continueButton.click();

  // Assert: Check the new user's roles have changed
  await expect(
    usersPage.page
      .getByRole('row')
      .filter({ has: usersPage.page.getByText(newUserName) })
      .getByText('Appointment manager | Availability manager', { exact: true }),
  ).toBeVisible();
});

test('A user tries to remove all roles from another user', async ({
  page,
  newUserName,
}) => {
  // Arrange: Create new user
  // TODO: Use seed data instead!

  await expect(put.emailStep.title).toBeVisible();
  await put.emailStep.emailInput.fill(newUserName);
  await put.emailStep.continueButton.click();

  await expect(put.rolesStep.title).toBeVisible();
  await put.rolesStep.appointmentManagerCheckbox.check();
  await put.rolesStep.availabilityManagerCheckbox.check();
  await put.rolesStep.continueButton.click();

  await expect(put.summaryStep.title).toBeVisible();
  const usersPage = await put.summaryStep.saveUserRoles();

  await expect(
    usersPage.page.getByRole('cell', { name: newUserName }),
  ).toBeVisible();

  // Act: Edit the new user's roles
  put = await usersPage.clickEditUserLink(newUserName);

  await expect(put.rolesStep.title).toBeVisible();
  await put.rolesStep.appointmentManagerCheckbox.uncheck();
  await put.rolesStep.availabilityManagerCheckbox.uncheck();
  await put.rolesStep.continueButton.click();

  // Assert: Check for a validation message
  // TODO: Shouldn't this be covered by jest tests?
  await expect(
    page.getByText('You have not selected any roles for this user'),
  ).toBeVisible();
});

test('A user tries to edit their own roles', async ({ page, getTestSite }) => {
  await page.goto(
    `/manage-your-appointments/site/${getTestSite().id}/users/manage?user=zzz_test_user_1@nhs.net`,
  );
  await expect(new NotAuthorizedPage(page).title).toBeVisible();
});
