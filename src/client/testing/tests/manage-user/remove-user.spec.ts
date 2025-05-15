import { test, expect } from '../../fixtures';
import {
  ManageUserPage,
  NotFoundPage,
  OAuthLoginPage,
  RemoveUserPage,
  RootPage,
  SitePage,
  SiteSelectionPage,
  UsersPage,
} from '@testing-page-objects';
import { Site } from '@types';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let usersPage: UsersPage;
let manageUserPage: ManageUserPage;
let removeUserPage: RemoveUserPage;
let notFoundPage: NotFoundPage;

let site: Site;

test.beforeEach(async ({ page, getTestSite }) => {
  site = getTestSite();
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  usersPage = new UsersPage(page);
  manageUserPage = new ManageUserPage(page);
  removeUserPage = new RemoveUserPage(page, site);
  notFoundPage = new NotFoundPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite(getTestSite());
  await sitePage.userManagementCard.click();
  await page.waitForURL(`**/site/${site.id}/users`);
});

// TODO: Stop creating new users in this test, use seed data instead

test('Verify user manager is able to remove a user', async ({
  page,
  getTestSite,
  newUserName,
}) => {
  const newUser = newUserName(test.info());
  await usersPage.addUserButton.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users/manage`);

  await expect(manageUserPage.emailStep.title).toBeVisible();
  await manageUserPage.emailStep.emailInput.fill(newUser);
  await manageUserPage.emailStep.continueButton.click();

  await expect(manageUserPage.rolesStep.title).toBeVisible();
  await manageUserPage.rolesStep.appointmentManagerCheckbox.check();
  await manageUserPage.rolesStep.continueButton.click();

  await expect(manageUserPage.summaryStep.title).toBeVisible();
  await manageUserPage.summaryStep.continueButton.click();

  await usersPage.userExists(newUser);
  await usersPage.removeFromThisSiteLink(newUser);
  await removeUserPage.verifyUserNavigatedToRemovePage(newUser);
  await removeUserPage.clickButton('Remove this account');
  await usersPage.userDoesNotExist(newUser);
});

test('Displays a notification banner after removing a user, which disappears when Close is clicked', async ({
  page,
  getTestSite,
  newUserName,
}) => {
  const newUser = newUserName(test.info());
  await usersPage.addUserButton.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users/manage`);

  await expect(manageUserPage.emailStep.title).toBeVisible();
  await manageUserPage.emailStep.emailInput.fill(newUser);
  await manageUserPage.emailStep.continueButton.click();

  await expect(manageUserPage.rolesStep.title).toBeVisible();
  await manageUserPage.rolesStep.appointmentManagerCheckbox.check();
  await manageUserPage.rolesStep.continueButton.click();

  await expect(manageUserPage.summaryStep.title).toBeVisible();
  await manageUserPage.summaryStep.continueButton.click();
  await usersPage.userExists(newUser);
  await usersPage.removeFromThisSiteLink(newUser);
  await removeUserPage.verifyUserNavigatedToRemovePage(newUser);
  await removeUserPage.clickButton('Remove this account');
  await usersPage.verifyRemoveUserSuccessBannerDisplayed(newUser);
  await usersPage.closeBanner();
  await usersPage.verifyRemoveUserSuccessBannerNotDisplayed(newUser);
});

test('Receives 404 when trying to remove an invalid user', async ({ page }) => {
  await page.goto(
    `/manage-your-appointments/site/${site.id}/users/remove?user=not-a-user`,
  );
  await expect(notFoundPage.title).toBeVisible();
  await expect(notFoundPage.notFoundMessageText).toBeVisible();
});
