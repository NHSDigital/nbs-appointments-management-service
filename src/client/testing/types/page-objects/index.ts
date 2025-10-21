import { Locator } from '@playwright/test';
import PageObject from './page-object';
import MYALayout from './mya-layout';

type CookieBanner = {
  preAcceptanceHeader: Locator;
  postAcceptanceMessage: Locator;
  acceptCookiesButton: Locator;
  rejectCookiesButton: Locator;
};

type FooterLinks = {
  userGuidance: Locator;
  termsOfUse: Locator;
  privacyPolicy: Locator;
  cookiesPolicy: Locator;
  accessibilityStatement: Locator;
};

type Footer = {
  buildNumber: () => Promise<string | null | undefined>;
  links: FooterLinks;
};

type NavBar = {
  viewAvailability: Locator;
  createAvailability: Locator;
  changeSiteDetails: Locator;
  manageUsers: Locator;
  reports: Locator;
};

type Header = {
  serviceName: Locator;
  currentUser: (userName: string) => Locator;
  changeSiteButton: Locator;
  logOutButton: Locator;
  navBar: NavBar;
};

export { PageObject, MYALayout };
export type { CookieBanner, Footer, FooterLinks, Header };
