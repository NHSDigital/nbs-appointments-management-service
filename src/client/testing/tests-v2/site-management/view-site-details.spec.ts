import {
  buildAddress,
  buildIcbName,
  buildOdsCode,
  buildPhoneNumber,
  buildRegionName,
  buildSiteName,
} from '@e2etests/data';
import { test, expect } from '../../fixtures-v2';
//TEFsdgfsdf
test('A user views the details of a site', async ({ setUpSingleSite }) => {
  const { sitePage, testId } = await setUpSingleSite();

  await sitePage.clickSiteDetailsCard().then(async siteDetailsPage => {
    await expect(siteDetailsPage.title).toBeVisible();

    // Check all site details are present
    await expect(siteDetailsPage.detailsCard.title).toBeVisible();
    await expect(
      siteDetailsPage.detailsCard.summaryList.getItem('Name'),
    ).toHaveText(buildSiteName(testId));
    await expect(
      siteDetailsPage.detailsCard.summaryList.getItem('Address'),
    ).toHaveText(buildAddress(testId));
    await expect(
      siteDetailsPage.detailsCard.summaryList.getItem('Latitude'),
    ).toHaveText('53.795467');
    await expect(
      siteDetailsPage.detailsCard.summaryList.getItem('Longitude'),
    ).toHaveText('-1.6610648');
    await expect(
      siteDetailsPage.detailsCard.summaryList.getItem('Phone Number'),
    ).toHaveText(buildPhoneNumber(testId));

    // Check all site reference details are present
    await expect(siteDetailsPage.referenceDetailsCard.title).toBeVisible();
    await expect(
      siteDetailsPage.referenceDetailsCard.summaryList.getItem('ODS code'),
    ).toHaveText(buildOdsCode(testId));
    await expect(
      siteDetailsPage.referenceDetailsCard.summaryList.getItem('ICB'),
    ).toHaveText(buildIcbName(testId));
    await expect(
      siteDetailsPage.referenceDetailsCard.summaryList.getItem('Region'),
    ).toHaveText(buildRegionName(testId));

    // Check all access needs are present
    await expect(siteDetailsPage.accessNeedsCard.title).toBeVisible();
    await expect(
      siteDetailsPage.accessNeedsCard.summaryList.getItem('Accessible toilet'),
    ).toHaveText(/No/);
    await expect(
      siteDetailsPage.accessNeedsCard.summaryList.getItem(
        'Braille translation service',
      ),
    ).toHaveText(/No/);
    await expect(
      siteDetailsPage.accessNeedsCard.summaryList.getItem(
        'Disabled car parking',
      ),
    ).toHaveText(/No/);
    await expect(
      siteDetailsPage.accessNeedsCard.summaryList.getItem('Car parking'),
    ).toHaveText(/No/);
    await expect(
      siteDetailsPage.accessNeedsCard.summaryList.getItem('Induction loop'),
    ).toHaveText(/No/);
    await expect(
      siteDetailsPage.accessNeedsCard.summaryList.getItem(
        'Sign language service',
      ),
    ).toHaveText(/No/);
    await expect(
      siteDetailsPage.accessNeedsCard.summaryList.getItem('Step free access'),
    ).toHaveText(/No/);
    await expect(
      siteDetailsPage.accessNeedsCard.summaryList.getItem('Text relay'),
    ).toHaveText(/No/);
    await expect(
      siteDetailsPage.accessNeedsCard.summaryList.getItem('Wheelchair access'),
    ).toHaveText(/No/);

    // Check information for citizens is present
    await expect(
      siteDetailsPage.informationForCitizensCard.title,
    ).toBeVisible();
    await expect(siteDetailsPage.informationForCitizensCard.content).toHaveText(
      /This is some placeholder information for citizens./,
    );
  });
});
