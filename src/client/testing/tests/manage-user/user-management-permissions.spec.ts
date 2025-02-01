import { test } from '../../fixtures';
import { expect } from '@playwright/test';

import { abc01_id } from '../../fixtures';
import RootPage from '../../page-objects/root';
import OAuthLoginPage from '../../page-objects/oauth';
import SiteSelectionPage from '../../page-objects/site-selection';
import SitePage from '../../page-objects/site';
import UsersPage from '../../page-objects/manage-users/users-page';
import UserManagementPage from '../../page-objects/manage-users/edit-manage-user-roles-page';
import NotAuthorizedPage from '../../page-objects/unauthorized';
import EditManageUserRolesPage from '../../page-objects/manage-users/edit-manage-user-roles-page';
import SiteDetailsPage from '../../page-objects/change-site-details-pages/site-details';
import CreateAvailabilityPage from '../../page-objects/create-availability';
import ViewAvailabilityPage from '../../page-objects/view-availability-appointment-pages/month-view-availability-page';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let usersPage: UsersPage;
let userManagementPage: UserManagementPage;
let notAuthorizedPage: NotAuthorizedPage;
let editManageUserRolesPage: EditManageUserRolesPage;
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
  siteDetailsPage = new SiteDetailsPage(page);
  createAvailabilityPage = new CreateAvailabilityPage(page);
  viewAvailabilityPage = new ViewAvailabilityPage(page);
});

test('A user with the appropriate permission can view other users at a site but not edit them', async ({
  getTestUser,
}) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(getTestUser(2));
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
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
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
  await siteSelectionPage.selectSite('Church Lane Pharmacy');
  await sitePage.userManagementCard.click();
  await expect(usersPage.manageColumn).toBeVisible();

  // Then check it does NOT exist at Robin Lane
  await rootPage.goto();

  await siteSelectionPage.selectSite('Robin Lane Medical Centre');

  await sitePage.userManagementCard.click();
  await expect(usersPage.manageColumn).not.toBeVisible();
});

test('Verify user manager cannot edit or remove self account', async ({
  getTestUser,
}) => {
  const user1 = getTestUser(1);
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(user1);
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.userManagementCard.click();
  await usersPage.verifyLinkNotVisible(user1.Username, 'Edit');
  await usersPage.verifyLinkNotVisible(user1.Username, 'Remove from this site');
});

test('Verify user can only view appointment manager related tiles In app when user is assigned Appointment Manager role.', async ({
  page,
  getTestUser,
}) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.userManagementCard.click();
  await usersPage.assignStaffRolesLink.click();

  const user8 = getTestUser(8);
  await editManageUserRolesPage.emailInput.fill(user8.SubjectId);
  await editManageUserRolesPage.searchUserButton.click();
  await editManageUserRolesPage.selectStaffRole('Appointment manager');
  await editManageUserRolesPage.unselectStaffRole('Availability manager');
  await editManageUserRolesPage.unselectStaffRole('Site details manager');
  await editManageUserRolesPage.unselectStaffRole('User manager');
  await editManageUserRolesPage.confirmAndSaveButton.click();
  await usersPage.verifyUserRoles('Appointment manager', user8.SubjectId);
  await rootPage.logOut();
  await page.waitForURL(
    `**/manage-your-appointments/login?redirectUrl=/site/${abc01_id}/users`,
  );
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(getTestUser(8));
  await expect(siteSelectionPage.title).toBeVisible();
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.verifyTileVisible('ManageAppointment');
  await sitePage.verifyTileVisible('SiteManagement');
  await sitePage.verifyTileNotVisible('UserManagement');
  await sitePage.verifyTileNotVisible('CreateAvailability');
  await sitePage.siteManagementCard.click();
  await siteDetailsPage.verifySitePage('Robin Lane Medical Centre');
  await siteDetailsPage.verifyEditButtonNotVisible();
});

