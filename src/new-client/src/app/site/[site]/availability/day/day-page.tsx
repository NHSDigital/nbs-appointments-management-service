'use client';
import React, { useMemo, useState } from 'react';
import { AvailabilityBlock, Site } from '@types';
import { DaySummary } from '@components/day-summary';
import { useAvailability } from '@hooks/useAvailability';
import { Button, ButtonGroup, Pagination } from '@components/nhsuk-frontend';
import { formatDateForUrl, parseDate } from '@services/timeService';
import AvailabilityTabs from './availabilityTabs';
import CardWithBlueHeader from '@components/card-with-blue-header';
import Link from 'next/link';
import AppointmentsSummaryText from './appointments-summary-text';
import { confirmSchedule } from './confirm-schedule';
// import ErrorSummaryCard from '@components/error-summary-card';
import {
  calculateAvailabilityInBlocks,
  timeSort,
} from '@services/availabilityService';
import { useRouter } from 'next/navigation';
import ConfirmRemoveBlock from './confirm-remove-block';

type DayViewProps = {
  referenceDate: string;
  site: Site;
};

const DayViewPage = ({ referenceDate, site }: DayViewProps) => {
  const { push } = useRouter();
  const [blockToDelete, setBlockToDelete] =
    useState<AvailabilityBlock | null>();

  const parsedDate = parseDate(referenceDate);
  const { blocks, saveBlock, removeBlock } = useAvailability();

  const filteredBlocks = useMemo(() => {
    return blocks
      .filter(block => block.day.isSame(parsedDate))
      .toSorted(timeSort);
  }, [blocks, parsedDate]);

  const totalAppointments = useMemo(() => {
    return calculateAvailabilityInBlocks(filteredBlocks);
  }, [filteredBlocks]);

  const removeAction = {
    title: 'Delete',
    action: (bl: AvailabilityBlock) => {
      setBlockToDelete(bl);
    },
    test: (b: AvailabilityBlock) => !b.isPreview,
  };

  const yesterday = parsedDate.subtract(1, 'day');
  const tomorrow = parsedDate.add(1, 'day');

  return (
    <>
      <Pagination
        previous={{
          title: yesterday.format('DD MMMM YYYY'),
          href: `/site/${site.id}/availability/day?date=${formatDateForUrl(yesterday)}`,
        }}
        next={{
          title: tomorrow.format('DD MMMM YYYY'),
          href: `/site/${site.id}/availability/day?date=${formatDateForUrl(tomorrow)}`,
        }}
      />

      {/* {bannerError && <ErrorSummaryCard {...bannerError} />} */}
      {/* <ErrorSummaryCard
        message="A session period must contain at least one service to be able to create a new session"
        errorsWithLinks={[
          { message: 'No services selected', link: '#Session%20%Details' },
        ]}
      /> */}
      <AvailabilityTabs
        saveBlock={saveBlock}
        date={formatDateForUrl(parsedDate)}
      />
      <Link href="#maincontent">Back to top</Link>
      {blockToDelete && (
        <ConfirmRemoveBlock
          block={blockToDelete}
          removeBlock={() => {
            removeBlock(blockToDelete);
            setBlockToDelete(null);
          }}
        />
      )}

      <CardWithBlueHeader title="Schedule Preview">
        <DaySummary
          blocks={filteredBlocks}
          showBreaks={true}
          hasError={() => false}
          // primaryAction={editAction}
          secondaryAction={removeAction}
        />
        <AppointmentsSummaryText total={totalAppointments} />
      </CardWithBlueHeader>
      <ButtonGroup>
        <form
          action={confirmSchedule.bind(
            null,
            site.id,
            formatDateForUrl(parsedDate),
          )}
        >
          <Button type="submit">Confirm schedule</Button>
        </form>

        <Button
          type="button"
          styleType="secondary"
          onClick={() => {
            push(
              `/site/${site.id}/availability/month?date=${formatDateForUrl(parsedDate)}`,
            );
          }}
        >
          Cancel
        </Button>
      </ButtonGroup>
      <Link href="#maincontent">Back to top</Link>
    </>
  );
};

export default DayViewPage;
