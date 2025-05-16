import { test, expect } from '../../fixtures';
import {
  ManageUserPage,
  NotAuthorizedPage,
  OAuthLoginPage,
  RootPage,
  SitePage,
  SiteSelectionPage,
  UsersPage,
} from '@testing-page-objects';
import { Site } from '@types';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let usersPage: UsersPage;
let manageUserPage: ManageUserPage;
let notAuthorizedPage: NotAuthorizedPage;

let site: Site;

test.beforeEach(async ({ page, getTestSite }) => {
  site = getTestSite();
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  usersPage = new UsersPage(page);
  manageUserPage = new ManageUserPage(page);
  notAuthorizedPage = new NotAuthorizedPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite(site);
  await sitePage.userManagementCard.click();
  await page.waitForURL(`**/site/${site.id}/users`);
});

test('Verify user manager able to edit user role', async ({
  page,
  getTestSite,
  newUserName,
}) => {
  const newUser = newUserName(test.info());
  // Arrange: Create new user
  // TODO: Use seed data instead!
  await usersPage.addUserButton.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users/manage`);

  await expect(manageUserPage.emailStep.title).toBeVisible();
  await manageUserPage.emailStep.emailInput.fill(newUser);
  await manageUserPage.emailStep.continueButton.click();

  await expect(manageUserPage.rolesStep.title).toBeVisible();
  await manageUserPage.rolesStep.appointmentManagerCheckbox.check();
  await manageUserPage.rolesStep.availabilityManagerCheckbox.check();
  await manageUserPage.rolesStep.continueButton.click();

  await expect(manageUserPage.summaryStep.title).toBeVisible();
  await manageUserPage.summaryStep.continueButton.click();

  await usersPage.userExists(newUser);

  // Act: Edit the new user's roles
  await usersPage.clickEditLink(newUser);

  await expect(manageUserPage.rolesStep.title).toBeVisible();
  await manageUserPage.rolesStep.appointmentManagerCheckbox.check();
  await manageUserPage.rolesStep.availabilityManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.continueButton.click();

  await expect(manageUserPage.summaryStep.title).toBeVisible();
  await manageUserPage.summaryStep.continueButton.click();

  // Assert: Check the new user's roles have changed
  await usersPage.verifyUserRoles('Appointment manager', newUser);
  await usersPage.verifyUserRoleRemoved('Availability manager', newUser);
});

test('Verify all roles cannot be removed from existing account', async ({
  page,
  getTestSite,
  newUserName,
}) => {
  const newUser = newUserName(test.info());
  // Arrange: Create new user
  // TODO: Use seed data instead!
  await usersPage.addUserButton.click();

  await usersPage.addUserButton.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users/manage`);

  await expect(manageUserPage.emailStep.title).toBeVisible();
  await manageUserPage.emailStep.emailInput.fill(newUser);
  await manageUserPage.emailStep.continueButton.click();

  await expect(manageUserPage.rolesStep.title).toBeVisible();
  await manageUserPage.rolesStep.appointmentManagerCheckbox.check();
  await manageUserPage.rolesStep.availabilityManagerCheckbox.check();
  await manageUserPage.rolesStep.continueButton.click();

  await expect(manageUserPage.summaryStep.title).toBeVisible();
  await manageUserPage.summaryStep.continueButton.click();

  await usersPage.userExists(newUser);

  // Act: Edit the new user's roles
  await usersPage.clickEditLink(newUser);

  await expect(manageUserPage.rolesStep.title).toBeVisible();
  await manageUserPage.rolesStep.appointmentManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.availabilityManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.continueButton.click();

  // Assert: Check for a validation message
  // TODO: Shouldn't this be covered by jest tests?
  await expect(
    page.getByText('You have not selected any roles for this user'),
  ).toBeVisible();
});

test('Receives 403 error when trying to edit self', async ({ page }) => {
  await page.goto(
    `/manage-your-appointments/site/${site.id}/users/manage?user=zzz_test_user_1@nhs.net`,
  );
  await expect(notAuthorizedPage.title).toBeVisible();
});
