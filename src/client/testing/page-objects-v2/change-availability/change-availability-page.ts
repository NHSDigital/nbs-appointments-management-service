import { MYALayout } from '@e2etests/types';
import SelectDatePage from './select-date-page';
import MonthViewAvailabilityPage from '../view-availability-appointment-pages/month-view-availability-page';
import WeekViewAvailabilityPage from '../view-availability-appointment-pages/week-view-availability-page';
import DayViewAvailabilityPage from '../view-availability-appointment-pages/day-view-availability-page';

export default class ChangeAvailabilityPage extends MYALayout {
  title = this.page.getByRole('heading', {
    name: this.site?.name,
  });

  readonly continueButton = this.page.getByRole('button', {
    name: 'Continue to cancel',
  });

  readonly backButton = this.page.getByRole('link', {
    name: 'Back',
    exact: true,
  });

  readonly listItems = this.page
    .locator('ol.nhsuk-list--number')
    .getByRole('listitem');

  readonly beforeYouContinueHeading = this.page.getByRole('heading', {
    name: 'Before you continue',
    exact: true,
  });

  async clickContinueButton(): Promise<SelectDatePage> {
    await this.continueButton.click();

    return new SelectDatePage(this.page, this.site);
  }

  async clickBackToMonthViewButton(): Promise<MonthViewAvailabilityPage> {
    await this.backButton.click();

    return new MonthViewAvailabilityPage(this.page, this.site);
  }

  async clickBackToWeekViewButton(): Promise<WeekViewAvailabilityPage> {
    await this.backButton.click();

    await this.page.waitForURL(url =>
      url.pathname.includes(`/site/${this.site?.id}/view-availability/week`),
    );

    return new WeekViewAvailabilityPage(this.page, this.site);
  }

  async clickBackToDayViewButton(): Promise<DayViewAvailabilityPage> {
    await this.backButton.click();

    await this.page.waitForURL(url =>
      url.pathname.includes(
        `/site/${this.site?.id}/view-availability/daily-appointments`,
      ),
    );

    return new DayViewAvailabilityPage(this.page, this.site);
  }
}
