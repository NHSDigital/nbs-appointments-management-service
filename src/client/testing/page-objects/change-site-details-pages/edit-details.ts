import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';
import { Site } from '@types';
import { SiteDetailsPage } from '@testing-page-objects';

export default class EditDetailsPage extends RootPage {
  readonly site: Site;
  readonly title: Locator;
  readonly saveAndContinueButton: Locator;
  readonly backLink: Locator;

  readonly nameInput: Locator;
  readonly addressInput: Locator;
  readonly latitudeInput: Locator;
  readonly longitudeInput: Locator;
  readonly phoneNumberInput: Locator;

  constructor(page: Page, site: Site) {
    super(page);
    this.site = site;
    this.title = page.getByRole('heading', {
      name: 'Edit site details',
    });
    this.backLink = page.getByRole('link', { name: 'Go back' });

    this.saveAndContinueButton = page.getByRole('button', {
      name: 'Save and continue',
    });

    this.nameInput = page.getByRole('textbox', {
      name: 'Site name',
    });

    this.addressInput = page.getByRole('textbox', {
      name: 'Site address',
    });

    this.latitudeInput = page.getByRole('textbox', {
      name: 'Latitude',
    });

    this.longitudeInput = page.getByRole('textbox', {
      name: 'Longitude',
    });

    this.phoneNumberInput = page.getByRole('textbox', {
      name: 'Phone Number',
    });
  }

  async saveSiteDetails(): Promise<SiteDetailsPage> {
    await this.saveAndContinueButton.click();
    await this.page.waitForURL(`**/site/${this.site.id}/details`);

    return new SiteDetailsPage(this.page, this.site);
  }
}
