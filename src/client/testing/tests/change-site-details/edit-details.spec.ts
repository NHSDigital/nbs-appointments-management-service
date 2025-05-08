import { EditDetailsPage, LoginPage } from '@testing-page-objects';
import { test, expect } from '../../fixtures';

let put: EditDetailsPage;

test.beforeEach(async ({ page, getTestSite }) => {
  put = await new LoginPage(page)
    .logInWithNhsMail()
    .then(oAuthPage => oAuthPage.signIn())
    .then(siteSelectionPage => siteSelectionPage.selectSite(getTestSite(2)))
    .then(sitePage => sitePage.clickSiteDetailsCard())
    .then(siteDetailsPage => siteDetailsPage.clickEditDetailsLink());
});

test(
  'A user updates the details for a site',
  { tag: ['@affects:site2'] },
  async () => {
    await put.addressInput.fill('One House,\nOne Road,\nOne Town');
    await put.longitudeInput.fill('0.32445345');
    await put.latitudeInput.fill('53.742');
    await put.phoneNumberInput.fill('0118 999 88199 9119 725 3');

    const siteDetailsPage = await put.saveSiteDetails();
    await expect(
      siteDetailsPage.notificationBanner.getByText(
        'You have successfully updated the details for the current site.',
      ),
    ).toBeVisible();

    await expect(siteDetailsPage.address).toHaveText(
      'One House, One Road, One Town',
    );
    await expect(siteDetailsPage.latitude).toHaveText('0.32445345');
    await expect(siteDetailsPage.longitude).toHaveText('53.742');
    await expect(siteDetailsPage.phoneNumber).toHaveText(
      '0118 999 88199 9119 725 3',
    );

    put = await siteDetailsPage.clickEditDetailsLink();

    await expect(put.addressInput).toHaveValue(
      'One House,\nOne Road,\nOne Town',
    );
    await expect(put.longitudeInput).toHaveValue('0.32445345');
    await expect(put.latitudeInput).toHaveValue('53.742');
    await expect(put.phoneNumberInput).toHaveValue('0118 999 88199 9119 725 3');
  },
);

test('A user navigates back to the site details page using the back link', async () => {
  const siteDetailsPage = await put.goBack();

  await expect(siteDetailsPage.title).toBeVisible();
});
