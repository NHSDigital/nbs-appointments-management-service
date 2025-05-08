import { test, expect } from '../../fixtures';
import { RootPage, SiteDetailsPage } from '@testing-page-objects';

let put: SiteDetailsPage;

test.beforeEach(async ({ page, getTestSite }) => {
  put = await new RootPage(page)
    .logInWithNhsMail()
    .then(oAuthPage => oAuthPage.signIn())
    .then(siteSelectionPage => siteSelectionPage.selectSite(getTestSite(2)))
    .then(sitePage => sitePage.clickSiteDetailsCard());
});

test(
  'A user views the details of a site',
  { tag: ['@affects:site2'] },
  async () => {
    await expect(put.title).toBeVisible();

    await expect(
      put.page.getByRole('heading', { name: 'Site reference details' }),
    ).toBeVisible();
    await expect(put.odsCode).toHaveText('ABC000032434543');
    await expect(put.icb).toHaveText('Integrated Care Board 1');
    await expect(put.region).toHaveText('Region 1');

    await expect(
      put.page.getByRole('heading', { name: 'Site details' }),
    ).toBeVisible();
    await expect(put.address).toHaveText('Site 2 Address');
    await expect(put.latitude).toHaveText('53.742');
    await expect(put.longitude).toHaveText('0.32445345');
    await expect(put.phoneNumber).toHaveText('0118 999 88199 9119 725 3');

    await expect(
      put.page.getByRole('heading', { name: 'Access needs' }),
    ).toBeVisible();
    await expect(put.accessibleToilet).toHaveText('Yes');
    await expect(put.brailleTranslationService).toHaveText('No');
    await expect(put.disabledCarParking).toHaveText('No');
    await expect(put.carParking).toHaveText('No');
    await expect(put.inductionLoop).toHaveText('No');
    await expect(put.signLanguageService).toHaveText('No');
    await expect(put.stepFreeAccess).toHaveText('No');
    await expect(put.textRelay).toHaveText('No');
    await expect(put.wheelchairAccess).toHaveText('No');

    await expect(
      put.page.getByRole('heading', { name: 'Information for citizens' }),
    ).toBeVisible();
    await expect(
      put.page.getByText('Mock information for citizens about site 2'),
    ).toBeVisible();
  },
);
