import { SummaryListItem } from '@components/nhsuk-frontend';
import { Site, WellKnownOdsEntry } from '@types';

export const mapSiteSummaryData = (
  site: Site,
  wellKnownOdsCodeEntries: WellKnownOdsEntry[],
) => {
  if (!site) {
    return undefined;
  }

  const items: SummaryListItem[] = [];

  items.push({
    title: 'Address',
    value: site.address.match(/[^,]+,|[^,]+$/g) || [], // Match each word followed by a comma, or the last word without a comma
  });
  items.push({ title: 'ODS code', value: site.id });
  items.push({
    title: 'ICB',
    value:
      wellKnownOdsCodeEntries.find(e => e.odsCode === site.integratedCareBoard)
        ?.displayName ?? site.integratedCareBoard,
  });
  items.push({
    title: 'Region',
    value:
      wellKnownOdsCodeEntries.find(e => e.odsCode === site.region)
        ?.displayName ?? site.region,
  });

  const border = false;

  return { items, border };
};
