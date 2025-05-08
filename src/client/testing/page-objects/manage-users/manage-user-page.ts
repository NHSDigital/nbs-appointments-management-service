import { type Page } from '@playwright/test';
import RootPage from '../root';
import NamesStep from './names-step';
import EmailStep from './email-step';
import RolesStep from './roles-step';
import SummaryStep from './summary-step';
import { Site } from '@types';

export default class ManageUserPage extends RootPage {
  readonly site: Site;
  readonly emailStep: EmailStep;
  readonly namesStep: NamesStep;
  readonly rolesStep: RolesStep;
  readonly summaryStep: SummaryStep;

  constructor(page: Page, site: Site) {
    super(page);
    this.site = site;

    this.emailStep = new EmailStep(page);
    this.namesStep = new NamesStep(page);
    this.rolesStep = new RolesStep(page);
    this.summaryStep = new SummaryStep(page, site, 'Confirm');
  }
}
