import { test, expect } from '../../fixtures';

test(
  'A user views the details of a site',
  { tag: ['@acts-as:user1', '@asserts-on:site1'] },
  async ({ signInToSite }) => {
    await signInToSite()
      .then(sitePage => sitePage.clickSiteDetailsCard())
      .then(async siteDetailsPage => {
        await expect(siteDetailsPage.title).toBeVisible();

        await expect(
          siteDetailsPage.page.getByRole('heading', {
            name: 'Site reference details',
          }),
        ).toBeVisible();
        await expect(siteDetailsPage.odsCode).toHaveText(/ABC01/);
        await expect(siteDetailsPage.icb).toHaveText(/Integrated Care Board 1/);
        await expect(siteDetailsPage.region).toHaveText(/Region 1/);

        await expect(
          siteDetailsPage.page.getByRole('heading', { name: 'Site details' }),
        ).toBeVisible();
        await expect(siteDetailsPage.address).toHaveText(
          /Pudsey, Leeds, LS28 7BR/,
        );
        await expect(siteDetailsPage.latitude).toHaveText(/53.795467/);
        await expect(siteDetailsPage.longitude).toHaveText(/-1.6610648/);
        await expect(siteDetailsPage.phoneNumber).toHaveText(/0113 1111111/);

        await expect(
          siteDetailsPage.page.getByRole('heading', { name: 'Access needs' }),
        ).toBeVisible();
        await expect(siteDetailsPage.accessibleToilet).toHaveText(/No/);
        await expect(siteDetailsPage.brailleTranslationService).toHaveText(
          /No/,
        );
        await expect(siteDetailsPage.disabledCarParking).toHaveText(/No/);
        await expect(siteDetailsPage.carParking).toHaveText(/No/);
        await expect(siteDetailsPage.inductionLoop).toHaveText(/No/);
        await expect(siteDetailsPage.signLanguageService).toHaveText(/No/);
        await expect(siteDetailsPage.stepFreeAccess).toHaveText(/No/);
        await expect(siteDetailsPage.textRelay).toHaveText(/No/);
        await expect(siteDetailsPage.wheelchairAccess).toHaveText(/No/);

        await expect(
          siteDetailsPage.page.getByRole('heading', {
            name: 'Information for citizens',
          }),
        ).toBeVisible();
        await expect(
          siteDetailsPage.page.getByText(
            /Mock information for citizens about site 1/,
          ),
        ).toBeVisible();
      });
  },
);
