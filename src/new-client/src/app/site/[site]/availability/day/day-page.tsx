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
import ErrorSummaryCard, {
  ErrorWithLink,
} from '@components/error-summary-card';
import {
  calculateAvailabilityInBlocks,
  timeSort,
} from '@services/availabilityService';
import { useRouter } from 'next/navigation';
import ConfirmRemoveBlock from './confirm-remove-block';
import { FormProvider, useForm } from 'react-hook-form';

type DayViewProps = {
  referenceDate: string;
  site: Site;
};

export type FormFields = {
  startTime: string;
  endTime: string;
  maxSimultaneousAppointments: number;
  appointmentLength: number;
  services: {
    covid: boolean;
    covidAges: string[];
    flu: boolean;
    fluAges: string[];
    shingles: boolean;
    shinglesAges: string[];
    pneumonia: boolean;
    pneumoniaAges: string[];
    rsv: boolean;
    rsvAges: string[];
  };
  isBreak?: boolean;
};

const DayViewPage = ({ referenceDate, site }: DayViewProps) => {
  const { push } = useRouter();
  const [blockToDelete, setBlockToDelete] =
    useState<AvailabilityBlock | null>();

  const parsedDate = parseDate(referenceDate);
  const { blocks, saveBlock, removeBlock } = useAvailability();

  const methods = useForm<FormFields>({
    defaultValues: {
      startTime: '09:00',
      endTime: '17:00',
      maxSimultaneousAppointments: 1,
      appointmentLength: 5,
      services: {
        covid: false,
        covidAges: [],
        flu: false,
        fluAges: [],
        shingles: false,
        shinglesAges: [],
        pneumonia: false,
        pneumoniaAges: [],
        rsv: false,
        rsvAges: [],
      },
      isBreak: false,
    },
  });

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

  const globalErrors = useMemo(() => {
    const errors: ErrorWithLink[] = [];
    if (methods.formState.errors.startTime?.message) {
      errors.push({
        message: methods.formState.errors.startTime?.message,
        link: '#startTime',
      });
    }

    if (methods.formState.errors.endTime?.message) {
      errors.push({
        message: methods.formState.errors.endTime?.message,
        link: '#startTime',
      });
    }

    if (methods.formState.errors.services?.covidAges?.message) {
      errors.push({
        message: methods.formState.errors.services?.covidAges?.message,
        link: '#services-offered-in-session',
      });
    }

    if (methods.formState.errors.services?.fluAges?.message) {
      errors.push({
        message: methods.formState.errors.services?.fluAges?.message,
        link: '#services-offered-in-session',
      });
    }

    if (methods.formState.errors.services?.shinglesAges?.message) {
      errors.push({
        message: methods.formState.errors.services?.shinglesAges?.message,
        link: '#services-offered-in-session',
      });
    }

    if (methods.formState.errors.services?.pneumoniaAges?.message) {
      errors.push({
        message: methods.formState.errors.services?.pneumoniaAges?.message,
        link: '#services-offered-in-session',
      });
    }

    if (methods.formState.errors.services?.rsvAges?.message) {
      errors.push({
        message: methods.formState.errors.services?.rsvAges?.message,
        link: '#services-offered-in-session',
      });
    }

    if (errors.length > 0) {
      return errors;
    }
    return undefined;
  }, [methods.formState]);
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

      {globalErrors && (
        <ErrorSummaryCard
          message={
            'The session cannot be added to the schedule for the following reasons:'
          }
          errorsWithLinks={globalErrors}
        />
      )}
      <FormProvider {...methods}>
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
      </FormProvider>
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
