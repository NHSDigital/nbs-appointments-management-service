import { type Page } from '@playwright/test';
import { FooterLinks, PageObject } from '@e2etests/types';

export default class Footer extends PageObject {
  public readonly links: FooterLinks;

  constructor(page: Page) {
    super(page);
    this.links = new FooterLinks(page);
  }

  async readBuildNumber(): Promise<string | null | undefined> {
    const buildNumberSpan = await this.page
      .getByText(/^Build number: /)
      .textContent();

    return buildNumberSpan?.split('Build number: ')[1];
  }
}
