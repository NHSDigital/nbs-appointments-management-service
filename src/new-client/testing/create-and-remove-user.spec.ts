import { test, expect } from '@playwright/test';
import env from './testEnvironment';
import RootPage from './page-objects/root';
import OAuthLoginPage from './page-objects/oauth';
import SiteSelectionPage from './page-objects/site-selection';
import SitePage from './page-objects/site';
import UsersPage from './page-objects/users';
import UserManagementPage from './page-objects/user-management';

const { TEST_USERS } = env;

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let usersPage: UsersPage;
let userManagementPage: UserManagementPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  usersPage = new UsersPage(page);
  userManagementPage = new UserManagementPage(page);
});

test('Can create a new user', async () => {
  await rootPage.goto();
  await rootPage.logInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser1);
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.userManagementCard.click();
  await usersPage.assignStaffRolesLink.click();

  await userManagementPage.emailInput.fill('zzz_new_test_user_1@nhs.net');
  await userManagementPage.searchUserButton.click();
  await userManagementPage.selectRoles(['Check-in', 'Availability manager']);

  // TODO: write more...
});

// TODO Write more tests for user deletion
