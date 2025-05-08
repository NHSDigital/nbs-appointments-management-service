import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';
import { Site } from '@types';
import { TopNav } from '@testing-page-objects';

export default class WeekViewPage extends RootPage {
  readonly site: Site;
  readonly topNav: TopNav;

  readonly nextWeekButton: Locator;
  readonly previousWeekButton: Locator;

  constructor(page: Page, site: Site) {
    super(page);
    this.site = site;
    this.topNav = new TopNav(page, site);

    this.nextWeekButton = page.getByRole('link', {
      name: 'Next',
    });
    this.previousWeekButton = page.getByRole('link', {
      name: 'Previous',
    });
  }

  async goToSpecificDate(date: string): Promise<this> {
    await this.page.goto(
      `/manage-your-appointments/site/${this.site.id}/view-availability/week?date=${date}`,
    );
    await this.page.waitForURL(
      `**/site/${this.site.id}/view-availability?date=${date}`,
    );
    await this.page.waitForSelector('.nhsuk-loader', {
      state: 'detached',
    });

    return this;
  }

  async dayCard(day: string): Promise<Locator> {
    return this.page
      .getByRole('listitem')
      .filter({ has: this.page.getByRole('heading', { name: day }) });
  }
}
