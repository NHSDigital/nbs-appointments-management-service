import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';

export default class SiteSelectionPage extends RootPage {
  readonly title: Locator;
  readonly siteSelectionCardHeading: Locator;
  readonly noSitesMessage: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Appointment Management Service',
    });
    this.siteSelectionCardHeading = page.getByRole('heading', {
      name: 'Choose a site',
    });
    this.noSitesMessage = page.getByText(
      'You have not been assigned to any sites.',
    );
  }

  async selectSite(siteName: string) {
    await this.page.getByRole('link', { name: siteName }).click();
  }
}
