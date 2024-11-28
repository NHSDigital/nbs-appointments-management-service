import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';

export default class CreateAvailabilityPage extends RootPage {
  readonly title: Locator;
  readonly btnCreateAvailability: Locator;
  readonly sessionTittle: Locator;
  readonly btnContinue: Locator;
  readonly setAndCapacityTittle: Locator;
  readonly btnSaveSession: Locator;

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
  }

  async selectSession(sessionType: string) {
    await this.page.getByRole('radio', { name: sessionType }).click();
  }

  async enterSessionDate(day: string, month: string, year: string) {
    await this.page.getByLabel('Day').fill(day);
    await this.page.getByLabel('Month').fill(month);
    await this.page.getByLabel('Year').fill(year);
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
}
