import { type Locator } from '@playwright/test';
import PageObject from './page-object';

export default class SummaryList extends PageObject {
  getItem(label: string | RegExp): Locator {
    return this.self()
      .getByRole('listitem')
      .filter({ has: this.page.getByText(label, { exact: true }) })
      .getByRole('definition');
  }
}
