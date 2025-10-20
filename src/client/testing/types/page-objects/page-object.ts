import { Page } from '@playwright/test';

abstract class PageObject {
  protected page: Page;

  constructor(page: Page) {
    this.page = page;
  }
}

export default PageObject;
