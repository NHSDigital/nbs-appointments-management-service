import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';
import { Site } from '@types';
import { TopNav } from '@testing-page-objects';

export default class DayViewPage extends RootPage {
  readonly site: Site;
  readonly topNav: TopNav;

  readonly nextDayButton: Locator;
  readonly previousDayButton: Locator;

  constructor(page: Page, site: Site) {
    super(page);
    this.site = site;
    this.topNav = new TopNav(page, site);

    this.nextDayButton = page.getByRole('link', {
      name: 'Next',
    });
    this.previousDayButton = page.getByRole('link', {
      name: 'Previous',
    });
  }

  async goToSpecificDate(date: string): Promise<this> {
    await this.page.goto(
      `/manage-your-appointments/site/${this.site.id}/view-availability/Day?date=${date}`,
    );
    await this.page.waitForURL(
      `**/site/${this.site.id}/view-availability/day?date=${date}`,
    );
    await this.page.waitForSelector('.nhsuk-loader', {
      state: 'detached',
    });

    return this;
  }
}
