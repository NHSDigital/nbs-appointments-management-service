import { MYALayout } from '@e2etests/types';
import CheckYourAnswersPage from './check-your-answers-page';
import SelectDatePage from './select-date-page';
import MonthViewAvailabilityPage from '../view-availability-appointment-pages/month-view-availability-page';

export default class CancellationImpactPage extends MYALayout {
  title = this.page.getByRole('heading', {
    name: this.site?.name,
  });

  readonly keepBookingsRadio = this.page.getByRole('radio', {
    name: 'Keep bookings',
  });

  readonly cancelBookingsRadio = this.page.getByRole('radio', {
    name: 'Cancel bookings',
  });

  readonly canNotCancelHeading = this.page.getByRole('heading', {
    name: /You cannot cancel these sessions/i,
  });

  readonly noSessionsHeading = this.page.getByRole('heading', {
    name: 'There are no sessions in this date range',
    exact: true,
  });

  readonly canNotCancelReturnButton = this.page.getByRole('button', {
    name: 'Return to view availability',
    exact: true,
  });

  readonly newDateRangeButton = this.page.getByRole('button', {
    name: 'Choose a new date range.',
    exact: true,
  });

  readonly canNotCancelDifferentDatesLink = this.page.getByRole('button', {
    name: 'Select different dates',
    exact: true,
  });

  readonly cancelSessionsNoBookingsText = this.page.getByText(
    /There are no bookings for (this|these) (session|sessions)?/i,
  );

  readonly cancelSessionsHeading = (sessionCount: number) =>
    this.page.getByRole('heading', {
      name: `You are about to cancel ${sessionCount} ${sessionCount > 1 ? 'sessions' : 'session'}`,
      exact: true,
    });

  readonly continueButton = this.page.getByRole('button', {
    name: 'Continue',
    exact: true,
  });

  async clickContinueButton(): Promise<CheckYourAnswersPage> {
    await this.continueButton.click();

    return new CheckYourAnswersPage(this.page, this.site);
  }

  async clickNewDateRangeButton(): Promise<SelectDatePage> {
    await this.newDateRangeButton.click();

    return new SelectDatePage(this.page, this.site);
  }

  async clickCanNotCancelReturnButton(): Promise<MonthViewAvailabilityPage> {
    await this.canNotCancelReturnButton.click();

    return new MonthViewAvailabilityPage(this.page, this.site);
  }

  async clickCanNotCancelDifferentDatesButton(): Promise<SelectDatePage> {
    await this.canNotCancelDifferentDatesLink.click();

    return new SelectDatePage(this.page, this.site);
  }
}
