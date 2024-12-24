import { test, expect } from './fixtures';
import env from './testEnvironment';
import RootPage from './page-objects/root';
import OAuthLoginPage from './page-objects/oauth';
import SiteSelectionPage from './page-objects/site-selection';
import SitePage from './page-objects/site';
import UsersPage from './page-objects/users';
import UserManagementPage from './page-objects/user-management';
import ConfirmRemoveUserPage from './page-objects/confirm-remove-user';
import NotFoundPage from './page-objects/not-found';
import NotAuthorizedPage from './page-objects/unauthorized';

const { TEST_USERS } = env;

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let usersPage: UsersPage;
let userManagementPage: UserManagementPage;
let confirmRemoveUserPage: ConfirmRemoveUserPage;
let notFoundPage: NotFoundPage;
let notAuthorizedPage: NotAuthorizedPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  usersPage = new UsersPage(page);
  userManagementPage = new UserManagementPage(page);
  confirmRemoveUserPage = new ConfirmRemoveUserPage(page);
  notFoundPage = new NotFoundPage(page);
  notAuthorizedPage = new NotAuthorizedPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser1);
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.userManagementCard.click();

  await page.waitForURL('**/site/ABC01/users');
});

// TODO: Maybe something like this to clear up all the users created along the way?
// test.afterEach(async ({ page }) => {
//   await cosmosDbSeeder.clearUsers();
// });

test('Can create a user', async ({ newUserName }) => {
  await usersPage.assignStaffRolesLink.click();

  await userManagementPage.emailInput.fill(newUserName);
  await userManagementPage.searchUserButton.click();

  await userManagementPage.selectRole('Appointment manager');
  await userManagementPage.selectRole('Availability manager');

  await userManagementPage.confirmAndSaveButton.click();

  await userManagementPage.userExists(newUserName);

  await expect(
    userManagementPage.page
      .getByRole('row')
      .filter({
        has: userManagementPage.page.getByText(newUserName),
      })
      .getByText(/Appointment manager/),
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
  await userManagementPage.selectRole('Appointment manager');
  await userManagementPage.selectRole('Availability manager');

  await userManagementPage.confirmAndSaveButton.click();
  await userManagementPage.page.waitForURL('**/site/ABC01/users');
  await userManagementPage.userExists(newUserName);
});

test('Can remove a user', async ({ newUserName }) => {
  await usersPage.assignStaffRolesLink.click();

  await userManagementPage.emailInput.fill(newUserName);
  await userManagementPage.searchUserButton.click();
  await userManagementPage.selectRole('Appointment manager');
  await userManagementPage.selectRole('Availability manager');

  await userManagementPage.confirmAndSaveButton.click();
  await userManagementPage.page.waitForURL('**/site/ABC01/users');
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
  await confirmRemoveUserPage.page.waitForURL('**/site/ABC01/users');

  await userManagementPage.userDoesNotExist(newUserName);
});

test('Displays a notification banner after removing a user, which disappears when Close is clicked', async ({
  newUserName,
}) => {
  await usersPage.assignStaffRolesLink.click();

  await userManagementPage.emailInput.fill(newUserName);
  await userManagementPage.searchUserButton.click();
  await userManagementPage.selectRole('Appointment manager');
  await userManagementPage.confirmAndSaveButton.click();
  await userManagementPage.page.waitForURL('**/site/ABC01/users');

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
  await confirmRemoveUserPage.page.waitForURL('**/site/ABC01/users');

  await userManagementPage.userDoesNotExist(newUserName);
});

test('Receives 403 error when trying to remove self', async ({ page }) => {
  await page.goto(
    `/manage-your-appointments/site/ABC01/users/remove?user=zzz_test_user_1@nhs.net`,
  );

  await expect(notAuthorizedPage.title).toBeVisible();
});

test('Receives 404 when trying to remove an invalid user', async ({ page }) => {
  await page.goto(
    `/manage-your-appointments/site/ABC01/users/remove?user=not-a-user`,
  );

  await expect(notFoundPage.title).toBeVisible();
  await expect(notFoundPage.notFoundMessageText).toBeVisible();
});

test('Receives 403 error when trying to edit self', async ({ page }) => {
  await page.goto(
    `/manage-your-appointments/site/ABC01/users/manage?user=zzz_test_user_1@nhs.net`,
  );

  await expect(notAuthorizedPage.title).toBeVisible();
});
