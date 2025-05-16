import { test, expect } from '../../fixtures';
import {
  CreateAvailabilityPage,
  ManageUserPage,
  MonthViewAvailabilityPage,
  NotAuthorizedPage,
  OAuthLoginPage,
  RootPage,
  SiteDetailsPage,
  SitePage,
  SiteSelectionPage,
  UserManagementPage,
  UsersPage,
} from '@testing-page-objects';
import { Site } from '@types';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let usersPage: UsersPage;
let userManagementPage: UserManagementPage;
let notAuthorizedPage: NotAuthorizedPage;
let manageUserPage: ManageUserPage;
let siteDetailsPage1: SiteDetailsPage;
let createAvailabilityPage: CreateAvailabilityPage;
let viewMonthAvailabilityPage: MonthViewAvailabilityPage;

let site1: Site;
let site2: Site;

test.beforeEach(async ({ page, getTestSite }) => {
  site1 = getTestSite(1);
  site2 = getTestSite(2);
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  usersPage = new UsersPage(page);
  userManagementPage = new UserManagementPage(page);
  notAuthorizedPage = new NotAuthorizedPage(page);
  manageUserPage = new ManageUserPage(page);
  siteDetailsPage1 = new SiteDetailsPage(page, site1);
  createAvailabilityPage = new CreateAvailabilityPage(page);
  viewMonthAvailabilityPage = new MonthViewAvailabilityPage(page);
});

test('A user with the appropriate permission can view other users at a site and also edit them', async ({
  getTestSite,
}) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite(getTestSite());
  await sitePage.userManagementCard.click();
  await expect(usersPage.manageColumn).toBeVisible();
  const userCount = await userManagementPage.page
    .getByRole('row')
    .filter({
      hasNot: userManagementPage.page.getByText(/int-test-user/),
      has: userManagementPage.page.getByRole('link', { name: 'Edit' }),
    })
    .count();
  await expect(userCount).toBeGreaterThan(0);
});

test('Navigating straight to the user management page works as expected', async ({
  page,
}) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await page.goto(
    '/manage-your-appointments/site/5914b64a-66bb-4ee2-ab8a-94958c1fdfcb/users',
  );
  await expect(usersPage.title).toBeVisible();
});

test('Navigating straight to the user management page displays an appropriate error if the permission is missing', async ({
  page,
  getTestUser,
}) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(getTestUser(3));
  await page.goto(
    '/manage-your-appointments/site/5914b64a-66bb-4ee2-ab8a-94958c1fdfcb/users',
  );
  await expect(usersPage.emailColumn).not.toBeVisible();
  await expect(notAuthorizedPage.title).toBeVisible();
  await page.goto(
    '/manage-your-appointments/site/5914b64a-66bb-4ee2-ab8a-94958c1fdfcb/users/manage',
  );
  await expect(userManagementPage.emailInput).not.toBeVisible();
  await expect(notAuthorizedPage.title).toBeVisible();
});

test('permissions are applied per site', async ({ getTestUser }) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(getTestUser(2));

  // First check Edit column exists at Church Lane
  await siteSelectionPage.selectSite(site2);
  await sitePage.userManagementCard.click();
  await expect(usersPage.manageColumn).toBeVisible();

  // Then check it does NOT exist at Robin Lane
  await rootPage.goto();

  await siteSelectionPage.selectSite(site1);

  await sitePage.siteManagementCard.click();
  await expect(siteDetailsPage1.editSiteDetailsButton).not.toBeVisible();
});

test('Verify user manager cannot edit or remove self account', async ({
  getTestUser,
  getTestSite,
}) => {
  const user1 = getTestUser(1);
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(user1);
  await siteSelectionPage.selectSite(getTestSite());
  await sitePage.userManagementCard.click();
  await usersPage.verifyLinkNotVisible(user1.username, 'Edit');
  await usersPage.verifyLinkNotVisible(user1.username, 'Remove from this site');
});

