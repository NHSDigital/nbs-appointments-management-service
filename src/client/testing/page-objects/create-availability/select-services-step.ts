import { type Locator, type Page } from '@playwright/test';
import CreateAvailabilityStep from './create-availability-step';
import { Site } from '@types';

export default class SelectServicesStep extends CreateAvailabilityStep {
  readonly title: Locator;

  readonly rsvCheckbox: Locator = this.page.getByRole('checkbox', {
    name: 'RSV (Adult)',
  });

  constructor(page: Page, site: Site) {
    super(page, site);
    this.title = page.getByRole('heading', {
      name: 'Add services to your session',
    });
  }
}
