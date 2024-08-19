'use client';
import NhsErrorPage from '@components/nhs-error-page';

export default function Error({
  error,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <NhsErrorPage
      title="Appointment Management Service"
      message={error.message}
    />
  );
}
