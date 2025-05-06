import { type Page } from '@playwright/test';
import RootPage from '../root';
import NamesStep from './names-step';
import EmailStep from './email-step';
import RolesStep from './roles-step';
import SummaryStep from './summary-step';

export default class ManageUserPage extends RootPage {
  readonly emailStep: EmailStep;
  readonly namesStep: NamesStep;
  readonly rolesStep: RolesStep;
  readonly summaryStep: SummaryStep;

  constructor(page: Page) {
    super(page);

    this.emailStep = new EmailStep(page);
    this.namesStep = new NamesStep(page);
    this.rolesStep = new RolesStep(page);
    this.summaryStep = new SummaryStep(page, 'Confirm');
  }
}
