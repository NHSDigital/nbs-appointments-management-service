import { MYALayout } from '@e2etests/types';
import { expect, Locator } from '../../fixtures-v2';
import { WeekOverview } from '../../availability';
import ChangeAvailabilityPage from '../change-availability/change-availability-page';
import WeekViewAvailabilityPage from './week-view-availability-page';

export default class DayViewAvailabilityPage extends MYALayout {
  title = this.page.getByRole('heading');

  readonly changeAvailabilityButton = this.page.getByRole('button', {
    name: 'Change availability',
  });

  async clickChangeAvailabilityButton(): Promise<ChangeAvailabilityPage> {
    await this.changeAvailabilityButton.click();

    await this.page.waitForURL(
      `/manage-your-appointments/site/${this.site?.id}/change-availability`,
    );

    return new ChangeAvailabilityPage(this.page, this.site);
  }

  async getHeading(text: string): Promise<Locator> {
    return this.page.getByRole('heading', {
      name: text,
    });
  }
}
