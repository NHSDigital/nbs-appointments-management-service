import { buildIcbName, buildRegionName } from '@e2etests/data';
import { test, expect } from '../../fixtures-v2';

test('A user views the details of a site', async ({ setup }) => {
  const { site, sitePage, testId } = await setup();

  await sitePage.clickSiteDetailsCard().then(async siteDetailsPage => {
    await expect(siteDetailsPage.title).toBeVisible();

    // Check all site details are present
    await expect(siteDetailsPage.detailsCard.title).toBeVisible();
    await expect(
      siteDetailsPage.detailsCard.summaryList.getV10Item('Name'),
    ).toHaveText(site.name);
    await expect(
      siteDetailsPage.detailsCard.summaryList.getV10Item('Address'),
    ).toHaveText(site.address);
    await expect(
      siteDetailsPage.detailsCard.summaryList.getV10Item('Latitude'),
    ).toHaveText('53.795467');
    await expect(
      siteDetailsPage.detailsCard.summaryList.getV10Item('Longitude'),
    ).toHaveText('-1.6610648');
    await expect(
      siteDetailsPage.detailsCard.summaryList.getV10Item('Phone Number'),
    ).toHaveText(site.phoneNumber);

    // Check all site reference details are present
    await expect(siteDetailsPage.referenceDetailsCard.title).toBeVisible();
    await expect(
      siteDetailsPage.referenceDetailsCard.summaryList.getV10Item('ODS code'),
    ).toHaveText(site.odsCode);
    await expect(
      siteDetailsPage.referenceDetailsCard.summaryList.getV10Item('ICB'),
    ).toHaveText(buildIcbName(testId));
    await expect(
      siteDetailsPage.referenceDetailsCard.summaryList.getV10Item('Region'),
    ).toHaveText(buildRegionName(testId));

    // Check all access needs are present
    await expect(siteDetailsPage.accessNeedsCard.title).toBeVisible();
    await expect(
      siteDetailsPage.accessNeedsCard.summaryList.getV10Item(
        'Accessible toilet',
      ),
    ).toHaveText(/No/);
    await expect(
      siteDetailsPage.accessNeedsCard.summaryList.getV10Item(
        'Braille translation service',
      ),
    ).toHaveText(/No/);
    await expect(
      siteDetailsPage.accessNeedsCard.summaryList.getV10Item(
        'Disabled car parking',
      ),
    ).toHaveText(/No/);
    await expect(
      siteDetailsPage.accessNeedsCard.summaryList.getV10Item('Car parking'),
    ).toHaveText(/No/);
    await expect(
      siteDetailsPage.accessNeedsCard.summaryList.getV10Item('Induction loop'),
    ).toHaveText(/No/);
    await expect(
      siteDetailsPage.accessNeedsCard.summaryList.getV10Item(
        'Sign language service',
      ),
    ).toHaveText(/No/);
    await expect(
      siteDetailsPage.accessNeedsCard.summaryList.getV10Item(
        'Step free access',
      ),
    ).toHaveText(/No/);
    await expect(
      siteDetailsPage.accessNeedsCard.summaryList.getV10Item('Text relay'),
    ).toHaveText(/No/);
    await expect(
      siteDetailsPage.accessNeedsCard.summaryList.getV10Item(
        'Wheelchair access',
      ),
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
