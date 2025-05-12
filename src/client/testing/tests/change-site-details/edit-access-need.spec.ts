import { LoginPage } from '@testing-page-objects';
import { test, expect } from '../../fixtures';

test(
  'A user updates the access needs for a site',
  { tag: ['@affects:site2'] },
  async ({ page, getTestSite }) => {
    await new LoginPage(page)
      .logInWithNhsMail()
      .then(oAuthPage => oAuthPage.signIn())
      .then(siteSelectionPage => siteSelectionPage.selectSite(getTestSite(2)))
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

        return await siteDetailsPage.clickEditAccessNeedsLink();
      })
      .then(async editAccessNeedsPage => {
        await expect(
          editAccessNeedsPage.checkboxes.accessibleToilet,
        ).toBeChecked();
        await expect(
          editAccessNeedsPage.checkboxes.stepFreeAccess,
        ).not.toBeChecked();

        await editAccessNeedsPage.page.reload();
        await page.waitForURL(
          `**/site/${editAccessNeedsPage.site.id}/details/edit-accessibilities`,
        );

        await expect(
          editAccessNeedsPage.checkboxes.accessibleToilet,
        ).toBeChecked();
        await expect(
          editAccessNeedsPage.checkboxes.stepFreeAccess,
        ).not.toBeChecked();
      });
  },
);

test('A user navigates back to the site details page using the back link', async ({
  page,
  getTestSite,
}) => {
  await new LoginPage(page)
    .logInWithNhsMail()
    .then(oAuthPage => oAuthPage.signIn())
    .then(siteSelectionPage => siteSelectionPage.selectSite(getTestSite(2)))
    .then(sitePage => sitePage.clickSiteDetailsCard())
    .then(siteDetailsPage => siteDetailsPage.clickEditAccessNeedsLink())
    .then(editAccessNeedsPage => editAccessNeedsPage.goBack())
    .then(siteDetailsPage => {
      expect(siteDetailsPage.title).toBeVisible();
    });
});
