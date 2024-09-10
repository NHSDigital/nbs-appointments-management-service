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

test('A user with the appropriate permission can view other users at a site but not edit them', async () => {
  await rootPage.goto();
  await rootPage.logInButton.click();
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
  await rootPage.logInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser1);
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.userManagementCard.click();

  await expect(usersPage.manageColumn).toBeVisible();
  await expect(usersPage.page.getByRole('link', { name: 'Edit' })).toHaveCount(
    5,
  );
});

test('Navigating straight to the user management page works as expected', async ({
  page,
}) => {
  await rootPage.goto();
  await rootPage.logInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser1);

  await page.goto('/site/ABC01/users');
  await expect(usersPage.title).toBeVisible();
});

test('Navigating straight to the user management page displays an appropriate error if the permission is missing', async ({
  page,
}) => {
  await rootPage.goto();
  await rootPage.logInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser3);

  await page.goto('/site/ABC01/users');
  await expect(usersPage.emailColumn).not.toBeVisible();
  await expect(
    page.getByText('Forbidden: You lack the necessary permissions'),
  ).toBeVisible();

  await page.goto('/site/ABC01/users/manage');
  await expect(userManagementPage.emailInput).not.toBeVisible();
  await expect(
    page.getByText('Forbidden: You lack the necessary permissions'),
  ).toBeVisible();
});

test('permissions are applied per site', async ({ page }) => {
  await rootPage.goto();
  await rootPage.logInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser2);

  // First check Edit column exists at Church Lane
  await siteSelectionPage.selectSite('Church Lane Pharmacy');
  await sitePage.userManagementCard.click();
  await expect(usersPage.manageColumn).toBeVisible();

  // Then check it does NOT exist at Robin Lane
  await rootPage.homeBreadcrumb.click();

  await siteSelectionPage.selectSite('Robin Lane Medical Centre');

  await sitePage.userManagementCard.click();
  await expect(usersPage.manageColumn).not.toBeVisible();
});