test('Verify user can only view appointment manager related tiles In app when user is assigned Appointment Manager role.', async ({
  page,
  getTestSite,
  getTestUser,
}) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite(getTestSite());

  await sitePage.userManagementCard.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users`);

  await usersPage.addUserButton.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users/manage`);

  const user8 = getTestUser(8);
  await expect(manageUserPage.emailStep.title).toBeVisible();
  await manageUserPage.emailStep.emailInput.fill(user8.subjectId);
  await manageUserPage.emailStep.continueButton.click();

  await expect(manageUserPage.rolesStep.title).toBeVisible();
  await manageUserPage.rolesStep.appointmentManagerCheckbox.check();
  await manageUserPage.rolesStep.availabilityManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.siteDetailsManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.userManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.continueButton.click();

  await expect(manageUserPage.summaryStep.title).toBeVisible();
  await manageUserPage.summaryStep.continueButton.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users`);

  await usersPage.verifyUserRoles('Appointment manager', user8.subjectId);
  await rootPage.logOut();
  await page.waitForURL(`**/manage-your-appointments/login`);
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(getTestUser(8));
  await expect(siteSelectionPage.title).toBeVisible();
  await siteSelectionPage.selectSite(site1);
  await sitePage.verifyTileVisible('ManageAppointment');
  await sitePage.verifyTileVisible('SiteManagement');
  await sitePage.verifyTileNotVisible('UserManagement');
  await sitePage.verifyTileNotVisible('CreateAvailability');
  await sitePage.siteManagementCard.click();
  await page.waitForURL(`**/site/${getTestSite().id}/details`);
  await siteDetailsPage1.verifySitePage();
  await siteDetailsPage1.verifyEditButtonNotVisible();
});

test('Verify user can only view availability manager related tiles In app when user is assigned Availability Manager role.', async ({
  page,
  getTestSite,
  getTestUser,
}) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite(getTestSite());

  await sitePage.userManagementCard.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users`);

  await usersPage.addUserButton.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users/manage`);

  const user9 = getTestUser(9);
  await expect(manageUserPage.emailStep.title).toBeVisible();
  await manageUserPage.emailStep.emailInput.fill(user9.subjectId);
  await manageUserPage.emailStep.continueButton.click();

  await expect(manageUserPage.rolesStep.title).toBeVisible();
  await manageUserPage.rolesStep.appointmentManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.availabilityManagerCheckbox.check();
  await manageUserPage.rolesStep.siteDetailsManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.userManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.continueButton.click();

  await expect(manageUserPage.summaryStep.title).toBeVisible();
  await manageUserPage.summaryStep.continueButton.click();

  await usersPage.verifyUserRoles('Availability manager', user9.subjectId);
  await rootPage.logOut();
  await page.waitForURL(`**/manage-your-appointments/login`);
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(getTestUser(9));
  await expect(siteSelectionPage.title).toBeVisible();
  await siteSelectionPage.selectSite(site1);
  await sitePage.verifyTileVisible('ManageAppointment');
  await sitePage.verifyTileVisible('SiteManagement');
  await sitePage.verifyTileNotVisible('UserManagement');
  await sitePage.verifyTileVisible('CreateAvailability');
  await sitePage.siteManagementCard.click();
  await siteDetailsPage1.verifySitePage();
  await siteDetailsPage1.verifyEditButtonNotVisible();
  await page.goto(`/manage-your-appointments/site/${site1.id}`);
  await sitePage.createAvailabilityCard.click();
  await createAvailabilityPage.createAvailabilityButton.click();
  await page.waitForURL(
    `/manage-your-appointments/site/${site1.id}/create-availability/wizard`,
  );
  await createAvailabilityPage.verifyCreateAvailabilitySessionPageDisplayed();
  await page.goto(`/manage-your-appointments/site/${site1.id}`);
  await sitePage.viewAvailabilityAndManageAppointmentsCard.click();
  await viewMonthAvailabilityPage.verifyViewNextMonthButtonDisplayed();
});

test('Verify user can only view user manager related tiles In app when user is assigned user Manager role.', async ({
  page,
  getTestSite,
  getTestUser,
}) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite(getTestSite());

  await sitePage.userManagementCard.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users`);

  await usersPage.addUserButton.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users/manage`);

  const user10 = getTestUser(10);
  await expect(manageUserPage.emailStep.title).toBeVisible();
  await manageUserPage.emailStep.emailInput.fill(user10.subjectId);
  await manageUserPage.emailStep.continueButton.click();

  await expect(manageUserPage.rolesStep.title).toBeVisible();
  await manageUserPage.rolesStep.appointmentManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.availabilityManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.siteDetailsManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.userManagerCheckbox.check();
  await manageUserPage.rolesStep.continueButton.click();

  await expect(manageUserPage.summaryStep.title).toBeVisible();
  await manageUserPage.summaryStep.continueButton.click();

  await usersPage.verifyUserRoles('User manager', user10.subjectId);
  await rootPage.logOut();
  await page.waitForURL(`**/manage-your-appointments/login`);
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(getTestUser(10));
  await expect(siteSelectionPage.title).toBeVisible();
  await siteSelectionPage.selectSite(site1);
  await sitePage.verifyTileVisible('ManageAppointment');
  await sitePage.verifyTileVisible('SiteManagement');
  await sitePage.verifyTileVisible('UserManagement');
  await sitePage.verifyTileNotVisible('CreateAvailability');
  await sitePage.siteManagementCard.click();
  await siteDetailsPage1.verifySitePage();
  await siteDetailsPage1.verifyEditButtonNotVisible();
  await page.goto(`/manage-your-appointments/site/${site1.id}`);
  await sitePage.viewAvailabilityAndManageAppointmentsCard.click();
  await viewMonthAvailabilityPage.verifyViewNextMonthButtonDisplayed();
  await page.goto(`/manage-your-appointments/site/${site1.id}`);
  await sitePage.userManagementCard.click();
  await expect(usersPage.title).toBeVisible();
});

test('Verify user can only view site details manager related tiles In app when user is assigned site details manager role.', async ({
  page,
  getTestSite,
  getTestUser,
}) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite(site1);

  await sitePage.userManagementCard.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users`);

  await usersPage.addUserButton.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users/manage`);

  const user11 = getTestUser(11);
  await expect(manageUserPage.emailStep.title).toBeVisible();
  await manageUserPage.emailStep.emailInput.fill(user11.subjectId);
  await manageUserPage.emailStep.continueButton.click();

  await expect(manageUserPage.rolesStep.title).toBeVisible();
  await manageUserPage.rolesStep.appointmentManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.availabilityManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.siteDetailsManagerCheckbox.check();
  await manageUserPage.rolesStep.userManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.continueButton.click();

  await expect(manageUserPage.summaryStep.title).toBeVisible();
  await manageUserPage.summaryStep.continueButton.click();

  await usersPage.verifyUserRoles('Site details manager', user11.subjectId);
  await rootPage.logOut();
  await page.waitForURL(`**/manage-your-appointments/login`);
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(getTestUser(11));
  await expect(siteSelectionPage.title).toBeVisible();
  await siteSelectionPage.selectSite(site1);
  await sitePage.verifyTileVisible('ManageAppointment');
  await sitePage.verifyTileVisible('SiteManagement');
  await sitePage.verifyTileNotVisible('UserManagement');
  await sitePage.verifyTileNotVisible('CreateAvailability');
  await sitePage.siteManagementCard.click();
  await siteDetailsPage1.verifySitePage();
  await siteDetailsPage1.verifyEditButtonToBeVisible();
  await page.goto(`/manage-your-appointments/site/${site1.id}`);
  await sitePage.viewAvailabilityAndManageAppointmentsCard.click();
  await viewMonthAvailabilityPage.verifyViewNextMonthButtonDisplayed();
});
