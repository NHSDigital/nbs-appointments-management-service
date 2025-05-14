import { test, expect } from '../../fixtures';
import { ManageUserPage, LoginPage } from '@testing-page-objects';

let put: ManageUserPage;

test.beforeEach(async ({ page, getTestSite }) => {
  put = await new LoginPage(page)
    .logInWithNhsMail()
    .then(oAuthPage => oAuthPage.signIn())
    .then(siteSelectionPage => siteSelectionPage.selectSite(getTestSite()))
    .then(sitePage => sitePage.clickManageUsersCard())
    .then(manageUsersPage => manageUsersPage.clickAddUser());
});

test('The current user creates a new NHSMail user with some roles', async ({
  newUserName,
}) => {
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
  await expect(
    usersPage.page
      .getByRole('row')
      .filter({ has: usersPage.page.getByText(newUserName) })
      .getByText('Appointment manager | Availability manager', { exact: true }),
  ).toBeVisible();
});

test('The current user creates a new Okta user with some roles', async ({
  externalUserName,
}) => {
  await expect(put.emailStep.title).toBeVisible();
  await put.emailStep.emailInput.fill(externalUserName);
  await put.emailStep.continueButton.click();

  await expect(put.namesStep.title).toBeVisible();
  await put.namesStep.firstNameInput.fill('Elizabeth');
  await put.namesStep.lastNameInput.fill('Kensington-Jones');
  await put.namesStep.continueButton.click();

  await expect(put.rolesStep.title).toBeVisible();
  await put.rolesStep.appointmentManagerCheckbox.check();
  await put.rolesStep.availabilityManagerCheckbox.check();
  await put.rolesStep.continueButton.click();

  await expect(put.summaryStep.title).toBeVisible();
  await expect(put.summaryStep.nameSummary).toHaveText(
    'Elizabeth Kensington-Jones',
  );
  await expect(put.summaryStep.rolesSummary).toHaveText(
    'Appointment manager, Availability manager',
  );
  await expect(put.summaryStep.emailAddressSummary).toHaveText(
    externalUserName,
  );
  const usersPage = await put.summaryStep.saveUserRoles();

  await expect(
    usersPage.page.getByRole('cell', { name: externalUserName }),
  ).toBeVisible();
  await expect(
    usersPage.page
      .getByRole('row')
      .filter({ has: usersPage.page.getByText(externalUserName) })
      .getByText('Appointment manager | Availability manager', { exact: true }),
  ).toBeVisible();
});

test('The current user tries to create a new user without any roles', async ({
  page,
  newUserName,
}) => {
  await expect(put.emailStep.title).toBeVisible();
  await put.emailStep.emailInput.fill(newUserName);
  await put.emailStep.continueButton.click();

  await expect(put.rolesStep.title).toBeVisible();
  await put.rolesStep.continueButton.click();

  expect(
    page.getByText('You have not selected any roles for this user'),
  ).toBeVisible();
});

test('The current user creates a new user but enters the email of an existing user, and are able to edit this user', async () => {
  await expect(put.emailStep.title).toBeVisible();
  await put.emailStep.emailInput.fill('zzz_test_user_3@nhs.net');
  await put.emailStep.continueButton.click();

  await expect(put.rolesStep.title).toBeVisible();
  await expect(put.rolesStep.availabilityManagerCheckbox).toBeChecked();
});
