import { LoginPage } from '@e2etests/page-objects';
import { test, expect } from '../../fixtures-v2';
import Users from '../../page-objects-v2/manage-user/users';
import NotAuthorizedPage from '../../page-objects-v2/not-authorized-page';

test('A user with the appropriate permission can view other users at a site and also edit them', async ({
  setup,
}) => {
  const { sitePage } = await setup();

  await sitePage.clickManageUsersCard().then(async usersPage => {
    await expect(usersPage.manageColumn).toBeVisible();
    const usersCount = await usersPage.getUserCount();
    expect(usersCount).toBeGreaterThan(0);
  });
});

test('Navigation straight to the user management page works as expected', async ({
  page,
  setup,
}) => {
  const { sitePage } = await setup();

  await page.goto(`/manage-your-appointments/site/${sitePage.site?.id}/users`);

  const usersPage = new Users(page, sitePage.site);
  await expect(usersPage.title).toBeVisible();
});

test('Navigating straight to the user management page without permission shows 403', async ({
  page,
  setup,
}) => {
  const { sitePage } = await setup({
    roles: ['canned:appointment-manager'],
  });
  const notAuthorizedPage = new NotAuthorizedPage(page);

  await page.goto(`/manage-your-appointments/site/${sitePage.site?.id}/users`);
  await expect(notAuthorizedPage.title).toBeVisible();
});

// TODO: This test needs migrating over when we have a way to assign multiple sites to a user
// Copied from v1 as that file is being removed now the rest have been converted over to v2

// test('permissions are applied per site', async ({ getTestUser }) => {
//   await rootPage.goto();
//   await rootPage.pageContentLogInButton.click();
//   await oAuthPage.signIn(getTestUser(2));

//   // First check Edit column exists at Church Lane
//   await siteSelectionPage.selectSite(site2);
//   await sitePage.userManagementCard.click();
//   await expect(usersPage.manageColumn).toBeVisible();

//   // Then check it does NOT exist at Robin Lane
//   await rootPage.goto();

//   await siteSelectionPage.selectSite(site1);

//   await sitePage.siteManagementCard.click();
//   await expect(siteDetailsPage1.editSiteDetailsButton).not.toBeVisible();
// });

test('Verify user manager cannot edit or remove themself', async ({
  setup,
}) => {
  const { sitePage, testId } = await setup();
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
  setup,
}) => {
  const { sitePage, additionalUserData } = await setup({
    additionalUsers: [
      {
        siteRoles: [['canned:appointment-manager']],
      },
    ],
  });

  const newUser = additionalUserData.get('0');

  if (newUser === undefined) {
    throw new Error();
  }

  const oidc = newUser.user.oidc;
  const site = newUser.sites[0];

  await sitePage
    .clickManageUsersCard()
    .then(async usersPage => usersPage.clickAddUserButton())
    .then(async manageUserPage => {
      await expect(manageUserPage.emailStep.title).toBeVisible();
      await manageUserPage.emailStep.emailInput.fill(oidc.subjectId);

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

      await usersPage.verifyUserRoles('Appointment manager', oidc.subjectId);
      const loginPage = await usersPage.logOut();

      return { loginPage, newUser };
    })
    .then(async root => {
      const loginPage: LoginPage = root.loginPage;

      const mockOidcLoginPage = await loginPage.logInWithNhsMail();
      const siteSelectionPage = await mockOidcLoginPage.signIn(oidc);
      const newUserSitePage = await siteSelectionPage.selectSite(site);

      await newUserSitePage.verifyTileVisible('ManageAppointment');
      await newUserSitePage.verifyTileVisible('SiteManagement');
      await newUserSitePage.verifyTileNotVisible('UserManagement');
      await newUserSitePage.verifyTileNotVisible('CreateAvailability');
    });
});

