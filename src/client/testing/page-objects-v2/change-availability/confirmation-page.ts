import { MYALayout } from '@e2etests/types';
import { Locator } from '@playwright/test';
import MonthViewAvailabilityPage from '../view-availability-appointment-pages/month-view-availability-page';

export default class ConfirmationPage extends MYALayout {
  title = this.page.getByRole('heading', {
    name: this.site?.name,
  });

  readonly goBackToViewAvailabilityLink = this.page.getByRole('link', {
    name: 'Go back to view availability',
    exact: true,
  });

  async getSessionOnlyCancellationHeading(
    sessionCount: number,
  ): Promise<Locator> {
    const headingText = `${sessionCount} ${sessionCount > 1 ? 'sessions' : 'session'} cancelled`;
    return this.page.getByRole('heading', {
      name: headingText,
      exact: true,
    });
  }

  async clickGoBackToViewAvailabilityLink(): Promise<MonthViewAvailabilityPage> {
    await this.goBackToViewAvailabilityLink.click();

    await this.page.waitForURL(
      `/manage-your-appointments/site/${this.site?.id}/view-availability`,
    );

    return new MonthViewAvailabilityPage(this.page, this.site);
  }
}
