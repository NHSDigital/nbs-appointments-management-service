import { LoginPage } from '@e2etests/page-objects';
import { test, expect } from '../../fixtures-v2';
import Users from '../../page-objects-v2/manage-user/users';
import NotAuthorizedPage from '../../page-objects-v2/not-authorized-page';
import { buildE2ETestSite, buildE2ETestUser } from '@e2etests/data';

test('A user with the appropriate permission can view other users at a site and also edit them', async ({
  setUpSingleSite,
}) => {
  const { sitePage } = await setUpSingleSite();

  await sitePage.clickManageUsersCard().then(async usersPage => {
    await expect(usersPage.manageColumn).toBeVisible();
    const usersCount = await usersPage.getUserCount();
    expect(usersCount).toBeGreaterThan(0);
  });
});

test('Navigation straight to the user management page works as expected', async ({
  page,
  setUpSingleSite,
}) => {
  const { sitePage } = await setUpSingleSite();

  await page.goto(`/manage-your-appointments/site/${sitePage.site?.id}/users`);

  const usersPage = new Users(page, sitePage.site);
  await expect(usersPage.title).toBeVisible();
});

test('Navigating straight to the user management page without permission shows 403', async ({
  page,
  setUpSingleSite,
}) => {
  const { sitePage } = await setUpSingleSite({
    roles: ['canned:appointment-manager'],
  });
  const notAuthorizedPage = new NotAuthorizedPage(page);

  await page.goto(`/manage-your-appointments/site/${sitePage.site?.id}/users`);
  await expect(notAuthorizedPage.title).toBeVisible();
});

// test('Permissions are applied per site', )

test('Verify user manager cannot edit or remove themself', async ({
  setUpSingleSite,
}) => {
  const { sitePage, testId } = await setUpSingleSite();
  const testUserEmail = `test-user-${testId}@nhs.net`;

  await sitePage.clickManageUsersCard().then(async usersPage => {
    await usersPage.verifyLinkNotVisible(testUserEmail, 'Edit');
    await usersPage.verifyLinkNotVisible(
      testUserEmail,
      'Remove from this site',
    );
  });
});

test('Verify user can only view appointment manager related tiles when they have appointment manager role', async ({
  setUpSingleSite,
}) => {
  const { sitePage, additionalUserIds } = await setUpSingleSite({
    additionalUsers: [
      {
        roles: ['canned:appointment-manager'],
      },
    ],
  });

  const newUser = additionalUserIds.get('0');
  const newUserEmail = newUser ? `testuser${newUser}@nhs.net` : '';

  await sitePage
    .clickManageUsersCard()
    .then(async usersPage => usersPage.clickAddUserButton())
    .then(async manageUserPage => {
      await expect(manageUserPage.emailStep.title).toBeVisible();
      await manageUserPage.emailStep.emailInput.fill(newUserEmail);

      const userRoles = await manageUserPage.saveUserEmail();
      await expect(userRoles.title).toBeVisible();
      await userRoles.appointmentManagerCheckbox.check();

      const summaryStep = await manageUserPage.saveUserRoles();
      await expect(summaryStep.title).toBeVisible();

      const usersPage = await manageUserPage.saveUserDetails();
      return { usersPage };
    })
    .then(async users => {
      const usersPage = users.usersPage;

      await usersPage.verifyUserRoles('Appointment manager', newUserEmail);
      const loginPage = await usersPage.logOut();

      return { loginPage, newUser };
    })
    .then(async root => {
      const loginPage: LoginPage = root.loginPage;

      const mockOidcLoginPage = await loginPage.logInWithNhsMail();
      const siteSelectionPage = await mockOidcLoginPage.signIn(
        buildE2ETestUser(newUser ?? 0),
      );
      const newUserSitePage = await siteSelectionPage.selectSite(
        buildE2ETestSite(newUser ?? 0),
      );

      await newUserSitePage.verifyTileVisible('ManageAppointment');
      await newUserSitePage.verifyTileVisible('SiteManagement');
      await newUserSitePage.verifyTileNotVisible('UserManagement');
      await newUserSitePage.verifyTileNotVisible('CreateAvailability');
    });
});

