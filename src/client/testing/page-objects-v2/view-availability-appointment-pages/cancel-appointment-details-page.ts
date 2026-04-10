import { type Locator, expect } from '@playwright/test';
import { MYALayout } from '@e2etests/types';

type Appointment = {
  time: string;
  name: string;
  nhsNumber: string;
  dob: string;
  services: string;
  contactDetails: string;
};

export default class CancelAppointmentDetailsPage extends MYALayout {
  readonly title: Locator = this.page.getByRole('heading', {
    name: 'Cancel appointmentAre you sure you want to cancel this appointment?',
  });

  readonly backButton: Locator = this.page.getByRole('link', {
    name: 'Go back',
  });

  readonly dateTime: Locator = this.page.getByLabel('Date and time');

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
      name: `${title} summary`,
    });
    await expect(listitem).toBeVisible();

    await expect(listitem.getByRole('term')).toBeVisible();
    await expect(listitem.getByRole('term')).toHaveText(title);
    await expect(listitem.getByRole('definition')).toBeVisible();
    await expect(listitem.getByRole('definition')).toHaveText(value);
  }
}
