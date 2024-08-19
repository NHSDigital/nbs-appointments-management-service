'use client';
import NhsErrorPage from '@components/nhs-error-page';
import { useParams } from 'next/navigation';

export default function Error({
  error,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  const { site } = useParams();

  return (
    <NhsErrorPage
      title="Appointment Management Service - Site"
      message={error.message}
      breadcrumbs={[{ name: `Site ${site}` }]}
    />
  );
}
