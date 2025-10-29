import { buildOdsCode } from '@e2etests/data';
import { test, expect } from '../fixtures-v2';

test('A user updates the details for a site', async ({ setUpSingleSite }) => {
  const { sitePage } = await setUpSingleSite(['system:regional-user']);

  await sitePage
    .clickSiteDetailsCard()
    .then(async siteDetailsPage =>
      siteDetailsPage.clickEditReferenceDetailsLink(),
    )
    .then(async editReferenceDetailsPage => {
      await editReferenceDetailsPage.odsCodeInput.fill('ABC123');
      await editReferenceDetailsPage.icbSelectInput.selectOption(
        'Integrated Care Board 2',
      );
      await editReferenceDetailsPage.regionSelectInput.selectOption('Region 2');

      return editReferenceDetailsPage.saveReferenceDetails();
    })
    .then(async siteDetailsPage => {
      await expect(
        siteDetailsPage.notificationBanner.getByText(
          'You have successfully updated the details for the current site.',
        ),
      ).toBeVisible();

      await expect(
        siteDetailsPage.referenceDetailsCard.summaryList.getItem('ODS code'),
      ).toHaveText('ABC123');
      await expect(
        siteDetailsPage.referenceDetailsCard.summaryList.getItem('ICB'),
      ).toHaveText('Integrated Care Board 2');
      await expect(
        siteDetailsPage.referenceDetailsCard.summaryList.getItem('Region'),
      ).toHaveText('Region 2');
    });
});

test('A user starts to update the reference details for a site then changes their mind using the back button', async ({
  setUpSingleSite,
}) => {
  const { sitePage, testId } = await setUpSingleSite(['system:regional-user']);
  await sitePage
    .clickSiteDetailsCard()
    .then(siteDetailsPage => siteDetailsPage.clickEditReferenceDetailsLink())
    .then(async editReferenceDetailsPage => {
      await editReferenceDetailsPage.odsCodeInput.fill('Some other name');

      return editReferenceDetailsPage.goBack();
    })
    .then(async siteDetailsPage => {
      await expect(siteDetailsPage.title).toBeVisible();

      await expect(
        siteDetailsPage.referenceDetailsCard.summaryList.getItem('ODS code'),
      ).not.toHaveText(/Some other name/);

      await expect(
        siteDetailsPage.referenceDetailsCard.summaryList.getItem('ODS code'),
      ).toHaveText(buildOdsCode(testId));
    });
});
