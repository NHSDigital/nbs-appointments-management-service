import { NotFoundPage } from '@e2etests/page-objects';
import { test, expect } from '../../fixtures-v2';

test('Verify user manager is able to remove a user', async ({
  setUpSingleSite,
}) => {
  const { sitePage } = await setUpSingleSite();

  await sitePage
    .clickManageUsersCard()
    .then(async usersPage => usersPage.clickAddUserButton())
    .then(async manageUserPage => {
      const newUser = `testuser${Date.now()}@example.com`;

      await expect(manageUserPage.emailStep.title).toBeVisible();
      await manageUserPage.emailStep.emailInput.fill(newUser);
      await manageUserPage.emailStep.continueButton.click();

      await expect(manageUserPage.userRolesStep.title).toBeVisible();
      await manageUserPage.userRolesStep.appointmentManagerCheckbox.check();
      await manageUserPage.userRolesStep.continueButton.click();

      await expect(manageUserPage.summaryStep.title).toBeVisible();
      await manageUserPage.summaryStep.continueButton.click();

      const usersPage = await manageUserPage.saveUserDetails();
      return { usersPage, newUser };
    })
    .then(async users => {
      const newUser = users.newUser;
      const usersPage = users.usersPage;

      await usersPage.userExists(newUser);
      const removeUserPage = await usersPage.clickRemoveFromThisSite(newUser);

      return { removeUserPage, newUser };
    })
    .then(async removeUser => {
      const newUser = removeUser.newUser;
      const removeUserPage = removeUser.removeUserPage;

      await removeUserPage.verifyUserNavigatedToRemovePage(
        newUser,
        sitePage.site?.name || '',
      );
      const usersPage = await removeUserPage.clickButton('Remove this account');

      return { usersPage, newUser };
    })
    .then(async users => {
      const newUser = users.newUser;
      const usersPage = users.usersPage;

      await usersPage.userDoesNotExist(newUser);
    });
});

test('Displays a notification banner after removing a user, which disappears when Close is clicked', async ({
  setUpSingleSite,
}) => {
  const { sitePage } = await setUpSingleSite();

  await sitePage
    .clickManageUsersCard()
    .then(async usersPage => usersPage.clickAddUserButton())
    .then(async manageUserPage => {
      const newUser = `testuser${Date.now()}@example.com`;

      await expect(manageUserPage.emailStep.title).toBeVisible();
      await manageUserPage.emailStep.emailInput.fill(newUser);
      await manageUserPage.emailStep.continueButton.click();

      await expect(manageUserPage.userRolesStep.title).toBeVisible();
      await manageUserPage.userRolesStep.appointmentManagerCheckbox.check();
      await manageUserPage.userRolesStep.continueButton.click();

      await expect(manageUserPage.summaryStep.title).toBeVisible();
      await manageUserPage.summaryStep.continueButton.click();

      const usersPage = await manageUserPage.saveUserDetails();
      return { usersPage, newUser };
    })
    .then(async users => {
      const newUser = users.newUser;
      const usersPage = users.usersPage;

      await usersPage.userExists(newUser);
      const removeUserPage = await usersPage.clickRemoveFromThisSite(newUser);

      return { removeUserPage, newUser };
    })
    .then(async removeUser => {
      const newUser = removeUser.newUser;
      const removeUserPage = removeUser.removeUserPage;

      await removeUserPage.verifyUserNavigatedToRemovePage(
        newUser,
        sitePage.site?.name || '',
      );
      const usersPage = await removeUserPage.clickButton('Remove this account');

      return { usersPage, newUser };
    })
    .then(async users => {
      const newUser = users.newUser;
      const usersPage = users.usersPage;

      await usersPage.verifyRemoveUserSuccessBannerDisplayed(newUser);
      await usersPage.closeBanner();
      await usersPage.verifyRemoveUserSuccessBannerNotDisplayed(newUser);
    });
});

test('Receives 404 when trying to remove an invalid user', async ({
  page,
  setUpSingleSite,
}) => {
  const { sitePage } = await setUpSingleSite();
  const notFoundPage = new NotFoundPage(page);

  await page.goto(
    `/manage-your-appointments/site/${sitePage.site?.id}/users/remove?user=not-a-user`,
  );
  await expect(notFoundPage.title).toBeVisible();
  await expect(notFoundPage.notFoundMessageText).toBeVisible();
});
