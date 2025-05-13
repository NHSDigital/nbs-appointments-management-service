import { test, expect } from '../../fixtures';

test(
  'A user updates the details for a site',
  { tag: ['@acts-as:user1', '@alters:site2'] },
  async ({ signInToSite }) => {
    await signInToSite(1, 2)
      .then(sitePage => sitePage.clickSiteDetailsCard())
      .then(siteDetailsPage => siteDetailsPage.clickEditDetailsLink())
      .then(async editDetailsPage => {
        await editDetailsPage.addressInput.fill(
          'One House,\nOne Road,\nOne Town',
        );
        await editDetailsPage.longitudeInput.fill('53.742');
        await editDetailsPage.latitudeInput.fill('0.32445345');
        await editDetailsPage.phoneNumberInput.fill(
          '0118 999 88199 9119 725 3',
        );

        return editDetailsPage.saveSiteDetails();
      })
      .then(async siteDetailsPage => {
        await expect(
          siteDetailsPage.notificationBanner.getByText(
            'You have successfully updated the details for the current site.',
          ),
        ).toBeVisible();

        await expect(siteDetailsPage.address).toHaveText(
          /One House, One Road, One Town/,
        );
        await expect(siteDetailsPage.latitude).toHaveText(/0.32445345/);
        await expect(siteDetailsPage.longitude).toHaveText(/53.742/);
        await expect(siteDetailsPage.phoneNumber).toHaveText(
          /0118 999 88199 9119 725 3/,
        );
      });
  },
);

test(
  'A user navigates back to the site details page using the back link',
  { tag: ['@acts-as:user1', '@asserts-on:site2'] },
  async ({ signInToSite }) => {
    await signInToSite(1, 2)
      .then(sitePage => sitePage.clickSiteDetailsCard())
      .then(siteDetailsPage => siteDetailsPage.clickEditDetailsLink())
      .then(editDetailsPage => editDetailsPage.goBack())
      .then(async siteDetailsPage => {
        await expect(siteDetailsPage.title).toBeVisible();
      });
  },
);
