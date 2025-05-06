import { type Locator, type Page } from '@playwright/test';
import ManageUserStep from './manage-user-step';

export default class SummaryStep extends ManageUserStep {
  readonly stepTitle: Locator;

  readonly nameSummary: Locator;
  readonly emailAddressSummary: Locator;
  readonly rolesSummary: Locator;

  constructor(page: Page, positiveActionButtonText = 'Continue') {
    super(page, positiveActionButtonText);
    this.stepTitle = page.getByRole('heading', {
      name: 'Check user details',
    });

    this.nameSummary = this.page
      .getByRole('listitem')
      .filter({ has: this.page.getByRole('term', { name: 'Name' }) })
      .getByRole('definition');

    this.emailAddressSummary = this.page
      .getByRole('listitem')
      .filter({ has: this.page.getByRole('term', { name: 'Email address' }) })
      .getByRole('definition');

    this.rolesSummary = this.page
      .getByRole('listitem')
      .filter({
        has: this.page.getByRole('term', { name: 'Roles' }),
      })
      .getByRole('definition');
  }
}
