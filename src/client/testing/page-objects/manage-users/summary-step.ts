import { type Locator, type Page } from '@playwright/test';
import ManageUserStep from './manage-user-step';

export default class SummaryStep extends ManageUserStep {
  readonly title: Locator;

  readonly nameSummary: Locator;
  readonly emailAddressSummary: Locator;
  readonly rolesSummary: Locator;

  constructor(page: Page, positiveActionButtonText = 'Continue') {
    super(page, positiveActionButtonText);
    this.title = page.getByRole('heading', {
      name: 'Check user details',
    });

    this.nameSummary = this.page
      .locator('.nhsuk-summary-list__row', {
        has: this.page.locator('.nhsuk-summary-list__key', { hasText: 'Name' }),
      })
      .locator('.nhsuk-summary-list__value');

    this.emailAddressSummary = this.page
      .locator('.nhsuk-summary-list__row', {
        has: this.page.locator('.nhsuk-summary-list__key', {
          hasText: 'Email address',
        }),
      })
      .locator('.nhsuk-summary-list__value');

    this.rolesSummary = this.page
      .locator('.nhsuk-summary-list__row', {
        has: this.page.locator('.nhsuk-summary-list__key', {
          hasText: 'Roles',
        }),
      })
      .locator('.nhsuk-summary-list__value');
  }
}
