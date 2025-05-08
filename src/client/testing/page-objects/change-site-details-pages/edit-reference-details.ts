import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';
import { Site } from '@types';
import { SiteDetailsPage } from '@testing-page-objects';

export default class EditReferenceDetailsPage extends RootPage {
  readonly site: Site;
  readonly title: Locator;
  readonly saveAndContinueButton: Locator;
  readonly backLink: Locator;

  readonly odsCodeInput: Locator;
  readonly icbSelectInput: Locator;
  readonly regionSelectInput: Locator;

  constructor(page: Page, site: Site) {
    super(page);
    this.site = site;
    this.title = page.getByRole('heading', {
      name: 'Edit site reference details',
    });
    this.backLink = page.getByRole('link', { name: 'Go back' });

    this.saveAndContinueButton = page.getByRole('button', {
      name: 'Save and continue',
    });

    this.odsCodeInput = page.getByRole('textbox', {
      name: 'ODS code',
    });

    this.icbSelectInput = page.getByRole('combobox', {
      name: 'ICB',
    });

    this.regionSelectInput = page.getByRole('combobox', {
      name: 'Region',
    });
  }

  async saveReferenceDetails(): Promise<SiteDetailsPage> {
    await this.saveAndContinueButton.click();
    await this.page.waitForURL(`**/site/${this.site.id}/details`);

    return new SiteDetailsPage(this.page, this.site);
  }
}
