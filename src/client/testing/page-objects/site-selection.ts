import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';
import { Site } from '@types';

export default class SiteSelectionPage extends RootPage {
  readonly title: Locator;
  readonly siteSelectionCardHeading: Locator;
  readonly noSitesMessage: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Manage your appointments',
    });
    this.siteSelectionCardHeading = page.getByRole('heading', {
      name: 'Choose a site',
    });
    this.noSitesMessage = page.getByText(
      'You have not been assigned to any sites.',
    );
  }

  async selectSite(site: Site) {
    await this.page.getByRole('link', { name: site.name }).click();
    await this.page.waitForURL(`**/site/${site.id}`);
  }
}
