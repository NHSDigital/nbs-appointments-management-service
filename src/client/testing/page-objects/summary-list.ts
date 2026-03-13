import { Page, type Locator } from '@playwright/test';

export default class SummaryList {
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  getV10Value(label: string | RegExp): Locator {
    const matcher =
      typeof label === 'string' ? new RegExp(`^${label}$`) : label;

    return this.page
      .locator('dt', { hasText: matcher })
      .locator('xpath=following-sibling::dd[1]');
  }

  getV10Action(label: string | RegExp): Locator {
    const matcher =
      typeof label === 'string' ? new RegExp(`^${label}$`) : label;

    return this.page
      .locator('dt', { hasText: matcher })
      .locator('xpath=following-sibling::dd[2]');
  }
}
