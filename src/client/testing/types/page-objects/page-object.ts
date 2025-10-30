import { Locator, Page } from '@playwright/test';

abstract class PageObject {
  protected page: Page;
  private pathToThis?: (page: Page) => Locator;

  public self(): Page | Locator {
    if (this.pathToThis) {
      return this.pathToThis(this.page);
    }

    return this.page;
  }

  constructor(page: Page, pathToThis?: (page: Page) => Locator) {
    this.page = page;
    this.pathToThis = pathToThis;
  }
}

export default PageObject;
