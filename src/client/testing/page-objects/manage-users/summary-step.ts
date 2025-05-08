import { type Locator, type Page } from '@playwright/test';
import ManageUserStep from './manage-user-step';
import { Site } from '@types';
import { UsersPage } from '@testing-page-objects';

export default class SummaryStep extends ManageUserStep {
  readonly title: Locator;
  readonly site: Site;

  readonly nameSummary: Locator;
  readonly emailAddressSummary: Locator;
  readonly rolesSummary: Locator;

  constructor(page: Page, site: Site, positiveActionButtonText = 'Continue') {
    super(page, positiveActionButtonText);
    this.site = site;

    this.title = page.getByRole('heading', {
      name: 'Check user details',
    });

    this.nameSummary = this.page
      .getByRole('listitem', { name: 'Name summary' })
      .getByRole('definition')
      .filter({ hasNot: this.page.getByRole('link', { name: 'Change' }) });

    this.emailAddressSummary = this.page
      .getByRole('listitem', { name: 'Address summary' })
      .getByRole('definition')
      .filter({ hasNot: this.page.getByRole('link', { name: 'Change' }) });

    this.rolesSummary = this.page
      .getByRole('listitem', { name: 'Roles summary' })
      .getByRole('definition')
      .filter({ hasNot: this.page.getByRole('link', { name: 'Change' }) });
  }

  async saveUserRoles(): Promise<UsersPage> {
    await this.continueButton.click();
    await this.page.waitForURL(`**/site/${this.site.id}/users`);

    return new UsersPage(this.page, this.site);
  }
}
