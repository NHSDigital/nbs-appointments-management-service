import { test, expect } from '../../fixtures-v2';

test.describe.configure({ mode: 'serial' });

test('A site manager cannot edit site status when the feature toggle is disabled', async ({
  setUpSingleSite,
}) => {
  await setUpSingleSite({
    features: [{ name: 'SiteStatus', enabled: false }],
  })
    .then(({ sitePage }) => {
      return sitePage.clickSiteDetailsCard();
    })
    .then(async siteDetailsPage => {
      await expect(
        siteDetailsPage.detailsCard.summaryList.getV10Item('Site status'),
      ).not.toBeVisible();

      await expect(siteDetailsPage.detailsCard.editLinks[1]).not.toBeVisible();
    });
});

test('A site manager takes an online site offline', async ({
  setUpSingleSite,
}) => {
  await setUpSingleSite({
    features: [{ name: 'SiteStatus', enabled: true }],
  })
    .then(({ sitePage }) => {
      return sitePage.clickSiteDetailsCard();
    })
    .then(async siteDetailsPage => {
      return siteDetailsPage.clickEditSiteStatusLink();
    })
    .then(async editSiteStatusPage => {
      await expect(editSiteStatusPage.title).toBeVisible();

      await expect(
        editSiteStatusPage.siteStatusSummary.getV10Item('Current site status'),
      ).toHaveText('Online');

      await expect(editSiteStatusPage.keepSiteOnlineRadio).toBeChecked();
      await editSiteStatusPage.setSiteOfflineRadio.check();

      return editSiteStatusPage.saveChanges();
    })
    .then(async siteDetailsPage => {
      await expect(
        siteDetailsPage.notificationBanner.getByText(
          'The site is now offline and will not be available for appointments.',
        ),
      ).toBeVisible();

      await expect(
        siteDetailsPage.detailsCard.summaryList.getV10Item('Status'),
      ).toHaveText('Offline');
    });
});

test('A site manager takes an offline site online', async ({
  setUpSingleSite,
}) => {
  await setUpSingleSite({
    features: [{ name: 'SiteStatus', enabled: true }],
    siteConfig: { status: 'Offline' },
  })
    .then(({ sitePage }) => {
      return sitePage.clickSiteDetailsCard();
    })
    .then(async siteDetailsPage => {
      return siteDetailsPage.clickEditSiteStatusLink();
    })
    .then(async editSiteStatusPage => {
      await expect(editSiteStatusPage.title).toBeVisible();

      await expect(
        editSiteStatusPage.siteStatusSummary.getV10Item('Current site status'),
      ).toHaveText('Offline');

      await expect(editSiteStatusPage.keepSiteOfflineRadio).toBeChecked();
      await editSiteStatusPage.setSiteOnlineRadio.check();

      return editSiteStatusPage.saveChanges();
    })
    .then(async siteDetailsPage => {
      await expect(
        siteDetailsPage.notificationBanner.getByText(
          'The site is now online and is available for appointments.',
        ),
      ).toBeVisible();

      await expect(
        siteDetailsPage.detailsCard.summaryList.getV10Item('Status'),
      ).toHaveText('Online');
    });
});

test('A user starts to update site status then changes their mind using the back button', async ({
  setUpSingleSite,
}) => {
  await setUpSingleSite({ features: [{ name: 'SiteStatus', enabled: true }] })
    .then(({ sitePage }) => {
      return sitePage.clickSiteDetailsCard();
    })
    .then(async siteDetailsPage => {
      return siteDetailsPage.clickEditSiteStatusLink();
    })
    .then(async editSiteStatusPage => {
      await expect(editSiteStatusPage.title).toBeVisible();

      await expect(
        editSiteStatusPage.siteStatusSummary.getV10Item('Current site status'),
      ).toHaveText('Online');

      await expect(editSiteStatusPage.keepSiteOnlineRadio).toBeChecked();
      await editSiteStatusPage.setSiteOfflineRadio.check();

      return editSiteStatusPage.goBack();
    })
    .then(async siteDetailsPage => {
      await expect(
        siteDetailsPage.detailsCard.summaryList.getV10Item('Status'),
      ).toHaveText('Online');
    });
});
