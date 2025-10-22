import { type Locator, type Page } from '@playwright/test';
import { NavBar, PageObject } from '@e2etests/types';
import { Site } from '@types';

export default class Header extends PageObject {
  public readonly navBar: NavBar;

  constructor(page: Page, site?: Site) {
    super(page);
    this.navBar = new NavBar(page, site);
  }

  public readonly serviceName: Locator = this.page
    .getByRole('banner')
    .getByRole('link', { name: 'Manage Your Appointments' });

  public readonly changeSiteButton: Locator = this.page
    .getByRole('banner')
    .getByRole('button', { name: 'Change Site' });

  public readonly logOutButton: Locator = this.page
    .getByRole('banner')
    .getByRole('button', { name: 'Log Out' });

  public readonly currentUser: (userName: string) => Locator = (
    userName: string,
  ) => this.page.getByRole('banner').getByText(userName);
}
