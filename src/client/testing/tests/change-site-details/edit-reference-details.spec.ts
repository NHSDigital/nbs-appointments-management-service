import { test, expect } from '../../fixtures';

test(
  'A user updates the reference details for a site',
  { tag: ['@acts-as:user1', '@alters:site2'] },
  async ({ signInToSite }) => {
    await signInToSite(1, 2)
      .then(sitePage => sitePage.clickSiteDetailsCard())
      .then(siteDetailsPage => siteDetailsPage.clickEditReferenceDetailsLink())
      .then(async editReferenceDetailsPage => {
        await editReferenceDetailsPage.odsCodeInput.fill('ABC000032434543');
        await editReferenceDetailsPage.icbSelectInput.selectOption(
          'Integrated Care Board 1',
        );
        await editReferenceDetailsPage.regionSelectInput.selectOption(
          'Region 1',
        );

        return editReferenceDetailsPage.saveReferenceDetails();
      })
      .then(async siteDetailsPage => {
        await expect(
          siteDetailsPage.notificationBanner.getByText(
            'You have successfully updated the reference details for the current site.',
          ),
        ).toBeVisible();
        await siteDetailsPage.dismissNotificationBannerButton.click();
        await expect(siteDetailsPage.notificationBanner).not.toBeVisible();

        await expect(siteDetailsPage.odsCode).toHaveText(/ABC000032434543/);
        await expect(siteDetailsPage.icb).toHaveText(/Integrated Care Board 1/);
        await expect(siteDetailsPage.region).toHaveText(/Region 1/);
      });
  },
);

test(
  'A user navigates back to the site details page using the back link',
  { tag: ['@acts-as:user1', '@asserts-on:site2'] },
  async ({ signInToSite }) => {
    await signInToSite(1, 2)
      .then(sitePage => sitePage.clickSiteDetailsCard())
      .then(siteDetailsPage => siteDetailsPage.clickEditReferenceDetailsLink())
      .then(editReferenceDetailsPage => editReferenceDetailsPage.goBack())
      .then(async siteDetailsPage => {
        await expect(siteDetailsPage.title).toBeVisible();
      });
  },
);
