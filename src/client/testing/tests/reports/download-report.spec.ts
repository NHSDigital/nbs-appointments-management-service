import * as fs from 'fs';
import {
  OAuthLoginPage,
  RootPage,
  SitePage,
  SiteSelectionPage,
} from '@testing-page-objects';
import { test, expect, overrideFeatureFlag } from '../../fixtures';
import { Site } from '@types';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;

let site: Site;

test.describe.configure({ mode: 'serial' });

test.beforeAll(async () => {
  await overrideFeatureFlag('SiteSummaryReport', true);
});

test.afterAll(async () => {
  await overrideFeatureFlag('SiteSummaryReport', false);
});

test.beforeEach(async ({ page, getTestSite, getTestUser }) => {
  site = getTestSite(1);
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(getTestUser(12));
});

test('Navigates to the reports page via the header before selecting a site', async () => {
  const reportsPage = await siteSelectionPage.topNav.clickReports();

  await expect(reportsPage.selectDatesStep.stepTitle).toBeVisible();
});

test('Navigates to the reports page via a site page', async () => {
  await siteSelectionPage.selectSite(site);
  const reportsPage = await sitePage.clickReportsCard();

  await expect(reportsPage.selectDatesStep.stepTitle).toBeVisible();
});

test('Downloads a site summary report', async ({ page }) => {
  const reportsPage = await siteSelectionPage.topNav.clickReports();

  await reportsPage.selectDatesStep.startDateInput.fill('2025-08-10');
  await reportsPage.selectDatesStep.endDateInput.fill('2025-08-20');

  await reportsPage.selectDatesStep.continueButton.click();
  await expect(reportsPage.confirmDownloadStep.stepTitle).toBeVisible();
  await reportsPage.confirmDownloadStep.continueButton.click();

  const download = await page.waitForEvent('download');
  expect(download).toBeDefined();
  expect(download.suggestedFilename()).toContain('GeneralSiteSummaryReport');

  const fileName = 'downloaded-test-report.csv';
  await download.saveAs(fileName);
  const csvContent = fs.readFileSync(fileName, 'utf-8');

  const lines = csvContent.split('\n').filter(line => line.trim().length > 0);
  const headers = lines[0].split(',');
  expect(lines.length).toBeGreaterThan(1);

  expect(headers.length).toBe(expectedFileDownloadHeaders.length);

  // Last element of the header may contain a line ending character
  const headersTrimmed = headers.map(header => header.trim());

  expect(headersTrimmed).toEqual(expectedFileDownloadHeaders);

  fs.unlinkSync(fileName);
});

const expectedFileDownloadHeaders = [
  'Site Name',
  'ICB',
  'Region',
  'ODS Code',
  'Longitude',
  'Latitude',
  'RSV:Adult Booked',
  'COVID:5_11 Booked',
  'COVID:12_17 Booked',
  'COVID:18+ Booked',
  'FLU:18_64 Booked',
  'FLU:65+ Booked',
  'COVID_FLU:18_64 Booked',
  'COVID_FLU:65+ Booked',
  'FLU:2_3 Booked',
  'Total Bookings',
  'Maximum Capacity',
  'RSV:Adult Capacity',
  'COVID:5_11 Capacity',
  'COVID:12_17 Capacity',
  'COVID:18+ Capacity',
  'FLU:18_64 Capacity',
  'FLU:65+ Capacity',
  'COVID_FLU:18_64 Capacity',
  'COVID_FLU:65+ Capacity',
  'FLU:2_3 Capacity',
];
