import { test, expect } from '../../fixtures';
import RootPage from '../../page-objects/root';
import OAuthLoginPage from '../../page-objects/oauth';
import SiteSelectionPage from '../../page-objects/site-selection';
import SitePage from '../../page-objects/site';
import UsersPage from '../../page-objects/manage-users/users-page';
import EditManageUserRolesPage from '../../page-objects/manage-users/edit-manage-user-roles-page';
import NotAuthorizedPage from '../../page-objects/unauthorized';
import { Site } from '@types';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let usersPage: UsersPage;
let editManageUserRolesPage: EditManageUserRolesPage;
let notAuthorizedPage: NotAuthorizedPage;

let site: Site;

test.beforeEach(async ({ page, getTestSite }) => {
  site = getTestSite();
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  usersPage = new UsersPage(page);
  editManageUserRolesPage = new EditManageUserRolesPage(page);
  notAuthorizedPage = new NotAuthorizedPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite(site.name);
  await sitePage.userManagementCard.click();
  await page.waitForURL(`**/site/${site.id}/users`);
});

test('Verify user manager able to edit user role', async ({ newUserName }) => {
  await usersPage.assignStaffRolesLink.click();
  await editManageUserRolesPage.emailInput.fill(newUserName);
  await editManageUserRolesPage.searchUserButton.click();
  await editManageUserRolesPage.selectStaffRole('Appointment manager');
  await editManageUserRolesPage.selectStaffRole('Availability manager');
  await editManageUserRolesPage.confirmAndSaveButton.click();
  await usersPage.userExists(newUserName);
  await usersPage.clickEditLink(newUserName);
  await editManageUserRolesPage.unselectStaffRole('Availability manager');
  await editManageUserRolesPage.confirmAndSaveButton.click();
  await usersPage.verifyUserRoles('Appointment manager', newUserName);
  await usersPage.verifyUserRoleRemoved('Availability manager', newUserName);
});

test('Verify all roles cannot be removed from existing account', async ({
  newUserName,
}) => {
  await usersPage.assignStaffRolesLink.click();
  await editManageUserRolesPage.emailInput.fill(newUserName);
  await editManageUserRolesPage.searchUserButton.click();
  await editManageUserRolesPage.selectStaffRole('Appointment manager');
  await editManageUserRolesPage.selectStaffRole('Availability manager');
  await editManageUserRolesPage.confirmAndSaveButton.click();
  await usersPage.userExists(newUserName);
  await usersPage.clickEditLink(newUserName);
  await editManageUserRolesPage.unselectStaffRole('Appointment manager');
  await editManageUserRolesPage.unselectStaffRole('Availability manager');
  await editManageUserRolesPage.confirmAndSaveButton.click();
  await editManageUserRolesPage.verifyValidationMsgForNoRoles();
});

test('Verify users are redirected to users page upon cancel button clicked on edit user page', async ({
  newUserName,
}) => {
  await usersPage.assignStaffRolesLink.click();
  await editManageUserRolesPage.emailInput.fill(newUserName);
  await editManageUserRolesPage.searchUserButton.click();
  await editManageUserRolesPage.selectStaffRole('Appointment manager');
  await editManageUserRolesPage.selectStaffRole('Availability manager');
  await editManageUserRolesPage.confirmAndSaveButton.click();
  await usersPage.clickEditLink(newUserName);
  await editManageUserRolesPage.unselectStaffRole('Appointment manager');
  await editManageUserRolesPage.cancelButton.click();
  await usersPage.userExists(newUserName);
  await usersPage.verifyUserRoles('Appointment manager', newUserName);
  await usersPage.verifyUserRoles('Availability manager', newUserName);
});

test('Receives 403 error when trying to edit self', async ({ page }) => {
  await page.goto(
    `/manage-your-appointments/site/${site.id}/users/manage?user=zzz_test_user_1@nhs.net`,
  );
  await expect(notAuthorizedPage.title).toBeVisible();
});
