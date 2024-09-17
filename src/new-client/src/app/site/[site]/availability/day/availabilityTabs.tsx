'use client';

import { Tabs } from '@components/nhsuk-frontend';
import dynamic from 'next/dynamic';
import AddBreakForm from './add-break-form';
import AddSessionForm from './add-session-form';
import { AvailabilityBlock } from '@types';

type Props = {
  saveBlock: (block: AvailabilityBlock, oldBlockStart?: string) => void;
  date: string;
};

const AvailabilityTabs = ({ saveBlock, date }: Props) => {
  return (
    <Tabs
      title="Manage your current site's daily availability."
      tabs={[
        {
          title: 'Add session',
          content: <AddSessionForm saveBlock={saveBlock} date={date} />,
        },
        {
          title: 'Add break',
          content: <AddBreakForm saveBlock={saveBlock} date={date} />,
        },
      ]}
    />
  );
};

export default dynamic(() => Promise.resolve(AvailabilityTabs), {
  ssr: false,
});
