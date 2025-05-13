/* eslint-disable lines-between-class-members */
import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';
import { Site } from '@types';
import {
  EditAccessNeedsPage,
  EditDetailsPage,
  EditInformationForCitizensPage,
  EditReferenceDetailsPage,
  TopNav,
} from '@testing-page-objects';

export default class SiteDetailsPage extends RootPage {
  readonly site: Site;
  readonly title: Locator;
  readonly topNav: TopNav;

  private getSummaryListItem(cardTitle: string, label: string): Locator {
    return this.page
      .getByRole('listitem')
      .filter({ has: this.page.getByRole('heading', { name: cardTitle }) })
      .getByRole('listitem')
      .filter({ has: this.page.getByText(label, { exact: true }) });
  }

  readonly editSiteReferenceDetailsLink: Locator = this.page.getByRole('link', {
    name: 'Edit site reference details',
  });

  readonly editSiteDetailsLink: Locator = this.page.getByRole('link', {
    name: 'Edit site details',
  });

  readonly editAccessNeedsLink: Locator = this.page.getByRole('link', {
    name: 'Edit access needs',
  });

  readonly editInformationCitizenLink: Locator = this.page.getByRole('link', {
    name: 'Edit information for citizens',
  });

  readonly odsCode: Locator = this.getSummaryListItem(
    'Site reference details',
    'ODS code',
  );
  readonly icb: Locator = this.getSummaryListItem(
    'Site reference details',
    'ICB',
  );
  readonly region: Locator = this.getSummaryListItem(
    'Site reference details',
    'Region',
  );
  readonly address: Locator = this.getSummaryListItem(
    'Site details',
    'Address',
  );
  readonly latitude: Locator = this.getSummaryListItem(
    'Site details',
    'Latitude',
  );
  readonly longitude: Locator = this.getSummaryListItem(
    'Site details',
    'Longitude',
  );
  readonly phoneNumber: Locator = this.getSummaryListItem(
    'Site details',
    'Phone Number',
  );
  readonly accessibleToilet: Locator = this.getSummaryListItem(
    'Access needs',
    'Accessible toilet',
  );
  readonly brailleTranslationService: Locator = this.getSummaryListItem(
    'Access needs',
    'Braille translation service',
  );
  readonly disabledCarParking: Locator = this.getSummaryListItem(
    'Access needs',
    'Disabled car parking',
  );
  readonly carParking: Locator = this.getSummaryListItem(
    'Access needs',
    'Car parking',
  );
  readonly inductionLoop: Locator = this.getSummaryListItem(
    'Access needs',
    'Induction loop',
  );
  readonly signLanguageService: Locator = this.getSummaryListItem(
    'Access needs',
    'Sign language service',
  );
  readonly stepFreeAccess: Locator = this.getSummaryListItem(
    'Access needs',
    'Step free access',
  );
  readonly textRelay: Locator = this.getSummaryListItem(
    'Access needs',
    'Text relay',
  );
  readonly wheelchairAccess: Locator = this.getSummaryListItem(
    'Access needs',
    'Wheelchair access',
  );

  readonly informationForCitizensCard: Locator = this.page
    .getByRole('listitem')
    .filter({ has: this.page.getByText('Information for citizens') });

  constructor(page: Page, site: Site) {
    super(page);
    this.site = site;
    this.topNav = new TopNav(page, site);

    this.title = page.getByRole('heading', {
      name: 'Site details',
    });
  }

  async clickEditAccessNeedsLink(): Promise<EditAccessNeedsPage> {
    await this.editAccessNeedsLink.click();
    await this.page.waitForURL(
      `**/site/${this.site.id}/details/edit-accessibilities`,
    );

    return new EditAccessNeedsPage(this.page, this.site);
  }

  async clickEditInformationForCitizensLink(): Promise<EditInformationForCitizensPage> {
    await this.editInformationCitizenLink.click();
    await this.page.waitForURL(
      `**/site/${this.site.id}/details/edit-information-for-citizens`,
    );

    return new EditInformationForCitizensPage(this.page, this.site);
  }

  async clickEditReferenceDetailsLink(): Promise<EditReferenceDetailsPage> {
    await this.editSiteReferenceDetailsLink.click();
    await this.page.waitForURL(
      `**/site/${this.site.id}/details/edit-reference-details`,
    );

    return new EditReferenceDetailsPage(this.page, this.site);
  }

  async clickEditDetailsLink(): Promise<EditDetailsPage> {
    await this.editSiteDetailsLink.click();
    await this.page.waitForURL(`**/site/${this.site.id}/details/edit-details`);

    return new EditDetailsPage(this.page, this.site);
  }
}
