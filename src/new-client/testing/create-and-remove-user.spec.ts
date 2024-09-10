import { test, expect } from './fixtures';
import env from './testEnvironment';
import RootPage from './page-objects/root';
import OAuthLoginPage from './page-objects/oauth';
import SiteSelectionPage from './page-objects/site-selection';
import SitePage from './page-objects/site';
import UsersPage from './page-objects/users';
import UserManagementPage from './page-objects/user-management';
import ConfirmRemoveUserPage from './page-objects/confirm-remove-user';

const { TEST_USERS } = env;

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let usersPage: UsersPage;
let userManagementPage: UserManagementPage;
let confirmRemoveUserPage: ConfirmRemoveUserPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  usersPage = new UsersPage(page);
  userManagementPage = new UserManagementPage(page);
  confirmRemoveUserPage = new ConfirmRemoveUserPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser1);
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.userManagementCard.click();

  await page.waitForURL('**/site/1000/users');
});

// TODO: Maybe something like this to clear up all the users created along the way?
// test.afterEach(async ({ page }) => {
//   await cosmosDbSeeder.clearUsers();
// });

test('Can create a user', async ({ newUserName }) => {
  await usersPage.assignStaffRolesLink.click();

  await userManagementPage.emailInput.fill(newUserName);
  await userManagementPage.searchUserButton.click();

  await userManagementPage.selectRole('Check-in');
  await userManagementPage.selectRole('Availability manager');

  await userManagementPage.confirmAndSaveButton.click();

  await userManagementPage.userExists(newUserName);

  await expect(
    userManagementPage.page
      .getByRole('row')
      .filter({
        has: userManagementPage.page.getByText(newUserName),
      })
      .getByText(/Check-in/),
  ).toBeVisible();

  await expect(
    userManagementPage.page
      .getByRole('row')
      .filter({
        has: userManagementPage.page.getByText(newUserName),
      })
      .getByText(/Availability manager/),
  ).toBeVisible();
});

test('Cannot create a user without any roles', async ({ newUserName }) => {
  await usersPage.assignStaffRolesLink.click();

  await userManagementPage.emailInput.fill(newUserName);
  await userManagementPage.searchUserButton.click();

  await userManagementPage.confirmAndSaveButton.click();
  await expect(
    userManagementPage.page.getByText('You have not selected any roles'),
  ).toBeVisible();
  await userManagementPage.selectRole('Check-in');
  await userManagementPage.selectRole('Availability manager');

  await userManagementPage.confirmAndSaveButton.click();
  await userManagementPage.page.waitForURL('**/site/1000/users');
  await userManagementPage.userExists(newUserName);
});

test('Can remove a user', async ({ newUserName }) => {
  await usersPage.assignStaffRolesLink.click();

  await userManagementPage.emailInput.fill(newUserName);
  await userManagementPage.searchUserButton.click();
  await userManagementPage.selectRole('Check-in');
  await userManagementPage.selectRole('Availability manager');

  await userManagementPage.confirmAndSaveButton.click();
  await userManagementPage.userExists(newUserName);

  await userManagementPage.page
    .getByRole('row')
    .filter({
      has: userManagementPage.page.getByText(newUserName),
    })
    .getByRole('link', { name: 'Remove from this site' })
    .click();

  await expect(
    confirmRemoveUserPage.page.getByText(
      `Are you sure you wish to remove ${newUserName} from Robin Lane Medical Centre?`,
    ),
  ).toBeVisible();

  await confirmRemoveUserPage.confirmRemoveButton.click();
  await confirmRemoveUserPage.page.waitForURL('**/site/1000/users');

  await userManagementPage.userDoesNotExist(newUserName);
});
