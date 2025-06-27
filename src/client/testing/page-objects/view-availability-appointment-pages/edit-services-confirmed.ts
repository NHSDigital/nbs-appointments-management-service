import { expect } from '@playwright/test';
import RootPage from '../root';
import { RemovedServicesOverview } from '../../availability';

export default class EditServicesConfirmedPage extends RootPage {
  async verifyServicesRemoved(removedSessions: RemovedServicesOverview) {
    await expect(this.page.getByRole('main')).toContainText(
      `Services removed for ${removedSessions.date}`,
    );

    await expect(
      this.page.getByText('You have successfully edited the session.'),
    ).toBeVisible();

    const sessionTable = this.page.getByRole('table');

    //single table
    await expect(sessionTable).toBeVisible();

    const allTableRows = await sessionTable.getByRole('row').all();

    //start at 1 to ignore table header row
    const tableRow = allTableRows[1];
    const allCells = await tableRow.getByRole('cell').all();

    await expect(allCells[0]).toContainText(
      removedSessions.sessionTimeInterval,
    );
    await expect(allCells[1]).toContainText(removedSessions.serviceNames);
  }
}