test('Verify user can only view availability manager related tiles when they have availability manager role', async ({
  setup,
}) => {
  const { sitePage, additionalUserData } = await setup({
    additionalUsers: [
      {
        siteRoles: [['canned:availability-manager']],
      },
    ],
  });

  const newUser = additionalUserData.get('0');

  if (newUser === undefined) {
    throw new Error();
  }

  const oidc = newUser.user.oidc;
  const site = newUser.sites[0];

  await sitePage
    .clickManageUsersCard()
    .then(async usersPage => usersPage.clickAddUserButton())
    .then(async manageUserPage => {
      await expect(manageUserPage.emailStep.title).toBeVisible();
      await manageUserPage.emailStep.emailInput.fill(oidc.subjectId);

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

      await usersPage.verifyUserRoles('Availability manager', oidc.subjectId);
      const loginPage = await usersPage.logOut();

      return { loginPage, newUser };
    })
    .then(async root => {
      const loginPage: LoginPage = root.loginPage;

      const mockOidcLoginPage = await loginPage.logInWithNhsMail();
      const siteSelectionPage = await mockOidcLoginPage.signIn(oidc);
      const newUserSitePage = await siteSelectionPage.selectSite(site);

      await newUserSitePage.verifyTileVisible('ManageAppointment');
      await newUserSitePage.verifyTileVisible('SiteManagement');
      await newUserSitePage.verifyTileNotVisible('UserManagement');
      await newUserSitePage.verifyTileVisible('CreateAvailability');
    });
});

test('Verify user can only view user manager related tiles when they have user manager role', async ({
  setup,
}) => {
  const { sitePage, additionalUserData } = await setup({
    additionalUsers: [
      {
        siteRoles: [['canned:user-manager']],
      },
    ],
  });

  const newUser = additionalUserData.get('0');

  if (newUser === undefined) {
    throw new Error();
  }

  const oidc = newUser.user.oidc;
  const site = newUser.sites[0];

  await sitePage
    .clickManageUsersCard()
    .then(async usersPage => usersPage.clickAddUserButton())
    .then(async manageUserPage => {
      await expect(manageUserPage.emailStep.title).toBeVisible();
      await manageUserPage.emailStep.emailInput.fill(oidc.subjectId);

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

      await usersPage.verifyUserRoles('User manager', oidc.subjectId);
      const loginPage = await usersPage.logOut();

      return { loginPage, newUser };
    })
    .then(async root => {
      const loginPage: LoginPage = root.loginPage;

      const mockOidcLoginPage = await loginPage.logInWithNhsMail();
      const siteSelectionPage = await mockOidcLoginPage.signIn(oidc);
      const newUserSitePage = await siteSelectionPage.selectSite(site);

      await newUserSitePage.verifyTileVisible('ManageAppointment');
      await newUserSitePage.verifyTileVisible('SiteManagement');
      await newUserSitePage.verifyTileVisible('UserManagement');
      await newUserSitePage.verifyTileNotVisible('CreateAvailability');
    });
});

test('Verify user can only view site details manager related tiles when they have site details manager role', async ({
  setup,
}) => {
  const { sitePage, additionalUserData } = await setup({
    additionalUsers: [
      {
        siteRoles: [['canned:site-details-manager']],
      },
    ],
  });

  const newUser = additionalUserData.get('0');

  if (newUser === undefined) {
    throw new Error();
  }

  const oidc = newUser.user.oidc;
  const site = newUser.sites[0];

  await sitePage
    .clickManageUsersCard()
    .then(async usersPage => usersPage.clickAddUserButton())
    .then(async manageUserPage => {
      await expect(manageUserPage.emailStep.title).toBeVisible();
      await manageUserPage.emailStep.emailInput.fill(oidc.subjectId);

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

      await usersPage.verifyUserRoles('Site details manager', oidc.subjectId);
      const loginPage = await usersPage.logOut();

      return { loginPage, newUser };
    })
    .then(async root => {
      const loginPage: LoginPage = root.loginPage;

      const mockOidcLoginPage = await loginPage.logInWithNhsMail();
      const siteSelectionPage = await mockOidcLoginPage.signIn(oidc);
      const newUserSitePage = await siteSelectionPage.selectSite(site);

      await newUserSitePage.verifyTileVisible('ManageAppointment');
      await newUserSitePage.verifyTileVisible('SiteManagement');
      await newUserSitePage.verifyTileNotVisible('UserManagement');
      await newUserSitePage.verifyTileNotVisible('CreateAvailability');
    });
});
