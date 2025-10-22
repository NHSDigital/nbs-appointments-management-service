import { type Locator } from '@playwright/test';
import { PageObject } from '@e2etests/types';

export default class FooterLinks extends PageObject {
  public readonly userGuidance: Locator = this.page.getByRole('link', {
    name: 'User guidance',
  });

  public readonly termsOfUse: Locator = this.page.getByRole('link', {
    name: 'Terms of use',
  });

  public readonly privacyPolicy: Locator = this.page.getByRole('link', {
    name: 'Privacy Policy',
  });

  public readonly cookiesPolicy: Locator = this.page.getByRole('link', {
    name: 'Cookies Policy',
  });

  public readonly accessibilityStatement: Locator = this.page.getByRole(
    'link',
    {
      name: 'Accessibility Statement',
    },
  );
}
