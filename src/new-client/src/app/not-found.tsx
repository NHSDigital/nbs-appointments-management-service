import NhsPage from '@components/nhs-page';
import { WarningCallout } from '@components/nhsuk-frontend';

// TODO: Update this page with approved copy
export default function NotFound() {
  return (
    <NhsPage title="Appointment Management Service" omitTitleFromBreadcrumbs>
      <WarningCallout title="Page not found">
        <p>
          The page or resource you're looking for does not exist. Please check
          the address and try again.
        </p>
      </WarningCallout>
    </NhsPage>
  );
}
