import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';
import { Site } from '@types';

export default class SiteSelectionPage extends RootPage {
  readonly title: Locator;
  readonly noSitesMessage: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Manage your appointments',
    });
    this.noSitesMessage = page.getByText(
      'You have not been assigned to any sites.',
    );
  }

  async selectSite(site: Site) {
    const row = this.page.getByRole('row', { name: site.name });
    await row.getByRole('link', { name: 'View' }).click();
    await this.page.waitForURL(`**/site/${site.id}`);
  }
}
