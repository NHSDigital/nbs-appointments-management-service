import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';
import { DatabaseAccount } from '@azure/cosmos';

export default class SummaryPage extends RootPage {
  readonly summaryPageTitle: Locator;
  readonly saveSessionButton: Locator;

  constructor(page: Page) {
    super(page);
    this.summaryPageTitle = page.getByText('Check single date session');

    this.saveSessionButton = page.getByText('Save session');
  }

  async changeFunctionalityLink(val: string) {
    const row = this.page.locator('.nhsuk-summary-list__row', { hasText: val });
    const changeLink = row.locator('.nhsuk-summary-list__actions a');
    await changeLink.click();
  }
}
