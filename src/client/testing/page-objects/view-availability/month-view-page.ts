import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';
import { Site } from '@types';
import { TopNav, WeekViewPage } from '@testing-page-objects';

export default class MonthViewPage extends RootPage {
  readonly site: Site;
  readonly topNav: TopNav;

  readonly nextMonthButton: Locator;
  readonly previousMonthButton: Locator;

  constructor(page: Page, site: Site) {
    super(page);
    this.site = site;
    this.topNav = new TopNav(page, site);

    this.nextMonthButton = page.getByRole('link', {
      name: 'Next',
    });
    this.previousMonthButton = page.getByRole('link', {
      name: 'Previous',
    });
  }

  async goToSpecificDate(date: string): Promise<this> {
    await this.page.goto(
      `/manage-your-appointments/site/${this.site.id}/view-availability?date=${date}`,
    );
    await this.page.waitForURL(
      `**/site/${this.site.id}/view-availability?date=${date}`,
    );
    await this.page.waitForSelector('.nhsuk-loader', {
      state: 'detached',
    });

    return this;
  }

  async weekCard(week: string): Promise<Locator> {
    return this.page
      .getByRole('listitem')
      .filter({ has: this.page.getByRole('heading', { name: week }) });
  }

  async clickWeekCard(week: string): Promise<WeekViewPage> {
    const weekCard = await this.weekCard(week);
    await weekCard.click();
    await this.page.waitForURL(
      `**/site/${this.site.id}/view-availability/week?**`,
    );
    await this.page.waitForSelector('.nhsuk-loader', {
      state: 'detached',
    });

    return new WeekViewPage(this.page, this.site);
  }
}
