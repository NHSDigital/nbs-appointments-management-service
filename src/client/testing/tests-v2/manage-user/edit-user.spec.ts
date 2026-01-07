import { test, expect } from '../../fixtures-v2';
import NotAuthorizedPage from '../../page-objects-v2/not-authorized-page';

test('Verify user manage is able to edit a user role', async ({
  setUpSingleSite,
}) => {
  const { sitePage } = await setUpSingleSite();

  await sitePage
    .clickManageUsersCard()
    .then(async usersPage => usersPage.clickAddUserButton())
    .then(async manageUserPage => {
      const newUser = `testuser${Date.now()}@nhs.net`;

      await expect(manageUserPage.emailStep.title).toBeVisible();
      await manageUserPage.emailStep.emailInput.fill(newUser);

      const userRoles = await manageUserPage.saveUserEmail();
      await expect(userRoles.title).toBeVisible();
      await userRoles.appointmentManagerCheckbox.check();

      const summaryStep = await manageUserPage.saveUserRoles();
      await expect(summaryStep.title).toBeVisible();

      const usersPage = await manageUserPage.saveUserDetails();
      return { usersPage, newUser };
    })
    .then(async users => {
      const newUser = users.newUser;
      const usersPage = users.usersPage;

      await usersPage.userExists(newUser);
      const manageUserPage = await usersPage.clickEditLink(newUser);

      return { manageUserPage, newUser };
    })
    .then(async editUser => {
      const newUser = editUser.newUser;
      const manageUserPage = editUser.manageUserPage;

      await expect(manageUserPage.userRolesStep.title).toBeVisible();
      await manageUserPage.userRolesStep.appointmentManagerCheckbox.check();
      await manageUserPage.userRolesStep.availabilityManagerCheckbox.uncheck();

      const summaryStep = await manageUserPage.saveUserRoles();
      await expect(summaryStep.title).toBeVisible();

      const usersPage = await manageUserPage.saveUserDetails();
      return { usersPage, newUser };
    })
    .then(async users => {
      const newUser = users.newUser;
      const usersPage = users.usersPage;

      await usersPage.verifyUserRoles('Appointment manager', newUser);
      await usersPage.verifyUserRoleRemoved('Availability manager', newUser);
    });
});

test('Receives 403 when trying to edit self', async ({
  page,
  setUpSingleSite,
}) => {
  const { sitePage } = await setUpSingleSite();
  const notAuthorizedPage = new NotAuthorizedPage(page);

  await page.goto(
    `/manage-your-appointments/site/${sitePage.site?.id}/users/manage?user=zzz_test_user_1@nhs.net`,
  );
  await expect(notAuthorizedPage.title).toBeVisible();
});
