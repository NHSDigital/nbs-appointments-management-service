import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

type Appointment = {
  time: string;
  nameNhsNumber: string;
  dob: string;
  services: string;
  contactDetails: string;
};

export default class DailyAppointmentDetailsPage extends RootPage {
  readonly backToWeekViewButton: Locator;
  readonly continueButton: Locator;
  readonly appointmentsTable: Locator;

  constructor(page: Page) {
    super(page);
    this.backToWeekViewButton = page.getByRole('link', {
      name: 'Back to week view',
    });
    this.continueButton = page.getByRole('button', {
      name: 'Continue',
    });
    this.appointmentsTable = page.getByRole('table');
  }

  async verifyDailyAppointmentDetailsPageDisplayed() {
    await expect(this.page.getByText('Scheduled')).toBeVisible();
  }

  async verifyTableExistsWithHeaders() {
    await expect(this.appointmentsTable).toBeVisible();
    const headerRow = this.appointmentsTable.getByRole('row').first();
    const headers = await headerRow.getByRole('columnheader').all();

    expect(headers).toHaveLength(6);

    //verify header details
    await expect(headers[0]).toHaveText('Time');
    await expect(headers[1]).toHaveText('Name and NHS number');
    await expect(headers[2]).toHaveText('Date of birth');
    await expect(headers[3]).toHaveText('Contact details');
    await expect(headers[4]).toHaveText('Services');
    await expect(headers[5]).toHaveText('Action');
  }

  async verifyAllDailyAppointmentsTableInformationDisplayedCorrectly(
    expectedAppointments: Appointment[],
  ) {
    await this.verifyTableExistsWithHeaders();

    const tableRows = await this.appointmentsTable.getByRole('row').all();

    //n+1 rows (header included)
    expect(tableRows).toHaveLength(expectedAppointments.length + 1);

    for (let index = 0; index < expectedAppointments.length; index++) {
      const expectedAppointment = expectedAppointments[index];

      //start at 1 to ignore header row
      const tableRow = (await this.appointmentsTable.getByRole('row').all())[
        index + 1
      ];

      const cells = await tableRow.getByRole('cell').all();

      //verify cell details
      await expect(cells[0]).toContainText(expectedAppointment.time);
      await expect(cells[1]).toContainText(expectedAppointment.nameNhsNumber);
      await expect(cells[2]).toContainText(expectedAppointment.dob);
      await expect(cells[3]).toContainText(expectedAppointment.contactDetails);
      await expect(cells[4]).toContainText(expectedAppointment.services);
    }
  }

  async navigateToWeekView() {
    await this.backToWeekViewButton.click();
  }

  async cancelAppointment(appointmentDetail: string) {
    await this.page
      .getByRole('row')
      .filter({ hasText: `${appointmentDetail}` })
      .getByRole('link', { name: 'Cancel' })
      .click();
  }

  async confirmAppointmentCancellation(option: 'Yes' | 'No') {
    if (option == 'Yes') {
      await this.page
        .getByLabel('Yes, I want to cancel this appointment')
        .click();
    }
    if (option == 'No') {
      await this.page
        .getByLabel(`No, I do not want to cancel this appointment`)
        .click();
    }
    await this.continueButton.click();
  }

  async verifyAppointmentNotCancelled(appointmentDetail: string) {
    await expect(
      this.page.getByRole('row').filter({ hasText: `${appointmentDetail}` }),
    ).not.toBeVisible();
  }

  async verifyAppointmentCancelled(appointmentDetail: string) {
    await expect(
      this.page.getByRole('row').filter({ hasText: `${appointmentDetail}` }),
    ).toBeVisible();
  }

  async verifyManualAppointment() {
    await expect(
      this.page
        .getByRole('main')
        .getByText(
          `There are no booked appointments affected by availability changes.`,
        ),
    ).toBeVisible();
  }
}
