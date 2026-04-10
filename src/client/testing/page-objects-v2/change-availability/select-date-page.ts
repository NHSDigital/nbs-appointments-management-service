import { MYALayout } from '@e2etests/types';
import { DateComponents } from '@types';
import CancellationImpactPage from './cancellation-impact-page';
import ChangeAvailabilityPage from './change-availability-page';

export default class SelectDatePage extends MYALayout {
  title = this.page.getByRole('heading', {
    name: this.site?.name,
  });

  readonly pageHeading = this.page.getByRole('heading', {
    name: 'Select dates to cancel',
    exact: true,
  });

  readonly startDateDayInput = this.page.locator('#start-date-day');
  readonly startDateMonthInput = this.page.locator('#start-date-month');
  readonly startDateYearInput = this.page.locator('#start-date-year');
  readonly endDateDayInput = this.page.locator('#end-date-day');
  readonly endDateMonthInput = this.page.locator('#end-date-month');
  readonly endDateYearInput = this.page.locator('#end-date-year');
  readonly continueButton = this.page.getByRole('button', {
    name: 'Continue',
    exact: true,
  });

  readonly startDateError = this.page
    .locator('.nhsuk-form-group--error')
    .filter({
      has: this.page.locator('legend', { hasText: /^Start date$/ }),
    })
    .locator('.nhsuk-error-message');

  readonly endDateError = this.page
    .locator('.nhsuk-form-group--error')
    .filter({
      has: this.page.locator('legend', { hasText: /^End date$/ }),
    })
    .locator('.nhsuk-error-message');

  readonly backButton = this.page.getByRole('link', {
    name: 'Back',
    exact: true,
  });

  async fillDates(
    startDate: DateComponents,
    endDate: DateComponents,
  ): Promise<void> {
    await this.startDateDayInput.fill(startDate.day.toString());
    await this.startDateMonthInput.fill(startDate.month.toString());
    await this.startDateYearInput.fill(startDate.year.toString());
    await this.endDateDayInput.fill(endDate.day.toString());
    await this.endDateMonthInput.fill(endDate.month.toString());
    await this.endDateYearInput.fill(endDate.year.toString());
  }

  async clickContinueButton(): Promise<CancellationImpactPage> {
    await this.continueButton.click();

    return new CancellationImpactPage(this.page, this.site);
  }

  async clickContinueButtonForError(): Promise<void> {
    await this.continueButton.click();
  }

  async clickBackButton(): Promise<ChangeAvailabilityPage> {
    await this.backButton.click();

    return new ChangeAvailabilityPage(this.page, this.site);
  }
}
