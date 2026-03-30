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

test('Permissions are applied per site for the same user, and affect card visibility', async ({
  setup,
}) => {
  const { sitePage, additionalUserData } = await setup({
    additionalUsers: [
      {
        siteRoles: [
          ['canned:user-manager'],
          ['system:integration-test-user'],
          ['system:api-user'],
          ['canned:site-details-manager'],
          ['canned:availability-manager'],
          ['canned:availability-manager', 'canned:user-manager'],
        ],
      },
    ],
  });

  const newUser = additionalUserData.get('0');

  if (newUser === undefined) {
    throw new Error();
  }

  const oidc = newUser.user.oidc;

  //userManager at site1,
  //integrationUser at site2
  //apiUser at site3
  //site manager at site 4
  //availabilityManager at site 5
  //userManager AND availabilityManager at site 6
  const site1 = newUser.sites[0];
  const site2 = newUser.sites[1];
  const site3 = newUser.sites[2];
  const site4 = newUser.sites[3];
  const site5 = newUser.sites[4];
  const site6 = newUser.sites[5];

  //login as extra user that has two site permissions
  await sitePage.logOut().then(async loginPage => {
    const mockOidcLoginPage = await loginPage.logInWithNhsMail();
    const siteSelectionPage1 = await mockOidcLoginPage.signIn(oidc);
    const site1Page = await siteSelectionPage1.selectSite(site1);

    //assert cards for role

    //TODO why does a userManager role have permission to access to the availability and bookings...?
    //is this really needed?
    await site1Page.verifyTileVisible('ManageAppointment');

    await site1Page.verifyTileVisible('UserManagement');
    await site1Page.verifyTileVisible('SiteManagement');
    //since user has one site with this permission, visible on all site pages
    await site1Page.verifyTileVisible('DownloadReports');

    await site1Page.verifyTileNotVisible('CreateAvailability');

    await site1Page.changeSite().then(async siteSelectionPage2 => {
      const site2Page = await siteSelectionPage2.selectSite(site2);

      //assert cards for role
      await site2Page.verifyTileVisible('ManageAppointment');
      await site2Page.verifyTileVisible('SiteManagement');
      await site2Page.verifyTileVisible('UserManagement');
      await site2Page.verifyTileVisible('CreateAvailability');
      await site2Page.verifyTileVisible('DownloadReports');

      await site2Page.changeSite().then(async siteSelectionPage3 => {
        const site3Page = await siteSelectionPage3.selectSite(site3);

        //assert cards for role
        await site3Page.verifyTileVisible('ManageAppointment');
        await site3Page.verifyTileVisible('SiteManagement');
        //since user has one site with this permission, visible on all site pages
        await site3Page.verifyTileVisible('DownloadReports');

        await site3Page.verifyTileNotVisible('CreateAvailability');
        await site3Page.verifyTileNotVisible('UserManagement');

        await site3Page.changeSite().then(async siteSelectionPage4 => {
          const site4Page = await siteSelectionPage4.selectSite(site4);

          //assert cards for role
          await site4Page.verifyTileVisible('ManageAppointment');
          await site4Page.verifyTileVisible('SiteManagement');
          //since user has one site with this permission, visible on all site pages
          await site4Page.verifyTileVisible('DownloadReports');

          await site4Page.verifyTileNotVisible('CreateAvailability');
          await site4Page.verifyTileNotVisible('UserManagement');

          await site4Page.changeSite().then(async siteSelectionPage5 => {
            const site5Page = await siteSelectionPage5.selectSite(site5);

            //assert cards for role
            await site5Page.verifyTileVisible('ManageAppointment');
            await site5Page.verifyTileVisible('SiteManagement');
            await site5Page.verifyTileVisible('CreateAvailability');
            await site5Page.verifyTileVisible('DownloadReports');

            await site5Page.verifyTileNotVisible('UserManagement');

            await site5Page.changeSite().then(async siteSelectionPage6 => {
              const site6Page = await siteSelectionPage6.selectSite(site6);

              //all cards visible for both roles for site
              await site6Page.verifyTileVisible('ManageAppointment');
              await site6Page.verifyTileVisible('SiteManagement');
              await site6Page.verifyTileVisible('UserManagement');
              await site6Page.verifyTileVisible('CreateAvailability');
              await site6Page.verifyTileVisible('DownloadReports');
            });
          });
        });
      });
    });
  });
});

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
