import { expect } from '../../fixtures';
import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';
import { Site } from '@types';

export default class SiteDetailsPage extends RootPage {
  readonly title: Locator;
  readonly editSiteAccessibilitiesButton: Locator;
  readonly editSiteDetailsButton: Locator;
  readonly editSiteReferenceDetailsButton: Locator;
  readonly editInformationCitizenButton: Locator;
  readonly closeNotificationBannerButton: Locator;
  readonly headerMsg = 'Manage Site';
  readonly siteDetailsheaderMsg = 'Site details';
  readonly referenceDetailsheaderMsg = 'Site reference details';
  readonly accessNeedsheaderMsg = 'Access needs';
  readonly informationForCitizensheaderMsg = 'Information for citizens';

  readonly addressLabel = 'Address';
  readonly latitudeLabel = 'Latitude';
  readonly longitudeLabel = 'Longitude';
  readonly phoneNumberLabel = 'Phone Number';
  readonly odsCodeLabel = 'ODS code';
  readonly icbLabel = 'ICB';
  readonly regionLabel = 'Region';

  readonly siteDetails: Site;

  readonly informationSuccessBanner =
    "You have successfully updated the current site's information.";

  readonly detailsSuccessBanner =
    'You have successfully updated the details for the current site.';

  readonly referenceDetailsSuccessBanner =
    'You have successfully updated the reference details for the current site.';

  constructor(page: Page, siteDetails: Site) {
    super(page);

    this.siteDetails = siteDetails;

    this.title = page.getByRole('heading', {
      name: 'Site details',
    });
    this.editSiteReferenceDetailsButton = page.getByRole('link', {
      name: 'Edit site reference details',
    });
    this.editSiteDetailsButton = page.getByRole('link', {
      name: 'Edit site details',
    });
    this.editSiteAccessibilitiesButton = page.getByRole('link', {
      name: 'Edit access needs',
    });
    this.editInformationCitizenButton = page.getByRole('link', {
      name: 'Edit information for citizens',
    });

    this.closeNotificationBannerButton = page.getByRole('button', {
      name: 'Close',
    });
  }

  async accessibilityIsTrue(accessibilityName: string) {
    await expect(
      this.page
        .getByRole('row')
        .filter({ has: this.page.getByText(accessibilityName) })
        .getByText('Yes'),
    ).toBeVisible();
  }

  async verifyInformationSaved(information: string) {
    await expect(
      this.page.getByText(`${this.informationSuccessBanner}`),
    ).toBeVisible();
    await expect(this.page.getByText(information)).toBeVisible();
    await this.closeNotificationBannerButton.click();
    await expect(this.closeNotificationBannerButton).not.toBeVisible();
  }

  async verifyDetailsNotificationVisibility(shown: boolean) {
    if (!shown) {
      await expect(
        this.page.getByText(`${this.detailsSuccessBanner}`),
      ).not.toBeVisible();
    } else {
      await expect(
        this.page.getByText(`${this.detailsSuccessBanner}`),
      ).toBeVisible();
      await this.closeNotificationBannerButton.click();
      await expect(this.closeNotificationBannerButton).not.toBeVisible();
    }
  }

  async verifyReferenceDetailsNotificationVisibility(shown: boolean) {
    if (!shown) {
      await expect(
        this.page.getByText(`${this.referenceDetailsSuccessBanner}`),
      ).not.toBeVisible();
    } else {
      await expect(
        this.page.getByText(`${this.referenceDetailsSuccessBanner}`),
      ).toBeVisible();
      await this.closeNotificationBannerButton.click();
      await expect(this.closeNotificationBannerButton).not.toBeVisible();
    }
  }

  async verifyCoreDetailsContent(
    address: string,
    lat: string,
    long: string,
    phoneNumber: string,
  ) {
    await this.verifySummaryListItemContentValue(this.addressLabel, address);
    await this.verifySummaryListItemContentValue(this.latitudeLabel, lat);
    await this.verifySummaryListItemContentValue(this.longitudeLabel, long);
    await this.verifySummaryListItemContentValue(
      this.phoneNumberLabel,
      phoneNumber,
    );
  }

