import { test, expect } from '../../fixtures-v2';

test('A user updates the access needs for a site', async ({
  setUpSingleSite,
}) => {
  const { sitePage } = await setUpSingleSite();

  await sitePage
    .clickSiteDetailsCard()
    .then(async siteDetailsPage => siteDetailsPage.clickEditAccessNeedsLink())
    .then(async editAccessNeedsPage => {
      await expect(editAccessNeedsPage.title).toBeVisible();

      await editAccessNeedsPage.checkboxes.accessibleToilet.check();
      await editAccessNeedsPage.checkboxes.stepFreeAccess.check();

      return editAccessNeedsPage.saveSiteDetails();
    })
    .then(async siteDetailsPage => {
      await expect(
        siteDetailsPage.notificationBanner.getByText(
          `You have successfully updated the access needs for the current site.`,
        ),
      ).toBeVisible();

      await expect(
        siteDetailsPage.accessNeedsCard.summaryList.getItem(
          'Accessible toilet',
        ),
      ).toHaveText('Yes');
      await expect(
        siteDetailsPage.accessNeedsCard.summaryList.getItem('Step free access'),
      ).toHaveText('Yes');
    });
});

test('A user starts to update the citizen information then changes their mind using the back button', async ({
  setUpSingleSite,
}) => {
  const { sitePage } = await setUpSingleSite();
  await sitePage
    .clickSiteDetailsCard()
    .then(siteDetailsPage => siteDetailsPage.clickEditAccessNeedsLink())
    .then(async editAccessNeedsPage => {
      await expect(editAccessNeedsPage.title).toBeVisible();

      await editAccessNeedsPage.checkboxes.accessibleToilet.check();

      return await editAccessNeedsPage.cancel();
    })
    .then(async siteDetailsPage => {
      await expect(siteDetailsPage.title).toBeVisible();

      await expect(
        siteDetailsPage.accessNeedsCard.summaryList.getItem(
          'Accessible toilet',
        ),
      ).toHaveText('No');
    });
});
