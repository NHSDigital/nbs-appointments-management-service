import { test } from '../../fixtures';
import env from '../../testEnvironment';
import RootPage from '../../page-objects/root';
import OAuthLoginPage from '../../page-objects/oauth';
import SiteSelectionPage from '../../page-objects/site-selection';
import SitePage from '../../page-objects/site';
import UsersPage from '../../page-objects/manage-users/users-page';
import EditManageUserRolesPage from '../../page-objects/manage-users/edit-manage-user-roles-page';
import RemoveUserPage from '../../page-objects/manage-users/remove-user-page';
import NotFoundPage from '../../page-objects/not-found';
import NotAuthorizedPage from '../../page-objects/unauthorized';
import CreateUserPage from '../../page-objects/manage-users/create-user-page';

const { TEST_USERS } = env;

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let usersPage: UsersPage;
let createUserPage: CreateUserPage;
let editManageUserRolesPage: EditManageUserRolesPage;
let removeUserPage: RemoveUserPage;
let notFoundPage: NotFoundPage;
let notAuthorizedPage: NotAuthorizedPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  usersPage = new UsersPage(page);
  createUserPage = new CreateUserPage(page);
  editManageUserRolesPage = new EditManageUserRolesPage(page);
  removeUserPage = new RemoveUserPage(page);
  notFoundPage = new NotFoundPage(page);
  notAuthorizedPage = new NotAuthorizedPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser1);
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.userManagementCard.click();
  await page.waitForURL('**/site/ABC01/users');
});

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

test('Verify user manager is able to remove a user', async ({
  newUserName,
}) => {
  await usersPage.assignStaffRolesLink.click();
  await editManageUserRolesPage.emailInput.fill(newUserName);
  await editManageUserRolesPage.searchUserButton.click();
  await editManageUserRolesPage.selectStaffRole('Appointment manager');
  await editManageUserRolesPage.confirmAndSaveButton.click();
  await usersPage.userExists(newUserName);
  await usersPage.removeFromThisSiteLink(newUserName);
  await removeUserPage.verifyUserNavigatedToRemovePage(newUserName);
  await removeUserPage.clickButton('Remove this account');
  await usersPage.userDoesNotExist(newUserName);
});

test('Displays a notification banner after removing a user, which disappears when Close is clicked', async ({
  newUserName,
}) => {
  await usersPage.assignStaffRolesLink.click();
  await editManageUserRolesPage.emailInput.fill(newUserName);
  await editManageUserRolesPage.searchUserButton.click();
  await editManageUserRolesPage.selectStaffRole('Appointment manager');
  await editManageUserRolesPage.confirmAndSaveButton.click();
  await usersPage.userExists(newUserName);
  await usersPage.removeFromThisSiteLink(newUserName);
  await removeUserPage.verifyUserNavigatedToRemovePage(newUserName);
  await removeUserPage.clickButton('Remove this account');
  await usersPage.verifyRemoveUserSuccessBannerDisplayed(newUserName);
  await usersPage.closeBanner();
  await usersPage.verifyRemoveUserSuccessBannerNotDisplayed(newUserName);
});

// test('Receives 404 when trying to remove an invalid user', async ({ page }) )=> {
//   await page.goto(
//     `/manage-your-appointments/site/ABC01/users/remove?user=not-a-user`,
//   );

//   await expect(notFoundPage.title).toBeVisible();
//   await expect(notFoundPage.notFoundMessageText).toBeVisible();
// });

// test('Receives 403 error when trying to edit self', async ({ page }) => {
//   await page.goto(
//     `/manage-your-appointments/site/ABC01/users/manage?user=zzz_test_user_1@nhs.net`,
//   );

//   await expect(notAuthorizedPage.title).toBeVisible();
// });
