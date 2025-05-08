import { test, expect } from '../../fixtures';
import {
  NotFoundPage,
  RemoveUserPage,
  LoginPage,
  UsersPage,
} from '@testing-page-objects';

let put: RemoveUserPage;
let usersPage: UsersPage;

test.beforeEach(async ({ page, getTestSite }) => {
  usersPage = await new LoginPage(page)
    .logInWithNhsMail()
    .then(oAuthPage => oAuthPage.signIn())
    .then(siteSelectionPage => siteSelectionPage.selectSite(getTestSite()))
    .then(sitePage => sitePage.clickManageUsersCard());
});

test('A user removes the account of another user', async ({ newUserName }) => {
  // TODO: Use seed data instead of creating a new user just for this test
  const manageUserPage = await usersPage.clickAddUser();

  await expect(manageUserPage.emailStep.title).toBeVisible();
  await manageUserPage.emailStep.emailInput.fill(newUserName);
  await manageUserPage.emailStep.continueButton.click();

  await expect(manageUserPage.rolesStep.title).toBeVisible();
  await manageUserPage.rolesStep.appointmentManagerCheckbox.check();
  await manageUserPage.rolesStep.continueButton.click();

  await expect(manageUserPage.summaryStep.title).toBeVisible();
  await manageUserPage.summaryStep.continueButton.click();

  await expect(
    usersPage.page.getByRole('cell', { name: newUserName }),
  ).toBeVisible();
  put = await usersPage.clickRemoveUserLink(newUserName);

  await expect(put.title).toBeVisible();
  await expect(put.confirmationMessage).toHaveText(
    `Are you sure you want to remove ${newUserName} from this site?`,
  );

  usersPage = await put.clickConfirmButton();
  await expect(usersPage.page.getByText(newUserName)).not.toBeVisible();
});

test('A user manipulates the url to try to remove an invalid user', async ({
  page,
  getTestSite,
}) => {
  await page.goto(
    `/manage-your-appointments/site/${getTestSite().id}/users/remove?user=not-a-user`,
  );

  const notFoundPage = new NotFoundPage(page);
  await expect(notFoundPage.title).toBeVisible();
  await expect(notFoundPage.notFoundMessageText).toBeVisible();
});
