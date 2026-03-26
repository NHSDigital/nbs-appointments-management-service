import { MYALayout } from '@e2etests/types';
import MonthViewAvailabilityPage from './month-view-availability-page';
import WeekViewAvailabilityPage from './week-view-availability-page';
import DayViewAvailabilityPage from './day-view-availability-page';

export default class BaseViewAvailabilityPage extends MYALayout {
  title = this.page.getByRole('heading');

  readonly monthViewLink = this.page.getByRole('link', {
    name: 'Month view',
  });

  readonly dayViewLink = this.page.getByRole('link', {
    name: 'Day view',
  });

  readonly weekViewLink = this.page.getByRole('link', {
    name: 'Week view',
  });

  async clickMonthViewLink(): Promise<MonthViewAvailabilityPage> {
    await this.monthViewLink.click();
    await this.page.waitForURL(
      `/manage-your-appointments/site/${this.site?.id}/view-availability?date=**`,
    );

    return new MonthViewAvailabilityPage(this.page, this.site);
  }

  async clickWeekViewLink(): Promise<WeekViewAvailabilityPage> {
    await this.weekViewLink.click();
    await this.page.waitForURL(
      `/manage-your-appointments/site/${this.site?.id}/view-availability/week?date=**`,
    );

    return new WeekViewAvailabilityPage(this.page, this.site);
  }

  async clickDayViewLink(): Promise<DayViewAvailabilityPage> {
    await this.dayViewLink.click();
    await this.page.waitForURL(
      `/manage-your-appointments/site/${this.site?.id}/view-availability/daily-appointments?date=**`,
    );

    return new DayViewAvailabilityPage(this.page, this.site);
  }
}
