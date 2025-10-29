import { test, expect } from '../fixtures-v2';

test('A user updates the citizen information for a site', async ({
  setUpSingleSite,
}) => {
  const { sitePage } = await setUpSingleSite();

  await sitePage
    .clickSiteDetailsCard()
    .then(async siteDetailsPage =>
      siteDetailsPage.clickEditInformationForCitizensLink(),
    )
    .then(async editInformationForCitizensPage => {
      await expect(editInformationForCitizensPage.title).toBeVisible();
      await expect(editInformationForCitizensPage.infoTextArea).toHaveValue(
        'This is some placeholder information for citizens.',
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

      await expect(
        siteDetailsPage.informationForCitizensCard.content,
      ).toHaveText(/UPDATED INFORMATION/);
    });
});

test('A user starts to update the citizen information then changes their mind using the back button', async ({
  setUpSingleSite,
}) => {
  const { sitePage } = await setUpSingleSite();
  await sitePage
    .clickSiteDetailsCard()
    .then(siteDetailsPage =>
      siteDetailsPage.clickEditInformationForCitizensLink(),
    )
    .then(async editInformationForCitizensPage => {
      await editInformationForCitizensPage.infoTextArea.clear();
      await editInformationForCitizensPage.infoTextArea.fill(
        'UPDATED INFORMATION',
      );

      return editInformationForCitizensPage.cancel();
    })
    .then(async siteDetailsPage => {
      await expect(siteDetailsPage.title).toBeVisible();

      await expect(
        siteDetailsPage.informationForCitizensCard.content,
      ).toHaveText(/This is some placeholder information for citizens./);
    });
});
