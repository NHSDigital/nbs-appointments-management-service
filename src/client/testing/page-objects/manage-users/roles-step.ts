import { type Locator, type Page } from '@playwright/test';
import ManageUserStep from './manage-user-step';

export default class RolesStep extends ManageUserStep {
  readonly stepTitle: Locator;

  readonly appointmentManagerCheckbox: Locator;
  readonly availabilityManagerCheckbox: Locator;
  readonly siteDetailsManagerCheckbox: Locator;
  readonly userManagerCheckbox: Locator;

  constructor(page: Page) {
    super(page);
    this.stepTitle = page.getByRole('heading', {
      name: 'Additional details',
    });

    this.appointmentManagerCheckbox = this.page.getByRole('checkbox', {
      name: 'Appointment manager',
    });
    this.availabilityManagerCheckbox = this.page.getByRole('checkbox', {
      name: 'Availability manager',
    });
    this.siteDetailsManagerCheckbox = this.page.getByRole('checkbox', {
      name: 'Site details manager',
    });
    this.userManagerCheckbox = this.page.getByRole('checkbox', {
      name: 'User manager',
    });
  }
}
