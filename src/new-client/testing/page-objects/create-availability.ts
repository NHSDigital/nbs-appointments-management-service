import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';

export default class CreateAvailabilityPage extends RootPage {
  readonly title: Locator;
  readonly btnCreateAvailability: Locator;
  readonly sessionTittle: Locator;
  readonly btnContinue: Locator;
  readonly setAndCapacityTittle: Locator;
  readonly btnSaveSession: Locator;
  readonly sessionSuccessMsg: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Create availability',
    });

    this.btnCreateAvailability = page.getByRole('button', {
      name: 'Create availability',
    });

    this.sessionTittle = page.getByRole('heading', {
      name: 'What type of session do you want to create?',
    });

    this.btnContinue = page.getByRole('button', {
      name: 'Continue',
    });

    this.setAndCapacityTittle = page.getByRole('heading', {
      name: 'Set time and capacity for your session',
    });

    this.btnSaveSession = page.getByRole('button', {
      name: 'Save session',
    });

    this.sessionSuccessMsg = page.getByText(
      'You have successfully created availability for the current site.',
    );
  }

  async selectSession(sessionType: string) {
    await this.page.getByRole('radio', { name: sessionType }).click();
  }

  async enterSessionStartDate(day: string, month: string, year: string) {
    await this.page.locator('id=start-date-input-day').fill(day);
    await this.page.locator('id=start-date-input-month').fill(month);
    await this.page.locator('id=start-date-input-year').fill(year);
  }

  async enterSessionEndDate(day: string, month: string, year: string) {
    await this.page.locator('id=end-date-input-day').fill(day);
    await this.page.locator('id=end-date-input-month').fill(month);
    await this.page.locator('id=end-date-input-year').fill(year);
  }

  async enterStartTime(hour: string, minute: string) {
    await this.page.getByLabel('Session start time - hour').fill(hour);
    await this.page.getByLabel('Session start time - minute').fill(minute);
  }

  async enterEndtTime(hour: string, minute: string) {
    await this.page.getByLabel('Session end time - hour').fill(hour);
    await this.page.getByLabel('Session end time - minute').fill(minute);
  }

  async noOfVaccinators(numberof: string) {
    await this.page
      .getByLabel('How many vaccinators or vaccination spaces do you have?')
      .fill(numberof);
  }

  async appointmentLength(length: string) {
    await this.page.getByLabel('How long are your appointments?').fill(length);
  }

  async addServices(serviceName: string) {
    await this.page.getByRole('checkbox', { name: serviceName }).click();
  }

  async selectDay(Day: string) {
    await this.page.getByRole('checkbox', { name: Day }).click();
  }
}
