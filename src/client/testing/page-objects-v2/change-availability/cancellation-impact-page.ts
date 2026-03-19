import { MYALayout } from '@e2etests/types';
import CheckYourAnswersPage from './check-your-answers-page';

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

  readonly canNotCancelReturnButton = this.page.getByRole('button', {
    name: 'Return to view availability',
    exact: true,
  });

  readonly canNotCancelDifferentDatesButton = this.page.getByRole('button', {
    name: 'Select different dates',
    exact: true,
  });

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
}
