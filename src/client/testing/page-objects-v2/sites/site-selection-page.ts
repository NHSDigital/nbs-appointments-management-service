import { type Locator } from '@playwright/test';
import { E2ETestSite, MYALayout } from '@e2etests/types';
import { SitePage } from '@e2etests/page-objects';

export default class SiteSelectionPage extends MYALayout {
  title = this.page.getByRole('heading', {
    name: 'Manage your appointments',
  });

  readonly siteSelectionCardHeading: Locator = this.page.getByRole('heading', {
    name: 'Choose a site',
  });

  readonly noSitesMessage: Locator = this.page.getByText(
    'You have not been assigned to any sites.',
  );

  async selectSite(site: E2ETestSite): Promise<SitePage> {
    await this.page.getByRole('link', { name: site.name }).click();
    await this.page.waitForURL(`**/site/${site.id}`);

    return new SitePage(this.page, site);
  }
}
