import { test, expect } from '../../fixtures';
import { LoginPage, NotAuthorizedPage, UsersPage } from '@testing-page-objects';

let put: UsersPage;

test.beforeEach(async ({ page, getTestSite }) => {
  put = await new LoginPage(page)
    .logInWithNhsMail()
    .then(oAuthPage => oAuthPage.signIn())
    .then(siteSelectionPage => siteSelectionPage.selectSite(getTestSite()))
    .then(sitePage => sitePage.clickManageUsersCard());
});

test(
  'A user with the appropriate permission can view other users at a site but not edit them',
  { tag: ['@affects:user2', '@affects:site1'] },
  async ({ getTestSite, getTestUser, page }) => {
    put = await new LoginPage(page)
      .logInWithNhsMail()
      .then(oAuthPage => oAuthPage.signIn(getTestUser(2)))
      .then(siteSelectionPage => siteSelectionPage.selectSite(getTestSite(1)))
      .then(sitePage => sitePage.clickManageUsersCard());

    await expect(put.title).toBeVisible();
    await expect(put.emailColumn).toBeVisible();
    await expect(put.manageColumn).not.toBeVisible();
    await expect(put.addUserButton).not.toBeVisible();
  },
);

test(
  'A user with the appropriate permission can view other users at a site and also edit them',
  { tag: ['@uses:user2', '@uses:site1'] },
  async ({ getTestSite, getTestUser, page }) => {
    put = await new LoginPage(page)
      .logInWithNhsMail()
      .then(oAuthPage => oAuthPage.signIn(getTestUser(2)))
      .then(siteSelectionPage => siteSelectionPage.selectSite(getTestSite(1)))
      .then(sitePage => sitePage.clickManageUsersCard());

    await expect(put.manageColumn).toBeVisible();

    const userCount = await put.page
      .getByRole('row')
      .filter({
        hasNot: put.page.getByText(/int-test-user/),
        has: put.page.getByRole('link', { name: 'Edit' }),
      })
      .count();
    expect(userCount).toBeGreaterThan(0);
  },
);

test('Navigating straight to the user management page works as expected', async ({
  getTestUser,
  page,
}) => {
  const landingPage = await new LoginPage(page)
    .logInWithNhsMail()
    .then(oAuthPage => oAuthPage.signIn(getTestUser(2)));
  await landingPage.page.goto(
    '/manage-your-appointments/site/5914b64a-66bb-4ee2-ab8a-94958c1fdfcb/users',
  );

  await expect(put.title).toBeVisible();
});

test('Navigating straight to the user management page displays an appropriate error if the permission is missing', async ({
  page,
  getTestUser,
}) => {
  const landingPage = await new LoginPage(page)
    .logInWithNhsMail()
    .then(oAuthPage => oAuthPage.signIn(getTestUser(3)));
  await landingPage.page.goto(
    '/manage-your-appointments/site/5914b64a-66bb-4ee2-ab8a-94958c1fdfcb/users',
  );

  await expect(put.emailColumn).not.toBeVisible();
  await expect(new NotAuthorizedPage(page).title).toBeVisible();
  await page.goto(
    '/manage-your-appointments/site/5914b64a-66bb-4ee2-ab8a-94958c1fdfcb/users/manage',
  );
  await expect(new NotAuthorizedPage(page).title).toBeVisible();
});

test('permissions are applied per site', async ({
  page,
  getTestSite,
  getTestUser,
}) => {
  put = await new LoginPage(page)
    .logInWithNhsMail()
    .then(oAuthPage => oAuthPage.signIn(getTestUser(2)))
    .then(siteSelectionPage => siteSelectionPage.selectSite(getTestSite(2)))
    .then(sitePage => sitePage.clickManageUsersCard());

  await expect(put.manageColumn).toBeVisible();

  // Then check it does NOT exist at Robin Lane

  const siteSelectionPage = await put.topNav.clickServiceName();
  const sitePage = await siteSelectionPage.selectSite(getTestSite(1));

  put = await sitePage.clickManageUsersCard();
  await expect(put.manageColumn).not.toBeVisible();
});

