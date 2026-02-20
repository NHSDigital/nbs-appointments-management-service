import { buildSiteName } from '@e2etests/data';
import { test, expect } from '../../fixtures-v2';

test('A user updates the details for a site', async ({ setUpSingleSite }) => {
  const { sitePage } = await setUpSingleSite();

  await sitePage
    .clickSiteDetailsCard()
    .then(async siteDetailsPage => siteDetailsPage.clickEditDetailsLink())
    .then(async editDetailsPage => {
      await editDetailsPage.nameInput.fill('Updated Site Name');

      await editDetailsPage.addressInput.fill(
        'One House,\nOne Road,\nOne Town',
      );

      await editDetailsPage.latitudeInput.fill('53.742');
      await editDetailsPage.longitudeInput.fill('0.32445345');
      await editDetailsPage.phoneNumberInput.fill('0118 999 88199 9119 725 3');

      return editDetailsPage.saveSiteDetails();
    })
    .then(async siteDetailsPage => {
      await expect(
        siteDetailsPage.notificationBanner.getByText(
          'You have successfully updated the details for the current site.',
        ),
      ).toBeVisible();

      await expect(
        siteDetailsPage.detailsCard.summaryList.getV10Item('Name'),
      ).toHaveText('Updated Site Name');
      await expect(
        siteDetailsPage.detailsCard.summaryList.getV10Item('Address'),
      ).toHaveText(/One House, One Road, One Town/);

      await expect(
        siteDetailsPage.detailsCard.summaryList.getV10Item('Longitude'),
      ).toHaveText(/0.32445345/);
      await expect(
        siteDetailsPage.detailsCard.summaryList.getV10Item('Latitude'),
      ).toHaveText(/53.742/);
      await expect(
        siteDetailsPage.detailsCard.summaryList.getV10Item('Phone Number'),
      ).toHaveText(/0118 999 88199 9119 725 3/);
    });
});

test('A user starts to update the details for a site then changes their mind using the back button', async ({
  setUpSingleSite,
}) => {
  const { sitePage, testId } = await setUpSingleSite();
  await sitePage
    .clickSiteDetailsCard()
    .then(siteDetailsPage => siteDetailsPage.clickEditDetailsLink())
    .then(async editDetailsPage => {
      await editDetailsPage.nameInput.fill('Some other name');

      return editDetailsPage.goBack();
    })
    .then(async siteDetailsPage => {
      await expect(siteDetailsPage.title).toBeVisible();

      await expect(
        siteDetailsPage.detailsCard.summaryList.getV10Item('Name'),
      ).not.toHaveText(/Some other name/);

      await expect(
        siteDetailsPage.detailsCard.summaryList.getV10Item('Name'),
      ).toHaveText(buildSiteName(testId));
    });
});
