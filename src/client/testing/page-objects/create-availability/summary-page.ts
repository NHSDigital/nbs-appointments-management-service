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
    const row = this.page.locator('.nhsuk-summary-list__row', {
      has: this.page.locator('.nhsuk-summary-list__key', { hasText: val }),
    });

    await row.locator('a', { hasText: 'Change' }).click();
  }
}