  async verifyReferenceDetailsContent(
    odsCode: string,
    icb: string,
    region: string,
  ) {
    await this.verifySummaryListItemContentValue(this.odsCodeLabel, odsCode);

    //TODO refactor using seeder well-known codes
    let expectedICBDisplayValue = '';
    switch (icb) {
      case 'ICB1':
        expectedICBDisplayValue = 'Integrated Care Board 1';
        break;
      case 'ICB2':
        expectedICBDisplayValue = 'Integrated Care Board 2';
        break;
      default:
        expectedICBDisplayValue = icb;
        break;
    }

    await this.verifySummaryListItemContentValue(
      this.icbLabel,
      expectedICBDisplayValue,
    );

    //TODO refactor using seeder well-known codes
    let expectedRegionDisplayValue = '';
    switch (region) {
      case 'R1':
        expectedRegionDisplayValue = 'Region 1';
        break;
      case 'R2':
        expectedRegionDisplayValue = 'Region 2';
        break;
      default:
        expectedRegionDisplayValue = region;
        break;
    }

    await this.verifySummaryListItemContentValue(
      this.regionLabel,
      expectedRegionDisplayValue,
    );
  }

  async verifySummaryListItemContentValue(title: string, value: string) {
    const listitem = this.page.getByRole('listitem', {
      name: `${title}-listitem`,
    });
    await expect(listitem).toBeVisible();

    const dt = listitem.getByLabel(`${title}-term`);
    await expect(dt).toBeVisible();
    await expect(dt).toHaveText(title);

    const dd = listitem.getByLabel(`${title}-description`);
    await expect(dd).toBeVisible();
    await expect(dd).toHaveText(value);
  }

  async verifyInformationNotSaved(
    oldInformation: string,
    newInformation: string,
  ) {
    //Will uncomment below line once related defect is fixed.
    //await expect(this.page.getByText(`${this.successBanner}`)).not.toBeVisible();
    await expect(this.page.getByText(oldInformation)).toBeVisible();
    await expect(this.page.getByText(newInformation)).not.toBeVisible();
  }

  async verifyEditButtonNotVisible() {
    await expect(this.editInformationCitizenButton).not.toBeVisible();
    await expect(this.editSiteAccessibilitiesButton).not.toBeVisible();
  }

  async verifyEditButtonToBeVisible() {
    await expect(this.editInformationCitizenButton).toBeVisible();
    await expect(this.editSiteAccessibilitiesButton).toBeVisible();
  }

  async verifySitePage() {
    await expect(
      this.page.getByRole('heading', { name: `${this.headerMsg}` }),
    ).toBeVisible();
    await expect(
      this.page.getByRole('heading', { name: `${this.siteDetails.name}` }),
    ).toBeVisible();

    await expect(
      this.page.getByRole('heading', { name: `${this.siteDetailsheaderMsg}` }),
    ).toBeVisible();

    await this.verifyCoreDetailsContent(
      this.siteDetails.address,
      this.siteDetails.location.coordinates[0].toString(),
      this.siteDetails.location.coordinates[1].toString(),
      this.siteDetails.phoneNumber,
    );

    await expect(
      this.page.getByRole('heading', {
        name: `${this.referenceDetailsheaderMsg}`,
      }),
    ).toBeVisible();

    await this.verifyReferenceDetailsContent(
      this.siteDetails.odsCode,
      this.siteDetails.integratedCareBoard,
      this.siteDetails.region,
    );

    await expect(
      this.page.getByRole('heading', { name: `${this.accessNeedsheaderMsg}` }),
    ).toBeVisible();

    await expect(
      this.page.getByRole('heading', {
        name: `${this.informationForCitizensheaderMsg}`,
      }),
    ).toBeVisible();
  }
}
