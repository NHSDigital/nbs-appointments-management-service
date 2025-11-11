import { Page, type Locator } from '@playwright/test';
import { MYALayout, SummaryList } from '@e2etests/types';
import SiteDetailsPage from '../site-details-page';

export default class EditSiteStatusPage extends MYALayout {
  title = this.page.getByRole('heading', {
    name: 'Manage site visibility',
  });

  private readonly backLink: Locator = this.page.getByRole('link', {
    name: 'Back',
    exact: true,
  });

  private readonly saveAndContinueButton: Locator = this.page.getByRole(
    'button',
    {
      name: 'Save and continue',
    },
  );

  public readonly siteStatusSummary = new SummaryList(
    this.page,
    (page: Page) => {
      return page.getByRole('list');
    },
  );

  public setSiteOnlineRadio = this.page.getByRole('radio', {
    name: 'Make site online',
  });

  public keepSiteOnlineRadio = this.page.getByRole('radio', {
    name: 'Keep site online',
  });

  public setSiteOfflineRadio = this.page.getByRole('radio', {
    name: 'Take site offline',
  });

  public keepSiteOfflineRadio = this.page.getByRole('radio', {
    name: 'Keep site offline',
  });

  async goBack(): Promise<SiteDetailsPage> {
    await this.backLink.click();
    await this.page.waitForURL(`**/site/${this.site?.id}/details`);

    return new SiteDetailsPage(this.page, this.site);
  }

  async saveChanges(): Promise<SiteDetailsPage> {
    await this.saveAndContinueButton.click();
    await this.page.waitForURL(`**/site/${this.site?.id}/details`);

    return new SiteDetailsPage(this.page, this.site);
  }
}
