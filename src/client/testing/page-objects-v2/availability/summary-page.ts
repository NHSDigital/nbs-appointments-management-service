import { type Locator } from '@playwright/test';
import { MYALayout } from '@e2etests/types';

export default class SummaryPage extends MYALayout {
  readonly title: Locator = this.page.getByRole('heading', {
    name: /Check your answers/i,
  });

  readonly saveSessionButton: Locator = this.page.getByRole('button', {
    name: 'Save and publish availability',
  });

  async changeFunctionalityLink(val: string) {
    const row = this.page.locator('.nhsuk-summary-list__row', {
      // Matches "Date" or "Dates"
      has: this.page.locator('.nhsuk-summary-list__key', {
        hasText: new RegExp(`^${val}`, 'i'),
      }),
    });

    await row.locator('a', { hasText: 'Change' }).click();
  }
}
