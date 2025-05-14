import { test, expect } from '../../fixtures';
import { NotFoundPage } from '@testing-page-objects';

test('A user removes the account of another user', async ({
  newUserName,
  getTestSite,
  signInToSite,
}) => {
  // TODO: Use seed data instead of creating a new user just for this test
  await signInToSite()
    .then(async sitePage => sitePage.clickManageUsersCard())
    .then(usersPage => usersPage.clickAddUser())
    .then(async manageUserPage => {
      await expect(manageUserPage.emailStep.title).toBeVisible();
      await manageUserPage.emailStep.emailInput.fill(newUserName);
      await manageUserPage.emailStep.continueButton.click();

      await expect(manageUserPage.rolesStep.title).toBeVisible();
      await manageUserPage.rolesStep.appointmentManagerCheckbox.check();
      await manageUserPage.rolesStep.continueButton.click();

      await expect(manageUserPage.summaryStep.title).toBeVisible();
      return await manageUserPage.summaryStep.saveUserRoles();
    })
    .then(async usersPage => {
      await expect(
        usersPage.page.getByRole('cell', { name: newUserName }),
      ).toBeVisible();

      return await usersPage.clickRemoveUserLink(newUserName);
    })
    .then(async removeUserPage => {
      await expect(removeUserPage.title).toBeVisible();
      await expect(removeUserPage.confirmationMessage).toHaveText(
        `Are you sure you wish to remove ${newUserName} from ${getTestSite(1).name}?`,
      );

      return await removeUserPage.clickConfirmButton();
    })
    .then(async usersPage => {
      await expect(
        usersPage.notificationBanner.getByText(
          `You have successfully removed ${newUserName} from the current site.`,
        ),
      ).toBeVisible();

      await usersPage.dismissNotificationBannerButton.click();
      await expect(usersPage.notificationBanner).not.toBeVisible();
      await expect(usersPage.page.getByText(newUserName)).not.toBeVisible();
    });
});

test('A user manipulates the url to try to remove an invalid user', async ({
  page,
  getTestSite,
  signInToSite,
}) => {
  await signInToSite();
  await page.goto(
    `/manage-your-appointments/site/${getTestSite().id}/users/remove?user=not-a-user`,
  );

  const notFoundPage = new NotFoundPage(page);
  await expect(notFoundPage.title).toBeVisible();
  await expect(notFoundPage.notFoundMessageText).toBeVisible();
});
