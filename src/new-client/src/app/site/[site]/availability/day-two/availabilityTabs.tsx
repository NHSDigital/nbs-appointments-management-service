'use client';

import { Card, Tabs } from '@components/nhsuk-frontend';
import dynamic from 'next/dynamic';
import AddBreakForm from './add-break-form';
import AddSessionForm from './add-session-form';

const AvailabilityTabs = () => {
  return (
    <Tabs
      title="Manage your current site's daily availability."
      tabs={[
        {
          title: 'Add session',
          content: (
            <Card>
              <AddSessionForm />
            </Card>
          ),
        },
        {
          title: 'Add break',
          content: (
            <Card>
              <AddBreakForm />
            </Card>
          ),
        },
      ]}
    />
  );
};

export default dynamic(() => Promise.resolve(AvailabilityTabs), {
  ssr: false,
});
