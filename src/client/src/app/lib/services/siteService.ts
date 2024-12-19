import { SummaryListItem } from '@components/nhsuk-frontend';
import { Site } from '@types';

export const mapSiteSummaryData = (site: Site) => {
  if (!site) {
    return undefined;
  }

  const items: SummaryListItem[] = [];

  items.push({
    title: 'Address',
    value: site.address.match(/[^,]+,|[^,]+$/g) || [], // Match each word followed by a comma, or the last word without a comma
  });
  items.push({ title: 'ODS code', value: site.id });
  items.push({ title: 'ICB', value: site.integratedCareBoard });
  items.push({ title: 'Region', value: site.region });

  const border = false;

  return { items, border };
};
