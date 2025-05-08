import { type Locator, type Page } from '@playwright/test';
import CreateAvailabilityStep from './create-availability-step';

export default class SingleOrRepeatingSessionStep extends CreateAvailabilityStep {
  readonly title: Locator;
  readonly singleSessionRadio: Locator;
  readonly weeklyRepeatingSessionRadio: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'What type of session do you want to create?',
    });
    this.singleSessionRadio = page.getByRole('radio', {
      name: 'Single date session',
    });
    this.weeklyRepeatingSessionRadio = page.getByRole('radio', {
      name: 'Weekly sessions',
    });
  }
}
