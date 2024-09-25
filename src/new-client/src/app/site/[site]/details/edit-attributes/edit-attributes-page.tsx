// import { Button, ButtonGroup, SummaryList } from '@components/nhsuk-frontend';
// import AddAttributesForm from './add-attributes-form';
// import {
//   fetchAttributeDefinitions,
//   fetchSiteAttributeValues,
// } from '@services/appointmentsService';

// type Props = {
//   site: string;
//   permissions: string[];
// };

// export const SiteAttributesPage = async ({ site, permissions }: Props) => {
//   const attributeDefinitions = await fetchAttributeDefinitions();
//   const accessibilityAttributeDefinitions = attributeDefinitions.filter(ad =>
//     ad.id.startsWith('accessibility'),
//   );
//   const siteAttributeValues = await fetchSiteAttributeValues(site);

//   return (
//     <>
//       <div className="nhsuk-form-group">
//         <div className="nhsuk-hint">Configure your current site details</div>
//       </div>
//       <AddAttributesForm
//         attributeDefinitions={accessibilityAttributeDefinitions}
//         site={site}
//         attributeValues={siteAttributeValues}
//       />
//     </>
//   );
// };
