import { MYALayout } from '@e2etests/types';
import { expect } from '../../fixtures-v2';

export default class CreateAvailabilityPage extends MYALayout {
  readonly title = this.page.getByRole('heading', {
    name: 'Create availability',
  });

  readonly createAvailabilityButton = this.page.getByRole('button', {
    name: 'Create new availability',
  });

  readonly sessionTitle = this.page.getByRole('heading', {
    name: 'What type of session do you want to create?',
  });

  readonly continueButton = this.page.getByRole('button', {
    name: 'Continue',
  });

  readonly setAndCapacityTitle = this.page.getByRole('heading', {
    name: 'Set time and capacity for your session',
  });

  readonly saveSessionButton = this.page.getByRole('button', {
    name: 'Save and publish availability',
  });

  readonly selectDateErrorMsg = this.page.getByText(
    'Services must run on at least one day',
  );

  readonly sessionSuccessMsg = this.page.getByText(
    'You have successfully created availability for the current site.',
  );

  readonly sessionStartDateErrorMsg = this.page.getByText(
    'Session start date must be within the next year',
  );

  readonly sessionEndDateErrorMsg = this.page.getByText(
    'Session end date must be within the next year',
  );

  readonly sessionDateErrorMsg = this.page.getByText(
    'Session date must be within the next year',
  );

  async selectSession(sessionType: string) {
    await this.page.getByRole('radio', { name: sessionType }).click();
  }

  async enterSingleDateSessionDate(
    day: string | number,
    month: string | number,
    year: string | number,
  ) {
    const sessionDateFormGroup = this.page
      .getByRole('group')
      .filter({ has: this.page.getByText('Session date') });

    await sessionDateFormGroup.getByLabel('Day').fill(`${day}`);
    await sessionDateFormGroup.getByLabel('Month').fill(`${month}`);
    await sessionDateFormGroup.getByLabel('Year').fill(`${year}`);
  }

  async enterWeeklySessionStartDate(
    day: string | number,
    month: string | number,
    year: string | number,
  ) {
    const startDateFormGroup = this.page
      .getByRole('group')
      .filter({ has: this.page.getByText('Start date') });

    await startDateFormGroup.getByLabel('Day').fill(`${day}`);
    await startDateFormGroup.getByLabel('Month').fill(`${month}`);
    await startDateFormGroup.getByLabel('Year').fill(`${year}`);
  }

  async enterWeeklySessionEndDate(
    day: string | number,
    month: string | number,
    year: string | number,
  ) {
    const endDateFormGroup = this.page
      .getByRole('group')
      .filter({ has: this.page.getByText('End date') });

    await endDateFormGroup.getByLabel('Day').fill(`${day}`);
    await endDateFormGroup.getByLabel('Month').fill(`${month}`);
    await endDateFormGroup.getByLabel('Year').fill(`${year}`);
  }

  async enterStartTime(hour: string, minute: string) {
    await this.page.getByLabel('Session start time - hour').fill(hour);
    await this.page.getByLabel('Session start time - minute').fill(minute);
  }

  async enterEndTime(hour: string, minute: string) {
    await this.page.getByLabel('Session end time - hour').fill(hour);
    await this.page.getByLabel('Session end time - minute').fill(minute);
  }

  async enterNoOfVaccinators(numberOf: string) {
    await this.page
      .getByLabel('How many vaccinators or vaccination spaces do you have?')
      .fill(numberOf);
  }

  async appointmentLength(length: string) {
    await this.page.getByLabel('How long are your appointments?').fill(length);
  }

  async addService(serviceName: string) {
    await this.page
      .getByRole('checkbox', { name: serviceName, exact: true })
      .click();
  }

  async addServices(serviceNames: string[]) {
    for (const service of serviceNames) {
      await this.addService(service);
    }
  }

  async selectDay(day: string) {
    await this.page.getByRole('checkbox', { name: day }).click();
  }

  async unSelectDay(day: string) {
    await this.page.getByRole('checkbox', { name: day }).click();
  }

  async verifyCreateAvailabilitySessionPageDisplayed() {
    await expect(this.sessionTitle).toBeVisible();
    await expect(this.continueButton).toBeVisible();
  }
}