test('Verify user can only view availability manager related tiles In app when user is assigned Availability Manager role.', async ({
  page,
  getTestUser,
}) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.userManagementCard.click();
  await usersPage.assignStaffRolesLink.click();

  const user9 = getTestUser(9);
  await editManageUserRolesPage.emailInput.fill(user9.SubjectId);
  await editManageUserRolesPage.searchUserButton.click();
  await editManageUserRolesPage.unselectStaffRole('Appointment manager');
  await editManageUserRolesPage.selectStaffRole('Availability manager');
  await editManageUserRolesPage.unselectStaffRole('Site details manager');
  await editManageUserRolesPage.unselectStaffRole('User manager');
  await editManageUserRolesPage.confirmAndSaveButton.click();
  await usersPage.verifyUserRoles('Availability manager', user9.SubjectId);
  await rootPage.logOut();
  await page.waitForURL(
    `**/manage-your-appointments/login?redirectUrl=/site/${abc01_id}/users`,
  );
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(getTestUser(9));
  await expect(siteSelectionPage.title).toBeVisible();
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.verifyTileVisible('ManageAppointment');
  await sitePage.verifyTileVisible('SiteManagement');
  await sitePage.verifyTileNotVisible('UserManagement');
  await sitePage.verifyTileVisible('CreateAvailability');
  await sitePage.siteManagementCard.click();
  await siteDetailsPage.verifySitePage('Robin Lane Medical Centre');
  await siteDetailsPage.verifyEditButtonNotVisible();
  await page.goto(`/manage-your-appointments/site/${abc01_id}`);
  await sitePage.createAvailabilityCard.click();
  await createAvailabilityPage.createAvailabilityButton.click();
  await createAvailabilityPage.verifyCreateAvailabilitySessionPageDisplayed();
  await page.goto(`/manage-your-appointments/site/${abc01_id}`);
  await sitePage.viewAvailabilityAndManageAppointmentsCard.click();
  await viewAvailabilityPage.verifyViewMonthDisplayed();
});

test('Verify user can only view user manager related tiles In app when user is assigned user Manager role.', async ({
  page,
  getTestUser,
}) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.userManagementCard.click();
  await usersPage.assignStaffRolesLink.click();

  const user10 = getTestUser(10);
  await editManageUserRolesPage.emailInput.fill(user10.SubjectId);
  await editManageUserRolesPage.searchUserButton.click();
  await editManageUserRolesPage.unselectStaffRole('Appointment manager');
  await editManageUserRolesPage.unselectStaffRole('Availability manager');
  await editManageUserRolesPage.unselectStaffRole('Site details manager');
  await editManageUserRolesPage.selectStaffRole('User manager');
  await editManageUserRolesPage.confirmAndSaveButton.click();
  await usersPage.verifyUserRoles('User manager', user10.SubjectId);
  await rootPage.logOut();
  await page.waitForURL(
    `**/manage-your-appointments/login?redirectUrl=/site/${abc01_id}/users`,
  );
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(getTestUser(10));
  await expect(siteSelectionPage.title).toBeVisible();
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.verifyTileVisible('ManageAppointment');
  await sitePage.verifyTileVisible('SiteManagement');
  await sitePage.verifyTileVisible('UserManagement');
  await sitePage.verifyTileNotVisible('CreateAvailability');
  await sitePage.siteManagementCard.click();
  await siteDetailsPage.verifySitePage('Robin Lane Medical Centre');
  await siteDetailsPage.verifyEditButtonNotVisible();
  await page.goto(`/manage-your-appointments/site/${abc01_id}`);
  await sitePage.viewAvailabilityAndManageAppointmentsCard.click();
  await viewAvailabilityPage.verifyViewMonthDisplayed();
  await page.goto(`/manage-your-appointments/site/${abc01_id}`);
  await sitePage.userManagementCard.click();
  await expect(usersPage.title).toBeVisible();
});

test('Verify user can only view site details manager related tiles In app when user is assigned site details manager role.', async ({
  page,
  getTestUser,
}) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.userManagementCard.click();
  await usersPage.assignStaffRolesLink.click();

  const user11 = getTestUser(11);
  await editManageUserRolesPage.emailInput.fill(user11.SubjectId);
  await editManageUserRolesPage.searchUserButton.click();
  await editManageUserRolesPage.unselectStaffRole('Appointment manager');
  await editManageUserRolesPage.unselectStaffRole('Availability manager');
  await editManageUserRolesPage.selectStaffRole('Site details manager');
  await editManageUserRolesPage.unselectStaffRole('User manager');
  await editManageUserRolesPage.confirmAndSaveButton.click();
  await usersPage.verifyUserRoles('Site details manager', user11.SubjectId);
  await rootPage.logOut();
  await page.waitForURL(
    `**/manage-your-appointments/login?redirectUrl=/site/${abc01_id}/users`,
  );
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(getTestUser(11));
  await expect(siteSelectionPage.title).toBeVisible();
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.verifyTileVisible('ManageAppointment');
  await sitePage.verifyTileVisible('SiteManagement');
  await sitePage.verifyTileNotVisible('UserManagement');
  await sitePage.verifyTileNotVisible('CreateAvailability');
  await sitePage.siteManagementCard.click();
  await siteDetailsPage.verifySitePage('Robin Lane Medical Centre');
  await siteDetailsPage.verifyEditButtonToBeVisible();
  await page.goto(`/manage-your-appointments/site/${abc01_id}`);
  await sitePage.viewAvailabilityAndManageAppointmentsCard.click();
  await viewAvailabilityPage.verifyViewMonthDisplayed();
});
