import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

type Appointment = {
  time: string;
  name: string;
  nhsNumber: string;
  dob: string;
  services: string;
  contactDetails: string;
};

export default class CancelAppointmentDetailsPage extends RootPage {
  readonly backButton: Locator;
  readonly header: Locator;
  readonly dateTime: Locator;

  constructor(page: Page) {
    super(page);
    this.backButton = page.getByRole('link', {
      name: 'Go back',
    });
    this.header = page.getByRole('heading', {
      name: 'Cancel appointmentAre you sure you want to cancel this appointment?',
    });
    this.dateTime = page.getByLabel('Date and time');
  }

  async verifyAppointmentDetailsDisplayed(expectedAppointment: Appointment) {
    await this.verifySummaryListItemContentValue(
      'Date and time',
      expectedAppointment.time,
    );
    await this.verifySummaryListItemContentValue(
      'Name',
      expectedAppointment.name,
    );
    await this.verifySummaryListItemContentValue(
      'NHS number',
      expectedAppointment.nhsNumber,
    );
    await this.verifySummaryListItemContentValue(
      'Date of birth',
      expectedAppointment.dob,
    );
    await this.verifySummaryListItemContentValue(
      'Contact information',
      expectedAppointment.contactDetails,
    );
    await this.verifySummaryListItemContentValue(
      'Service',
      expectedAppointment.services,
    );
  }

  async verifySummaryListItemContentValue(title: string, value: string) {
    const listitem = this.page.getByRole('listitem', {
      name: `${title}-listitem`,
    });
    await expect(listitem).toBeVisible();

    const dt = listitem.getByLabel(`${title}-term`);
    await expect(dt).toBeVisible();
    await expect(dt).toHaveText(title);

    const dd = listitem.getByLabel(`${title}-description`);
    await expect(dd).toBeVisible();
    await expect(dd).toHaveText(value);
  }
}
