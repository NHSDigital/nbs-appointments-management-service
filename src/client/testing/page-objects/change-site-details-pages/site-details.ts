import { expect } from '../../fixtures';
import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';

export default class SiteDetailsPage extends RootPage {
  readonly title: Locator;
  readonly editSiteAttributesButton: Locator;
  readonly editSiteDetailsButton: Locator;
  readonly editInformationCitizenButton: Locator;
  readonly closeNotificationBannerButton: Locator;
  readonly headerMsg = 'Manage Site';
  readonly siteDetailsheaderMsg = 'Site Details';
  readonly adminDetailsheaderMsg = 'Admin Details';
  readonly accessNeedsheaderMsg = 'Access needs';
  readonly informationForCitizensheaderMsg = 'Information for citizens';

  readonly addressLabel = 'Address';
  readonly latitudeLabel = 'Latitude';
  readonly longitudeLabel = 'Longitude';
  readonly phoneNumberLabel = 'Phone Number';

  readonly defaultSiteName = 'Church Lane Pharmacy';
  readonly defaultAddress = 'Pudsey, Leeds, LS28 7LD';
  readonly defaultLatitude = '-1.66382134';
  readonly defaultLongitude = '53.79628754';
  readonly defaultPhoneNumber = '01132222222';

  readonly odsCodeLabel = 'ODS code';
  readonly icbLabel = 'ICB';
  readonly regionLabel = 'Region';

  readonly defaultODSCode = 'ABC02';
  readonly defaultICB = '	Integrated Care Board 2';
  readonly defaultRegion = 'Region 2';

  readonly informationSuccessBanner =
    "You have successfully updated the current site's information.";

  readonly detailsSuccessBanner =
    'You have successfully updated the details for the current site.';

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Site details',
    });
    this.editSiteDetailsButton = page.getByRole('link', {
      name: 'Edit site details',
    });
    this.editSiteAttributesButton = page.getByRole('link', {
      name: 'Edit access needs',
    });
    this.editInformationCitizenButton = page.getByRole('link', {
      name: 'Edit information for citizens',
    });

    this.closeNotificationBannerButton = page.getByRole('button', {
      name: 'Close',
    });
  }

  async attributeIsTrue(attributeName: string) {
    await expect(
      this.page
        .getByRole('row')
        .filter({ has: this.page.getByText(attributeName) })
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

  async verifyDefaultSitePage() {
    await expect(
      this.page.getByRole('heading', { name: `${this.headerMsg}` }),
    ).toBeVisible();
    await expect(
      this.page.getByRole('heading', { name: `${this.defaultSiteName}` }),
    ).toBeVisible();

    await expect(
      this.page.getByRole('heading', { name: `${this.siteDetailsheaderMsg}` }),
    ).toBeVisible();

    await this.verifyCoreDetailsContent(
      this.defaultAddress,
      this.defaultLatitude,
      this.defaultLongitude,
      this.defaultPhoneNumber,
    );

    await expect(
      this.page.getByRole('heading', { name: `${this.adminDetailsheaderMsg}` }),
    ).toBeVisible();

    await this.verifySummaryListItemContentValue(
      this.odsCodeLabel,
      this.defaultODSCode,
    );

    await this.verifySummaryListItemContentValue(
      this.icbLabel,
      this.defaultICB,
    );

    await this.verifySummaryListItemContentValue(
      this.regionLabel,
      this.defaultRegion,
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
