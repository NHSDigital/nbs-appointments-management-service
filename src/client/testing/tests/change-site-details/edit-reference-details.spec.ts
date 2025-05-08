import { EditReferenceDetailsPage, RootPage } from '@testing-page-objects';
import { test, expect } from '../../fixtures';

let put: EditReferenceDetailsPage;

test.beforeEach(async ({ page, getTestSite }) => {
  put = await new RootPage(page)
    .logInWithNhsMail()
    .then(oAuthPage => oAuthPage.signIn())
    .then(siteSelectionPage => siteSelectionPage.selectSite(getTestSite(2)))
    .then(sitePage => sitePage.clickSiteDetailsCard())
    .then(siteDetailsPage => siteDetailsPage.clickEditReferenceDetailsLink());
});

test(
  'A user updates the reference details for a site',
  { tag: ['@affects:site2'] },
  async () => {
    await put.odsCodeInput.fill('ABC000032434543');
    await put.icbSelectInput.selectOption('Integrated Care Board 1');
    await put.regionSelectInput.selectOption('Region 1');

    const siteDetailsPage = await put.saveReferenceDetails();

    const expectedNotification =
      'You have successfully updated the reference details for the current site.';
    await expect(
      siteDetailsPage.notificationBanner.getByText(expectedNotification),
    ).toBeVisible();
    await siteDetailsPage.dismissNotificationBannerButton.click();
    await expect(siteDetailsPage.notificationBanner).not.toBeVisible();

    await expect(siteDetailsPage.odsCode).toHaveText('ABC000032434543');
    await expect(siteDetailsPage.icb).toHaveText('Integrated Care Board 1');
    await expect(siteDetailsPage.region).toHaveText('Region 1');

    put = await siteDetailsPage.clickEditReferenceDetailsLink();

    await expect(put.odsCodeInput).toHaveValue('ABC000032434543');
    await expect(put.icbSelectInput).toHaveValue('ICB1');
    await expect(put.regionSelectInput).toHaveValue('R1');
  },
);

test('A user navigates back to the site details page using the back link', async () => {
  const siteDetailsPage = await put.goBack();

  await expect(siteDetailsPage.title).toBeVisible();
});
