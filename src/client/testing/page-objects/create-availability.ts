import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from './root';

export default class CreateAvailabilityPage extends RootPage {
  readonly title: Locator;
  readonly createAvailabilityButton: Locator;
  readonly sessionTitle: Locator;
  readonly continueButton: Locator;
  readonly setAndCapacityTitle: Locator;
  readonly saveSessionButton: Locator;
  readonly sessionSuccessMsg: Locator;
  readonly selectDateErrorMsg: Locator;
  readonly sessionStartDateErrorMsg: Locator;
  readonly sessionEndDateErrorMsg: Locator;
  readonly sessionDateErrorMsg: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Create availability',
    });

    this.createAvailabilityButton = page.getByRole('button', {
      name: 'Create availability',
    });

    this.sessionTitle = page.getByRole('heading', {
      name: 'What type of session do you want to create?',
    });

    this.continueButton = page.getByRole('button', {
      name: 'Continue',
    });

    this.setAndCapacityTitle = page.getByRole('heading', {
      name: 'Set time and capacity for your session',
    });

    this.saveSessionButton = page.getByRole('button', {
      name: 'Save session',
    });

    this.selectDateErrorMsg = page.getByText(
      'Services must run on at least one day',
    );

    this.sessionSuccessMsg = page.getByText(
      'You have successfully created availability for the current site.',
    );

    this.sessionStartDateErrorMsg = page.getByText(
      'Session start date must be within the next year',
    );

    this.sessionEndDateErrorMsg = page.getByText(
      'Session end date must be within the next year',
    );

    this.sessionDateErrorMsg = page.getByText(
      'Session date must be within the next year',
    );
  }

  async selectSession(sessionType: string) {
    await this.page.getByRole('radio', { name: sessionType }).click();
  }

  async enterSingleDateSessionDate(day: string, month: string, year: string) {
    const sessionDateFormGroup = this.page
      .getByRole('group')
      .filter({ has: this.page.getByText('Session date') });

    await sessionDateFormGroup.getByLabel('Day').fill(day);
    await sessionDateFormGroup.getByLabel('Month').fill(month);
    await sessionDateFormGroup.getByLabel('Year').fill(year);
  }

  async enterWeeklySessionStartDate(day: string, month: string, year: string) {
    const startDateFormGroup = this.page
      .getByRole('group')
      .filter({ has: this.page.getByText('Start date') });

    await startDateFormGroup.getByLabel('Day').fill(day);
    await startDateFormGroup.getByLabel('Month').fill(month);
    await startDateFormGroup.getByLabel('Year').fill(year);
  }

  async enterWeeklySessionEndDate(day: string, month: string, year: string) {
    const endDateFormGroup = this.page
      .getByRole('group')
      .filter({ has: this.page.getByText('End date') });

    await endDateFormGroup.getByLabel('Day').fill(day);
    await endDateFormGroup.getByLabel('Month').fill(month);
    await endDateFormGroup.getByLabel('Year').fill(year);
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
    await this.page.getByRole('checkbox', { name: serviceName }).click();
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
