import { test } from '../../fixtures';
import RootPage from '../../page-objects/root';
import OAuthLoginPage from '../../page-objects/oauth';
import SiteSelectionPage from '../../page-objects/site-selection';
import SitePage from '../../page-objects/site';
import UsersPage from '../../page-objects/manage-users/users-page';
import EditManageUserRolesPage from '../../page-objects/manage-users/edit-manage-user-roles-page';
import RemoveUserPage from '../../page-objects/manage-users/remove-user-page';
import CreateUserPage from '../../page-objects/manage-users/create-user-page';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let usersPage: UsersPage;
let createUserPage: CreateUserPage;
let editManageUserRolesPage: EditManageUserRolesPage;
let removeUserPage: RemoveUserPage;

test.beforeEach(async ({ page, getTestSite }) => {
  const site = getTestSite();
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  usersPage = new UsersPage(page);
  createUserPage = new CreateUserPage(page);
  editManageUserRolesPage = new EditManageUserRolesPage(page);
  removeUserPage = new RemoveUserPage(page, site);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite(site.name);
  await sitePage.userManagementCard.click();
  await page.waitForURL(`**/site/${site.id}/users`);
});

// TODO: Maybe something like this to clear up all the users created along the way?
// test.afterEach(async ({ page }) => {
//   await cosmosDbSeeder.clearUsers();
// });

test('Verify user manager able to create new user', async ({ newUserName }) => {
  await usersPage.assignStaffRolesLink.click();
  await editManageUserRolesPage.emailInput.fill(newUserName);
  await editManageUserRolesPage.searchUserButton.click();
  await editManageUserRolesPage.selectStaffRole('Appointment manager');
  await editManageUserRolesPage.selectStaffRole('Availability manager');
  await editManageUserRolesPage.confirmAndSaveButton.click();
  await usersPage.userExists(newUserName);
  await usersPage.verifyUserRoles('Appointment manager', newUserName);
  await usersPage.verifyUserRoles('Availability manager', newUserName);
  await usersPage.removeFromThisSiteLink(newUserName);
  await removeUserPage.confirmRemoveButton.click();
});

test('Cannot create a user without any roles', async ({ newUserName }) => {
  await usersPage.assignStaffRolesLink.click();
  await editManageUserRolesPage.emailInput.fill(newUserName);
  await editManageUserRolesPage.searchUserButton.click();
  await editManageUserRolesPage.confirmAndSaveButton.click();
  await createUserPage.notSelectedAnyRolesErrorMsg();
});

test('Verify users are redirected to users page upon cancel button clicked', async ({
  newUserName,
}) => {
  await usersPage.assignStaffRolesLink.click();
  await editManageUserRolesPage.emailInput.fill(newUserName);
  await editManageUserRolesPage.cancelButton.click();
  await usersPage.userDoesNotExist(newUserName);
});

test('Verify users are redirected to edit roles page when emailId already exists', async ({
  newUserName,
}) => {
  await usersPage.assignStaffRolesLink.click();
  await editManageUserRolesPage.emailInput.fill(newUserName);
  await editManageUserRolesPage.searchUserButton.click();
  await editManageUserRolesPage.selectStaffRole('Appointment manager');
  await editManageUserRolesPage.confirmAndSaveButton.click();
  await usersPage.userExists(newUserName);
  await usersPage.assignStaffRolesLink.click();
  await editManageUserRolesPage.emailInput.fill(newUserName);
  await editManageUserRolesPage.searchUserButton.click();
  await editManageUserRolesPage.verifyUserRedirectedToEditRolePage(
    'Appointment manager',
    'Checked',
  );
});
