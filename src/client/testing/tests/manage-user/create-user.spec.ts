import { test, expect } from '../../fixtures';

import {
  ManageUserPage,
  OAuthLoginPage,
  RootPage,
  SitePage,
  SiteSelectionPage,
  UsersPage,
} from '@testing-page-objects';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let usersPage: UsersPage;
let manageUserPage: ManageUserPage;

test.beforeEach(async ({ page, getTestSite }) => {
  const site = getTestSite();
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  usersPage = new UsersPage(page);
  manageUserPage = new ManageUserPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite(site.name);
  await sitePage.userManagementCard.click();
  await page.waitForURL(`**/site/${site.id}/users`);
});

test('The current user creates a new NHSMail user with some roles', async ({
  page,
  getTestSite,
  newUserName,
}) => {
  await usersPage.addUserButton.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users/manage`);

  await expect(manageUserPage.emailStep.title).toBeVisible();
  await manageUserPage.emailStep.emailInput.fill(newUserName);
  await manageUserPage.emailStep.continueButton.click();

  await expect(manageUserPage.rolesStep.title).toBeVisible();
  await manageUserPage.rolesStep.appointmentManagerCheckbox.check();
  await manageUserPage.rolesStep.continueButton.click();

  await expect(manageUserPage.summaryStep.title).toBeVisible();
  await manageUserPage.summaryStep.continueButton.click();

  await usersPage.userExists(newUserName);
  await usersPage.verifyUserRoles('Appointment manager', newUserName);
  await usersPage.verifyUserRoles('Availability manager', newUserName);
});

test('The current user creates a new Okta user with some roles', async ({
  page,
  getTestSite,
  externalUserName,
}) => {
  await usersPage.addUserButton.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users/manage`);

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
    'Appointment Manager, Availability Manager',
  );
  await expect(manageUserPage.summaryStep.emailAddressSummary).toHaveText(
    externalUserName,
  );
  await manageUserPage.summaryStep.continueButton.click();

  await usersPage.userExists(externalUserName);
  await usersPage.verifyUserRoles('Appointment manager', externalUserName);
  await usersPage.verifyUserRoles('Availability manager', externalUserName);
});

test('The current user tries to create a new user without any roles', async ({
  page,
  getTestSite,
  newUserName,
}) => {
  await usersPage.addUserButton.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users/manage`);

  await expect(manageUserPage.emailStep.title).toBeVisible();
  await manageUserPage.emailStep.emailInput.fill(newUserName);
  await manageUserPage.emailStep.continueButton.click();

  await expect(manageUserPage.rolesStep.title).toBeVisible();
  await manageUserPage.rolesStep.appointmentManagerCheckbox.check();
  await manageUserPage.rolesStep.continueButton.click();

  expect(
    page.getByText('You have not selected any roles for this user'),
  ).toBeVisible();
});

test('The current user creates a new user but enters the email of an existing user, and are able to edit this user', async ({
  page,
  getTestSite,
}) => {
  await usersPage.addUserButton.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users/manage`);

  await expect(manageUserPage.emailStep.title).toBeVisible();
  await manageUserPage.emailStep.emailInput.fill('zzz_test_user_3@nhs.net');
  await manageUserPage.emailStep.continueButton.click();

  await expect(manageUserPage.rolesStep.title).toBeVisible();
  await expect(
    manageUserPage.rolesStep.availabilityManagerCheckbox,
  ).toBeChecked();
});
