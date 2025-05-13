import { test, expect } from '../../fixtures';

test(
  'A user updates the access needs for a site',
  { tag: ['@acts-as:user1', '@alters:site2'] },
  async ({ signInToSite }) => {
    await signInToSite(1, 2)
      .then(sitePage => sitePage.clickSiteDetailsCard())
      .then(siteDetailsPage => siteDetailsPage.clickEditAccessNeedsLink())
      .then(async editAccessNeedsPage => {
        await editAccessNeedsPage.checkboxes.accessibleToilet.check();
        await editAccessNeedsPage.checkboxes.stepFreeAccess.check();

        return editAccessNeedsPage.saveSiteDetails();
      })
      .then(async siteDetailsPage => {
        await expect(
          siteDetailsPage.notificationBanner.getByText(
            'You have successfully updated the access needs for the current site.',
          ),
        ).toBeVisible();

        await siteDetailsPage.dismissNotificationBannerButton.click();
        await expect(siteDetailsPage.notificationBanner).not.toBeVisible();

        await expect(siteDetailsPage.accessibleToilet).toHaveText(/Yes/);
        await expect(siteDetailsPage.stepFreeAccess).toHaveText(/Yes/);
      });
  },
);

test(
  'A user navigates back to the site details page using the cancel button',
  { tag: ['@acts-as:user1', '@asserts-on:site2'] },
  async ({ signInToSite }) => {
    await signInToSite(1, 2)
      .then(sitePage => sitePage.clickSiteDetailsCard())
      .then(siteDetailsPage => siteDetailsPage.clickEditAccessNeedsLink())
      .then(editAccessNeedsPage => editAccessNeedsPage.cancel())
      .then(siteDetailsPage => {
        expect(siteDetailsPage.title).toBeVisible();
      });
  },
);
