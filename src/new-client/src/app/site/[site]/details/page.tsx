// import NhsPage from '@components/nhs-page';
// import { fetchPermissions, fetchSite } from '@services/appointmentsService';
// import { SiteAttributesPage } from './site-attributes-page';
// import { SummaryList } from '@components/nhsuk-frontend';

// export type PageProps = {
//   params: {
//     site: string;
//   };
// };

// const Page = async ({ params }: PageProps) => {
//   const site = await fetchSite(params.site);
//   const siteMoniker = site?.name ?? `Site ${params.site}`;
//   const sitePermissions = await fetchPermissions(params.site);

//   return (
//     <NhsPage
//       title="Site details"
//       breadcrumbs={[
//         { name: 'Home', href: '/' },
//         { name: siteMoniker, href: `/site/${params.site}` },
//       ]}
//     >
//       <SiteDetailsPage />
//     </NhsPage>
//   );
// };

// export default Page;
