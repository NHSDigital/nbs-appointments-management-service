import NhsPage from '@components/nhs-page';
import { WarningCallout } from '@components/nhsuk-frontend';
import { fetchUserProfile } from '@services/appointmentsService';

// TODO: Update this page with approved copy
export default async function NotFound() {
  const userProfile = await fetchUserProfile();

  return (
    <NhsPage
      title="Appointment Management Service"
      omitTitleFromBreadcrumbs
      userProfile={userProfile}
    >
      <WarningCallout title="Page not found">
        <p>
          The page or resource you're looking for does not exist. Please check
          the address and try again.
        </p>
      </WarningCallout>
    </NhsPage>
  );
}
