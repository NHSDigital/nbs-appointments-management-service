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

// const SiteDetailsPage = async ({ site, permissions }: Props) => {
//   const attributeDefinitions = await fetchAttributeDefinitions();
//   const accessibilityAttributeDefinitions = attributeDefinitions.filter(ad =>
//     ad.id.startsWith('accessibility'),
//   );
//   const siteAttributeValues = await fetchSiteAttributeValues(site);

//   return (
//     <>
//       <h3>Access needs</h3>
//       <p>The access needs your current site offers</p>

//       <SummaryList
//         items={accessibilityAttributeDefinitions.map(value => {
//           return {
//             title: value.displayName,
//             value: siteAttributeValues
//               .map(siteAttributeValue => siteAttributeValue.id)
//               .includes(value.id)
//               ? 'Status: Active'
//               : 'Status: Inactive',
//             action: {
//               text: 'Change',
//               href: `${site}/attributes/${value.id}`,
//             },
//           };
//         })}
//       />
//       <ButtonGroup>
//         <Button href={`/site/${site}/edit-attributes`}>
//           Manage site details
//         </Button>
//         <Button href={`/site/${site}`} styleType="secondary">
//           Go back
//         </Button>
//       </ButtonGroup>
//     </>
//   );
// };

// export default SiteDetailsPage;
