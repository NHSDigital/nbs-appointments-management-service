import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';
import { Site } from '@types';
import { SiteDetailsPage } from '@testing-page-objects';

export default class EditInformationForCitizensPage extends RootPage {
  readonly site: Site;
  readonly title: Locator;
  private readonly backLink: Locator = this.page.getByRole('link', {
    name: 'Go back',
  });

  private readonly saveButton: Locator;
  readonly cancelButton: Locator;
  readonly infoTextArea: Locator;

  constructor(page: Page, site: Site) {
    super(page);
    this.site = site;
    this.title = page.getByRole('heading', { name: 'Site management' });
    this.saveButton = page.getByRole('button', {
      name: 'Confirm site details',
    });
    this.cancelButton = page.getByRole('button', { name: 'Cancel' });
    this.infoTextArea = page.getByLabel(
      'What information would you like to include?',
    );
  }

  async cancel(): Promise<SiteDetailsPage> {
    await this.cancelButton.click();
    await this.page.waitForURL(`**/site/${this.site.id}/details`);

    return new SiteDetailsPage(this.page, this.site);
  }

  async saveCitizenInformation(): Promise<SiteDetailsPage> {
    await this.saveButton.click();
    await this.page.waitForURL(`**/site/${this.site.id}/details`);

    return new SiteDetailsPage(this.page, this.site);
  }
}
