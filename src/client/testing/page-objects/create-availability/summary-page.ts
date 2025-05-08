import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';
import { Site } from '@types';

export default class SummaryPage extends RootPage {
  readonly site: Site;
  readonly summaryPageTitle: Locator;
  readonly saveSessionButton: Locator;

  constructor(page: Page, site: Site) {
    super(page);
    this.site = site;
    this.summaryPageTitle = page.getByText('Check single date session');

    this.saveSessionButton = page.getByText('Save session');
  }

  async changeFunctionalityLink(val: string) {
    await this.page
      .getByRole('listitem')
      .filter({ hasText: `${val}` })
      .getByText('Change')
      .click();
  }
}
