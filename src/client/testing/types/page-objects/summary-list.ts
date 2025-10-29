import { type Locator } from '@playwright/test';
import PageObject from './page-object';

export default class SummaryList extends PageObject {
  async getItem(label: string | RegExp): Promise<Locator> {
    return this.self()
      .getByRole('listitem')
      .filter({ has: this.page.getByText(label, { exact: true }) })
      .getByRole('definition');
  }
}
