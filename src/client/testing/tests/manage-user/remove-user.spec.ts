import { test, expect } from '../../fixtures';
import {
  EditManageUserRolesPage,
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
let editManageUserRolesPage: EditManageUserRolesPage;
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
  editManageUserRolesPage = new EditManageUserRolesPage(page);
  removeUserPage = new RemoveUserPage(page, site);
  notFoundPage = new NotFoundPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');
  await sitePage.userManagementCard.click();
  await page.waitForURL(`**/site/${site.id}/users`);
});

test('Verify user manager is able to remove a user', async ({
  newUserName,
}) => {
  await usersPage.addUserButton.click();
  await editManageUserRolesPage.emailInput.fill(newUserName);
  await editManageUserRolesPage.continueButton.click();
  await editManageUserRolesPage.selectStaffRole('Appointment manager');
  await editManageUserRolesPage.continueButton.click();
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
  await usersPage.addUserButton.click();
  await editManageUserRolesPage.emailInput.fill(newUserName);
  await editManageUserRolesPage.continueButton.click();
  await editManageUserRolesPage.selectStaffRole('Appointment manager');
  await editManageUserRolesPage.continueButton.click();
  await editManageUserRolesPage.confirmAndSaveButton.click();
  await usersPage.userExists(newUserName);
  await usersPage.removeFromThisSiteLink(newUserName);
  await removeUserPage.verifyUserNavigatedToRemovePage(newUserName);
  await removeUserPage.clickButton('Remove this account');
  await usersPage.verifyRemoveUserSuccessBannerDisplayed(newUserName);
  await usersPage.closeBanner();
  await usersPage.verifyRemoveUserSuccessBannerNotDisplayed(newUserName);
});

test('Receives 404 when trying to remove an invalid user', async ({ page }) => {
  await page.goto(
    `/manage-your-appointments/site/${site.id}/users/remove?user=not-a-user`,
  );
  await expect(notFoundPage.title).toBeVisible();
  await expect(notFoundPage.notFoundMessageText).toBeVisible();
});