test('Verify user can only view availability manager related tiles when they have availability manager role', async ({
  setUpSingleSite,
}) => {
  const { sitePage, additionalUserIds } = await setUpSingleSite({
    additionalUsers: [
      {
        roles: ['canned:availability-manager'],
      },
    ],
  });

  const newUser = additionalUserIds.get('0');
  const newUserEmail = newUser ? `testuser${newUser}@nhs.net` : '';
  await sitePage
    .clickManageUsersCard()
    .then(async usersPage => usersPage.clickAddUserButton())
    .then(async manageUserPage => {
      await expect(manageUserPage.emailStep.title).toBeVisible();
      await manageUserPage.emailStep.emailInput.fill(newUserEmail);

      const userRoles = await manageUserPage.saveUserEmail();
      await expect(userRoles.title).toBeVisible();
      await userRoles.availabilityManagerCheckbox.check();

      const summaryStep = await manageUserPage.saveUserRoles();
      await expect(summaryStep.title).toBeVisible();

      const usersPage = await manageUserPage.saveUserDetails();
      return { usersPage };
    })
    .then(async users => {
      const usersPage = users.usersPage;

      await usersPage.verifyUserRoles('Availability manager', newUserEmail);
      const loginPage = await usersPage.logOut();

      return { loginPage, newUser };
    })
    .then(async root => {
      const loginPage: LoginPage = root.loginPage;

      const mockOidcLoginPage = await loginPage.logInWithNhsMail();
      const siteSelectionPage = await mockOidcLoginPage.signIn(
        buildE2ETestUser(root.newUser ?? 0),
      );
      const newUserSitePage = await siteSelectionPage.selectSite(
        buildE2ETestSite(root.newUser ?? 0),
      );

      await newUserSitePage.verifyTileVisible('ManageAppointment');
      await newUserSitePage.verifyTileVisible('SiteManagement');
      await newUserSitePage.verifyTileNotVisible('UserManagement');
      await newUserSitePage.verifyTileVisible('CreateAvailability');
    });
});

test('Verify user can only view user manager related tiles when they have user manager role', async ({
  setUpSingleSite,
}) => {
  const { sitePage, additionalUserIds } = await setUpSingleSite({
    additionalUsers: [
      {
        roles: ['canned:user-manager'],
      },
    ],
  });

  const newUser = additionalUserIds.get('0');
  const newUserEmail = newUser ? `testuser${newUser}@nhs.net` : '';

  await sitePage
    .clickManageUsersCard()
    .then(async usersPage => usersPage.clickAddUserButton())
    .then(async manageUserPage => {
      await expect(manageUserPage.emailStep.title).toBeVisible();
      await manageUserPage.emailStep.emailInput.fill(newUserEmail);

      const userRoles = await manageUserPage.saveUserEmail();
      await expect(userRoles.title).toBeVisible();
      await userRoles.userManagerCheckbox.check();

      const summaryStep = await manageUserPage.saveUserRoles();
      await expect(summaryStep.title).toBeVisible();

      const usersPage = await manageUserPage.saveUserDetails();
      return { usersPage };
    })
    .then(async users => {
      const usersPage = users.usersPage;

      await usersPage.verifyUserRoles('User manager', newUserEmail);
      const loginPage = await usersPage.logOut();

      return { loginPage, newUser };
    })
    .then(async root => {
      const loginPage: LoginPage = root.loginPage;

      const mockOidcLoginPage = await loginPage.logInWithNhsMail();
      const siteSelectionPage = await mockOidcLoginPage.signIn(
        buildE2ETestUser(root.newUser ?? 0),
      );
      const newUserSitePage = await siteSelectionPage.selectSite(
        buildE2ETestSite(root.newUser ?? 0),
      );

      await newUserSitePage.verifyTileVisible('ManageAppointment');
      await newUserSitePage.verifyTileVisible('SiteManagement');
      await newUserSitePage.verifyTileVisible('UserManagement');
      await newUserSitePage.verifyTileNotVisible('CreateAvailability');
    });
});

test('Verify user can only view site details manager related tiles when they have site details manager role', async ({
  setUpSingleSite,
}) => {
  const { sitePage, additionalUserIds } = await setUpSingleSite({
    additionalUsers: [
      {
        roles: ['canned:site-details-manager'],
      },
    ],
  });

  const newUser = additionalUserIds.get('0');
  const newUserEmail = newUser ? `testuser${newUser}@nhs.net` : '';
  await sitePage
    .clickManageUsersCard()
    .then(async usersPage => usersPage.clickAddUserButton())
    .then(async manageUserPage => {
      await expect(manageUserPage.emailStep.title).toBeVisible();
      await manageUserPage.emailStep.emailInput.fill(newUserEmail);

      const userRoles = await manageUserPage.saveUserEmail();
      await expect(userRoles.title).toBeVisible();
      await userRoles.siteDetailsManagerCheckbox.check();

      const summaryStep = await manageUserPage.saveUserRoles();
      await expect(summaryStep.title).toBeVisible();

      const usersPage = await manageUserPage.saveUserDetails();
      return { usersPage };
    })
    .then(async users => {
      const usersPage = users.usersPage;

      await usersPage.verifyUserRoles('Site details manager', newUserEmail);
      const loginPage = await usersPage.logOut();

      return { loginPage, newUser };
    })
    .then(async root => {
      const loginPage: LoginPage = root.loginPage;

      const mockOidcLoginPage = await loginPage.logInWithNhsMail();
      const siteSelectionPage = await mockOidcLoginPage.signIn(
        buildE2ETestUser(root.newUser ?? 0),
      );
      const newUserSitePage = await siteSelectionPage.selectSite(
        buildE2ETestSite(root.newUser ?? 0),
      );

      await newUserSitePage.verifyTileVisible('ManageAppointment');
      await newUserSitePage.verifyTileVisible('SiteManagement');
      await newUserSitePage.verifyTileNotVisible('UserManagement');
      await newUserSitePage.verifyTileNotVisible('CreateAvailability');
    });
});
