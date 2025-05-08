import { EditAccessNeedsPage, RootPage } from '@testing-page-objects';
import { test, expect } from '../../fixtures';

let put: EditAccessNeedsPage;

test.beforeEach(async ({ page, getTestSite }) => {
  put = await new RootPage(page)
    .logInWithNhsMail()
    .then(oAuthPage => oAuthPage.signIn())
    .then(siteSelectionPage => siteSelectionPage.selectSite(getTestSite(2)))
    .then(sitePage => sitePage.clickSiteDetailsCard())
    .then(siteDetailsPage => siteDetailsPage.clickEditAccessNeedsLink());
});

test(
  'A user updates the access needs for a site',
  { tag: ['affects:site2'] },
  async ({ page }) => {
    await put.checkboxes.accessibleToilet.check();
    await put.checkboxes.stepFreeAccess.check();
    const siteDetailsPage = await put.saveSiteDetails();

    const expectedNotification =
      'You have successfully updated the access needs for the current site.';
    await expect(
      siteDetailsPage.notificationBanner.getByText(expectedNotification),
    ).toBeVisible();

    await siteDetailsPage.dismissNotificationBannerButton.click();
    await expect(siteDetailsPage.notificationBanner).not.toBeVisible();

    put = await siteDetailsPage.clickEditAccessNeedsLink();
    await expect(put.checkboxes.accessibleToilet).toBeChecked();
    await expect(put.checkboxes.stepFreeAccess).not.toBeChecked();

    await put.page.reload();
    await page.waitForURL(
      `**/site/${put.site.id}/details/edit-accessibilities`,
    );

    await expect(put.checkboxes.accessibleToilet).toBeChecked();
    await expect(put.checkboxes.stepFreeAccess).not.toBeChecked();
  },
);

test('A user navigates back to the site details page using the back link', async () => {
  const siteDetailsPage = await put.goBack();

  await expect(siteDetailsPage.title).toBeVisible();
});
