import { type Locator, type Page } from '@playwright/test';
import { MYALayout } from '@e2etests/types';

export default class SiteSelectionPage extends MYALayout {
  title = this.page.getByRole('heading', {
    name: 'Manage your appointments',
  });

  readonly siteSelectionCardHeading: Locator;
  readonly noSitesMessage: Locator;

  constructor(page: Page) {
    super(page);

    this.siteSelectionCardHeading = page.getByRole('heading', {
      name: 'Choose a site',
    });
    this.noSitesMessage = page.getByText(
      'You have not been assigned to any sites.',
    );
  }

  // async selectSite(site: Site): Promise<SitePage> {
  //   await this.page.getByRole('link', { name: site.name }).click();
  //   await this.page.waitForURL(`**/site/${site.id}`);

  //   return new SitePage(this.page, site);
  // }
}
