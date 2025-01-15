import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';

export default class SummaryPage extends RootPage {
  readonly summaryPageTitle: Locator;
  readonly saveSessionButton: Locator;

  constructor(page: Page) {
    super(page);
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
