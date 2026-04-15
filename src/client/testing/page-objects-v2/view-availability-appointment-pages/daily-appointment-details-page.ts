import { type Locator, expect } from '@playwright/test';
import { MYALayout } from '@e2etests/types';

export type Appointment = {
  time: string;
  nameNhsNumber: string;
  dob: string;
  services: string;
  contactDetails: string;
};

export default class DailyAppointmentDetailsPage extends MYALayout {
  // Matches the "Appointments for..." heading
  readonly title: Locator = this.page.getByRole('heading', {
    name: /Appointments for/i,
  });

  readonly backToWeekLink: Locator = this.page.getByRole('link', {
    name: /Back to week view|Go back/i,
  });

  readonly cancelAppointmentButton: Locator = this.page.getByRole('button', {
    name: 'Cancel appointment',
  });

  readonly appointmentsTable: Locator = this.page.getByRole('table');

  readonly scheduledTab: Locator = this.page.locator('a.nhsuk-tabs__tab', {
    hasText: 'Scheduled',
  });

  async verifyDailyAppointmentDetailsPageDisplayed() {
    await expect(this.scheduledTab).toBeVisible();
  }

  async verifyTableExistsWithHeaders(displayAction = true) {
    await expect(this.appointmentsTable).toBeVisible();
    const headers = this.appointmentsTable.getByRole('columnheader');

    // Check for the existence of key headers rather than strict index matching
    const expectedHeaders = [
      'Time',
      'Name and NHS number',
      'Date of birth',
      'Contact details',
      'Services',
    ];

    for (const headerText of expectedHeaders) {
      await expect(headers.filter({ hasText: headerText })).toBeVisible();
    }

    if (displayAction) {
      await expect(headers.filter({ hasText: 'Action' })).toBeVisible();
    }
  }

  async verifyAllDailyAppointmentsTableInformationDisplayedCorrectly(
    expectedAppointments: Appointment[],
  ) {
    await this.verifyTableExistsWithHeaders();

    for (const expected of expectedAppointments) {
      // Find the specific row by NHS number (usually the most unique identifier)
      const row = this.appointmentsTable.getByRole('row').filter({
        hasText: expected.nameNhsNumber,
      });

      await expect(row).toBeVisible();

      // Verify all other details exist within that specific row
      await expect(row).toContainText(expected.time);
      await expect(row).toContainText(expected.dob);
      await expect(row).toContainText(expected.services);
      await expect(row).toContainText(expected.contactDetails);
    }
  }

  async cancelAppointment(appointmentDetail: string) {
    // Finds the row containing the detail (name or NHS number) and clicks Cancel
    await this.page
      .getByRole('row')
      .filter({ hasText: appointmentDetail })
      .getByRole('link', { name: 'Cancel' })
      .click();
  }

  async verifyAppointmentNotCancelled(appointmentDetail: string) {
    const timeOnly = appointmentDetail.replace(/[a-zA-Z]/g, '');

    await expect(
      this.page.locator('.nhsuk-summary-list__row', { hasText: timeOnly }),
    ).toBeVisible();
  }

  async verifyAppointmentCancelled(appointmentDetail: string) {
    // Logic check: If it IS cancelled, it should NOT be visible
    await expect(
      this.page.getByRole('row').filter({ hasText: appointmentDetail }),
    ).not.toBeVisible();
  }

  async verifyOrphanedMessageDoesNotExist() {
    await expect(
      this.page.getByText(/There are no booked appointments affected/i),
    ).not.toBeVisible();
  }

  async navigateToWeekView() {
    await this.backToWeekLink.click();
  }
}
