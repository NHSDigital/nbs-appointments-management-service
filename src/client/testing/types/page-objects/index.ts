import { Locator } from '@playwright/test';
import PageObject from './page-object';

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
};

export default PageObject;
export type { CookieBanner, FooterLinks };
