import { Page, type Locator } from '@playwright/test';
import ManageUserStep from './manage-user-step';

export default class UserSummaryStep extends ManageUserStep {
  title = this.page.getByRole('heading', {
    name: 'Check user details',
  });

  readonly nameSummary: Locator;
  readonly emailAddressSummary: Locator;
  readonly rolesSummary: Locator;

  constructor(page: Page, positiveActionButtonText = 'Continue') {
    super(page, positiveActionButtonText);

    this.nameSummary = this.page
      .getByRole('listitem', { name: 'Name summary' })
      .getByRole('definition')
      .filter({ hasNot: this.page.getByRole('button', { name: 'Change' }) });

    this.emailAddressSummary = this.page
      .getByRole('listitem', { name: 'Address summary' })
      .getByRole('definition')
      .filter({ hasNot: this.page.getByRole('button', { name: 'Change' }) });

    this.rolesSummary = this.page
      .getByRole('listitem', { name: 'Roles summary' })
      .getByRole('definition')
      .filter({ hasNot: this.page.getByRole('button', { name: 'Change' }) });
  }
}
