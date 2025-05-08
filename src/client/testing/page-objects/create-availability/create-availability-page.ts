import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';
import { Site } from '@types';
import CreateAvailabilityWizardPage from './create-availability-wizard-page';
import { TopNav } from '@testing-page-objects';

export default class CreateAvailabilityPage extends RootPage {
  readonly title: Locator;
  readonly site: Site;
  readonly topNav: TopNav;

  readonly createAvailabilityButton: Locator = this.page.getByRole('button', {
    name: 'Create availability',
  });

  readonly availabilityCreatedTable: Locator = this.page.getByRole('table');

  constructor(page: Page, site: Site) {
    super(page);
    this.topNav = new TopNav(page, site);
    this.site = site;
    this.title = page.getByRole('heading', {
      name: 'Create availability',
    });
  }

  async clickCreateAvailabilityButton(): Promise<CreateAvailabilityWizardPage> {
    await this.createAvailabilityButton.click();
    await this.page.waitForURL(
      `**/site/${this.site.id}/create-availability/wizard`,
    );

    return new CreateAvailabilityWizardPage(this.page, this.site);
  }
}