test('Verify user manager cannot edit or remove self account', async ({
  page,
  getTestSite,
  getTestUser,
}) => {
  put = await new LoginPage(page)
    .logInWithNhsMail()
    .then(oAuthPage => oAuthPage.signIn(getTestUser(1)))
    .then(siteSelectionPage => siteSelectionPage.selectSite(getTestSite(1)))
    .then(sitePage => sitePage.clickManageUsersCard());

  await expect(
    put.page
      .getByRole('row')
      .filter({ has: put.page.getByText(getTestUser(1).username) })
      .getByRole('link', { name: 'Remove from this site' }),
  ).not.toBeVisible();
});

test('Verify user can only view appointment manager related tiles In app when user is assigned Appointment Manager role.', async ({
  page,
  getTestSite,
  getTestUser,
}) => {
  put = await new LoginPage(page)
    .logInWithNhsMail()
    .then(oAuthPage => oAuthPage.signIn(getTestUser(1)))
    .then(siteSelectionPage => siteSelectionPage.selectSite(getTestSite(1)))
    .then(sitePage => sitePage.clickManageUsersCard());

  const manageUserPage = await put.clickAddUser();

  const user8 = getTestUser(8);
  await expect(manageUserPage.emailStep.title).toBeVisible();
  await manageUserPage.emailStep.emailInput.fill(user8.subjectId);
  await manageUserPage.emailStep.continueButton.click();

  await expect(manageUserPage.rolesStep.title).toBeVisible();
  await manageUserPage.rolesStep.appointmentManagerCheckbox.check();
  await manageUserPage.rolesStep.availabilityManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.siteDetailsManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.userManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.continueButton.click();

  await expect(manageUserPage.summaryStep.title).toBeVisible();
  await manageUserPage.summaryStep.continueButton.click();

  await usersPage.verifyUserRoles('Appointment manager', user8.subjectId);
  await rootPage.logOut();
  await page.waitForURL(`**/manage-your-appointments/login`);
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(getTestUser(8));
  await expect(siteSelectionPage.title).toBeVisible();
  await siteSelectionPage.selectSite(site1.name);
  await sitePage.verifyTileVisible('ManageAppointment');
  await sitePage.verifyTileVisible('SiteManagement');
  await sitePage.verifyTileNotVisible('UserManagement');
  await sitePage.verifyTileNotVisible('CreateAvailability');
  await sitePage.siteManagementCard.click();
  await siteDetailsPage1.verifySitePage();
  await siteDetailsPage1.verifyEditButtonNotVisible();
});

test('Verify user can only view availability manager related tiles In app when user is assigned Availability Manager role.', async ({
  page,
  getTestSite,
  getTestUser,
}) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');

  await sitePage.userManagementCard.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users`);

  await usersPage.addUserButton.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users/manage`);

  const user9 = getTestUser(9);
  await expect(manageUserPage.emailStep.title).toBeVisible();
  await manageUserPage.emailStep.emailInput.fill(user9.subjectId);
  await manageUserPage.emailStep.continueButton.click();

  await expect(manageUserPage.rolesStep.title).toBeVisible();
  await manageUserPage.rolesStep.appointmentManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.availabilityManagerCheckbox.check();
  await manageUserPage.rolesStep.siteDetailsManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.userManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.continueButton.click();

  await expect(manageUserPage.summaryStep.title).toBeVisible();
  await manageUserPage.summaryStep.continueButton.click();

  await usersPage.verifyUserRoles('Availability manager', user9.subjectId);
  await rootPage.logOut();
  await page.waitForURL(`**/manage-your-appointments/login`);
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(getTestUser(9));
  await expect(siteSelectionPage.title).toBeVisible();
  await siteSelectionPage.selectSite(site1.name);
  await sitePage.verifyTileVisible('ManageAppointment');
  await sitePage.verifyTileVisible('SiteManagement');
  await sitePage.verifyTileNotVisible('UserManagement');
  await sitePage.verifyTileVisible('CreateAvailability');
  await sitePage.siteManagementCard.click();
  await siteDetailsPage1.verifySitePage();
  await siteDetailsPage1.verifyEditButtonNotVisible();
  await page.goto(`/manage-your-appointments/site/${site1.id}`);
  await sitePage.createAvailabilityCard.click();
  await createAvailabilityPage.createAvailabilityButton.click();
  await createAvailabilityPage.verifyCreateAvailabilitySessionPageDisplayed();
  await page.goto(`/manage-your-appointments/site/${site1.id}`);
  await sitePage.viewAvailabilityAndManageAppointmentsCard.click();
  await viewMonthAvailabilityPage.verifyViewNextMonthButtonDisplayed();
});

