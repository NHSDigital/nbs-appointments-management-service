import { expect } from '../../fixtures';
import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';
import { Site } from '@types';

export default class SiteDetailsPage extends RootPage {
  readonly title: Locator;
  readonly editSiteAttributesButton: Locator;
  readonly editSiteDetailsButton: Locator;
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

  readonly sites: Site[] = [
    {
      id: 'test-1',
      address: 'Pudsey, Leeds, LS28 7LD',
      name: 'Church Lane Pharmacy',
      location: {
        coordinates: [-1.66382134, 53.79628754],
        type: 'point',
      },
      phoneNumber: '0113 2222222',
      odsCode: 'ABC02',
      integratedCareBoard: 'Integrated Care Board 2',
      region: 'Region 2',
    },
    {
      id: 'test-2',
      address: 'Pudsey, Leeds, LS28 7BR',
      name: 'Robin Lane Medical Centre',
      location: {
        coordinates: [-1.6610648, 53.795467],
        type: 'point',
      },
      phoneNumber: '0113 1111111',
      odsCode: 'ABC01',
      integratedCareBoard: 'Integrated Care Board 1',
      region: 'Region 1',
    },
  ];

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

  async verifyEditButtonNotVisible() {
    await expect(this.editInformationCitizenButton).not.toBeVisible();
    await expect(this.editSiteAttributesButton).not.toBeVisible();
  }

  async verifyEditButtonToBeVisible() {
    await expect(this.editInformationCitizenButton).toBeVisible();
    await expect(this.editSiteAttributesButton).toBeVisible();
  }

  async verifySitePage(
    siteName: 'Church Lane Pharmacy' | 'Robin Lane Medical Centre',
  ) {
    const site = this.sites.find(x => x.name == siteName);

    if (site === undefined) {
      throw new Error();
    }

    await expect(
      this.page.getByRole('heading', { name: `${this.headerMsg}` }),
    ).toBeVisible();
    await expect(
      this.page.getByRole('heading', { name: `${site?.name}` }),
    ).toBeVisible();

    await expect(
      this.page.getByRole('heading', { name: `${this.siteDetailsheaderMsg}` }),
    ).toBeVisible();

    await this.verifyCoreDetailsContent(
      site.address,
      site.location.coordinates[0].toString(),
      site.location.coordinates[1].toString(),
      site.phoneNumber,
    );

    await expect(
      this.page.getByRole('heading', {
        name: `${this.referenceDetailsheaderMsg}`,
      }),
    ).toBeVisible();

    await this.verifySummaryListItemContentValue(
      this.odsCodeLabel,
      site.odsCode,
    );

    await this.verifySummaryListItemContentValue(
      this.icbLabel,
      site.integratedCareBoard,
    );

    await this.verifySummaryListItemContentValue(this.regionLabel, site.region);

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
