import { type Locator, type Page } from '@playwright/test';
import CreateAvailabilityStep from './create-availability-step';

export default class SelectServicesStep extends CreateAvailabilityStep {
  readonly title: Locator;

  readonly rsvCheckbox: Locator = this.page.getByRole('checkbox', {
    name: 'RSV (Adult)',
  });

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Add services to your session',
    });
  }
}