test('Verify user can only view user manager related tiles In app when user is assigned user Manager role.', async ({
  page,
  getTestSite,
  getTestUser,
}) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite('Robin Lane Medical Centre');

  await sitePage.userManagementCard.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users`);

  await usersPage.addUserButton.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users/manage`);

  const user10 = getTestUser(10);
  await expect(manageUserPage.emailStep.title).toBeVisible();
  await manageUserPage.emailStep.emailInput.fill(user10.subjectId);
  await manageUserPage.emailStep.continueButton.click();

  await expect(manageUserPage.rolesStep.title).toBeVisible();
  await manageUserPage.rolesStep.appointmentManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.availabilityManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.siteDetailsManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.userManagerCheckbox.check();
  await manageUserPage.rolesStep.continueButton.click();

  await expect(manageUserPage.summaryStep.title).toBeVisible();
  await manageUserPage.summaryStep.continueButton.click();

  await usersPage.verifyUserRoles('User manager', user10.subjectId);
  await rootPage.logOut();
  await page.waitForURL(`**/manage-your-appointments/login`);
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(getTestUser(10));
  await expect(siteSelectionPage.title).toBeVisible();
  await siteSelectionPage.selectSite(site1.name);
  await sitePage.verifyTileVisible('ManageAppointment');
  await sitePage.verifyTileVisible('SiteManagement');
  await sitePage.verifyTileVisible('UserManagement');
  await sitePage.verifyTileNotVisible('CreateAvailability');
  await sitePage.siteManagementCard.click();
  await siteDetailsPage1.verifySitePage();
  await siteDetailsPage1.verifyEditButtonNotVisible();
  await page.goto(`/manage-your-appointments/site/${site1.id}`);
  await sitePage.viewAvailabilityAndManageAppointmentsCard.click();
  await viewMonthAvailabilityPage.verifyViewNextMonthButtonDisplayed();
  await page.goto(`/manage-your-appointments/site/${site1.id}`);
  await sitePage.userManagementCard.click();
  await expect(usersPage.title).toBeVisible();
});

test('Verify user can only view site details manager related tiles In app when user is assigned site details manager role.', async ({
  page,
  getTestSite,
  getTestUser,
}) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite(site1.name);

  await sitePage.userManagementCard.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users`);

  await usersPage.addUserButton.click();
  await page.waitForURL(`**/site/${getTestSite().id}/users/manage`);

  const user11 = getTestUser(11);
  await expect(manageUserPage.emailStep.title).toBeVisible();
  await manageUserPage.emailStep.emailInput.fill(user11.subjectId);
  await manageUserPage.emailStep.continueButton.click();

  await expect(manageUserPage.rolesStep.title).toBeVisible();
  await manageUserPage.rolesStep.appointmentManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.availabilityManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.siteDetailsManagerCheckbox.check();
  await manageUserPage.rolesStep.userManagerCheckbox.uncheck();
  await manageUserPage.rolesStep.continueButton.click();

  await expect(manageUserPage.summaryStep.title).toBeVisible();
  await manageUserPage.summaryStep.continueButton.click();

  await usersPage.verifyUserRoles('Site details manager', user11.subjectId);
  await rootPage.logOut();
  await page.waitForURL(`**/manage-your-appointments/login`);
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(getTestUser(11));
  await expect(siteSelectionPage.title).toBeVisible();
  await siteSelectionPage.selectSite(site1.name);
  await sitePage.verifyTileVisible('ManageAppointment');
  await sitePage.verifyTileVisible('SiteManagement');
  await sitePage.verifyTileNotVisible('UserManagement');
  await sitePage.verifyTileNotVisible('CreateAvailability');
  await sitePage.siteManagementCard.click();
  await siteDetailsPage1.verifySitePage();
  await siteDetailsPage1.verifyEditButtonToBeVisible();
  await page.goto(`/manage-your-appointments/site/${site1.id}`);
  await sitePage.viewAvailabilityAndManageAppointmentsCard.click();
  await viewMonthAvailabilityPage.verifyViewNextMonthButtonDisplayed();
});
