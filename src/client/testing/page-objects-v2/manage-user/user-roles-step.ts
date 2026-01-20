import { type Locator } from '@playwright/test';
import ManageUserStep from './manage-user-step';

export default class UserRolesStep extends ManageUserStep {
  title = this.page.getByRole('heading', {
    name: 'Additional details',
  });

  readonly appointmentManagerCheckbox: Locator = this.page.getByRole(
    'checkbox',
    {
      name: 'Appointment manager',
    },
  );

  readonly availabilityManagerCheckbox: Locator = this.page.getByRole(
    'checkbox',
    {
      name: 'Availability manager',
    },
  );

  readonly siteDetailsManagerCheckbox: Locator = this.page.getByRole(
    'checkbox',
    {
      name: 'Site details manager',
    },
  );

  readonly userManagerCheckbox: Locator = this.page.getByRole('checkbox', {
    name: 'User manager',
  });
}
