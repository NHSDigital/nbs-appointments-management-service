import { test, expect } from '../../fixtures';

test(
  'A user updates the citizen information for a site',
  { tag: ['@acts-as:user1', '@alters:site2'] },
  async ({ signInToSite }) => {
    await signInToSite(1, 2)
      .then(sitePage => sitePage.clickSiteDetailsCard())
      .then(siteDetailsPage =>
        siteDetailsPage.clickEditInformationForCitizensLink(),
      )
      .then(async editInformationForCitizensPage => {
        await expect(editInformationForCitizensPage.title).toBeVisible();
        await expect(editInformationForCitizensPage.infoTextArea).toHaveValue(
          'Mock information for citizens about site 2',
        );

        await editInformationForCitizensPage.infoTextArea.clear();
        await editInformationForCitizensPage.infoTextArea.fill(
          'UPDATED INFORMATION',
        );

        return editInformationForCitizensPage.saveCitizenInformation();
      })
      .then(async siteDetailsPage => {
        await expect(
          siteDetailsPage.notificationBanner.getByText(
            `You have successfully updated the current site's information.`,
          ),
        ).toBeVisible();

        await siteDetailsPage.dismissNotificationBannerButton.click();
        await expect(siteDetailsPage.notificationBanner).not.toBeVisible();

        await expect(siteDetailsPage.informationForCitizensCard).toHaveText(
          /UPDATED INFORMATION/,
        );
      });
  },
);

test(
  'A user navigates back to the site details page using the cancel button',
  { tag: ['@acts-as:user1', '@asserts-on:site2'] },
  async ({ signInToSite }) => {
    await signInToSite(1, 2)
      .then(sitePage => sitePage.clickSiteDetailsCard())
      .then(siteDetailsPage =>
        siteDetailsPage.clickEditInformationForCitizensLink(),
      )
      .then(editInformationForCitizensPage =>
        editInformationForCitizensPage.cancel(),
      )
      .then(async siteDetailsPage => {
        await expect(siteDetailsPage.title).toBeVisible();
      });
  },
);
