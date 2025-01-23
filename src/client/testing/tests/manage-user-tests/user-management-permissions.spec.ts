import { test, expect } from '@playwright/test';
import { testuser6_emailId, testuser7_emailId, abc01_id } from '../../fixtures';
import env from '../../testEnvironment';
import RootPage from '../../page-objects/root';
import OAuthLoginPage from '../../page-objects/oauth';
import SiteSelectionPage from '../../page-objects/site-selection';
import SitePage from '../../page-objects/site';
import UsersPage from '../../page-objects/manage-users/users-page';
import UserManagementPage from '../../page-objects/manage-users/edit-manage-user-roles-page';
import NotAuthorizedPage from '../../page-objects/unauthorized';
import EditManageUserRolesPage from '../../page-objects/manage-users/edit-manage-user-roles-page';
import EulaConsentPage from '../../page-objects/eula-consent';
import SiteDetailsPage from '../../page-objects/change-site-details-pages/site-details';
import CreateAvailabilityPage from '../../page-objects/create-availability';
import ViewAvailabilityPage from '../../page-objects/view-availability-appointment-pages/month-view-availability-page';

const { TEST_USERS } = env;

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let usersPage: UsersPage;
let userManagementPage: UserManagementPage;
let notAuthorizedPage: NotAuthorizedPage;
let editManageUserRolesPage: EditManageUserRolesPage;
let eulaConsentPage: EulaConsentPage;
let siteDetailsPage: SiteDetailsPage;
let createAvailabilityPage: CreateAvailabilityPage;
let viewAvailabilityPage: ViewAvailabilityPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  usersPage = new UsersPage(page);
  userManagementPage = new UserManagementPage(page);
  notAuthorizedPage = new NotAuthorizedPage(page);
  editManageUserRolesPage = new EditManageUserRolesPage(page);
  eulaConsentPage = new EulaConsentPage(page);
  siteDetailsPage = new SiteDetailsPage(page);
  createAvailabilityPage = new CreateAvailabilityPage(page);
  viewAvailabilityPage = new ViewAvailabilityPage(page);
});

test('A user with the appropriate permission can view other users at a site but not edit them', async () => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser2);
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.userManagementCard.click();

  await expect(usersPage.title).toBeVisible();
  await expect(usersPage.emailColumn).toBeVisible();

  await expect(usersPage.manageColumn).not.toBeVisible();
  await expect(usersPage.assignStaffRolesLink).not.toBeVisible();
});

test('A user with the appropriate permission can view other users at a site and also edit them', async () => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser1);
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.userManagementCard.click();

  await expect(usersPage.manageColumn).toBeVisible();

  await expect(
    userManagementPage.page.getByRole('row').filter({
      hasNot: userManagementPage.page.getByText(/int-test-user/),
      has: userManagementPage.page.getByRole('link', { name: 'Edit' }),
    }),
  ).toHaveCount(6);
});

test('Navigating straight to the user management page works as expected', async ({
  page,
}) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser1);

  await page.goto(
    '/manage-your-appointments/site/5914b64a-66bb-4ee2-ab8a-94958c1fdfcb/users',
  );
  await expect(usersPage.title).toBeVisible();
});

test('Navigating straight to the user management page displays an appropriate error if the permission is missing', async ({
  page,
}) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser3);

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

test('permissions are applied per site', async () => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser2);

  // First check Edit column exists at Church Lane
  await siteSelectionPage.selectSite('Church Lane Pharmacy');
  await sitePage.userManagementCard.click();
  await expect(usersPage.manageColumn).toBeVisible();

  // Then check it does NOT exist at Robin Lane
  await rootPage.goto();

  await siteSelectionPage.selectSite('Robin Lane Medical Centre');

  await sitePage.userManagementCard.click();
  await expect(usersPage.manageColumn).not.toBeVisible();
});

test('Verify user manager cannot edit or remove self account', async () => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser1);
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.userManagementCard.click();
  await usersPage.verifyLinkNotVisible(TEST_USERS.testUser1.username, 'Edit');
  await usersPage.verifyLinkNotVisible(
    TEST_USERS.testUser1.username,
    'Remove from this site',
  );
});

test('Verify user can only view appointment manager related tiles In app when user is assigned Appointment Manager role.', async ({
  page,
}) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser1);
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.userManagementCard.click();
  await usersPage.assignStaffRolesLink.click();
  await editManageUserRolesPage.emailInput.fill(testuser6_emailId);
  await editManageUserRolesPage.searchUserButton.click();
  await editManageUserRolesPage.selectStaffRole('Appointment manager');
  await editManageUserRolesPage.unselectStaffRole('Availability manager');
  await editManageUserRolesPage.unselectStaffRole('Site details manager');
  await editManageUserRolesPage.unselectStaffRole('User manager');
  await editManageUserRolesPage.confirmAndSaveButton.click();
  await usersPage.verifyUserRoles('Appointment manager', testuser6_emailId);
  await rootPage.logOut();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signInWithRequiredUser(
    TEST_USERS.testUser6.username,
    TEST_USERS.testUser6.password,
  );
  await eulaConsentPage.acceptAndContinueButton.click();
  await page.waitForURL('**/');
  await expect(siteSelectionPage.title).toBeVisible();
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.veriyTileVisible('ManageAppointment');
  await sitePage.veriyTileVisible('SiteManagement');
  await sitePage.veriyTileNotVisible('UserManagement');
  await sitePage.veriyTileNotVisible('CreateAvailability');
  await sitePage.siteManagementCard.click();
  await siteDetailsPage.verifySitepage();
  await siteDetailsPage.verifyEditButtonNotVisible();
});

test('Verify user can only view create availbility related tiles In app when user is assigned Availability Manager role.', async ({
  page,
}) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser1);
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.userManagementCard.click();
  await usersPage.assignStaffRolesLink.click();
  await editManageUserRolesPage.emailInput.fill(testuser7_emailId);
  await editManageUserRolesPage.searchUserButton.click();
  await editManageUserRolesPage.unselectStaffRole('Appointment manager');
  await editManageUserRolesPage.selectStaffRole('Availability manager');
  await editManageUserRolesPage.unselectStaffRole('Site details manager');
  await editManageUserRolesPage.unselectStaffRole('User manager');
  await editManageUserRolesPage.confirmAndSaveButton.click();
  await usersPage.verifyUserRoles('Availability manager', testuser6_emailId);
  await rootPage.logOut();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signInWithRequiredUser(
    TEST_USERS.testUser7.username,
    TEST_USERS.testUser7.password,
  );
  await eulaConsentPage.acceptAndContinueButton.click();
  await page.waitForURL('**/');
  await expect(siteSelectionPage.title).toBeVisible();
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.veriyTileVisible('ManageAppointment');
  await sitePage.veriyTileVisible('SiteManagement');
  await sitePage.veriyTileNotVisible('UserManagement');
  await sitePage.veriyTileVisible('CreateAvailability');
  await sitePage.siteManagementCard.click();
  await siteDetailsPage.verifySitepage();
  await siteDetailsPage.verifyEditButtonNotVisible();
  await page.goto(`/manage-your-appointments/site/${abc01_id}`);
  await sitePage.createAvailabilityCard.click();
  await createAvailabilityPage.createAvailabilityButton.click();
  await createAvailabilityPage.verifyCreateAvailabilitySessionPageDisplayed();
  await page.goto(`/manage-your-appointments/site/${abc01_id}`);
  await sitePage.viewAvailabilityAndManageAppointmentsCard.click();
  await viewAvailabilityPage.verifyViewMonthDisplayed();
});
