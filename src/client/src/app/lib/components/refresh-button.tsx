'use client';
import { useRouter } from 'next/navigation';
import { Button } from '@components/nhsuk-frontend';

const RefreshButton = () => {
  const router = useRouter();

  const handleRefresh = () => {
    router.refresh();
  };

  return (
    <Button onClick={handleRefresh} className="nhsuk-button">
      Refresh the page
    </Button>
  );
};

export default RefreshButton;
