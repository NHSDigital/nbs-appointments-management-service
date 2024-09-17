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
  fluServices,
  pneumoniaServices,
  rsvServices,
  shinglesServices,
  timeSort,
} from '@services/availabilityService';
import { useRouter } from 'next/navigation';
import ConfirmRemoveBlock from './confirm-remove-block';
import { FormProvider, useForm } from 'react-hook-form';
import { covidServices } from '@services/availabilityService';

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

  const editAction = {
    title: 'Edit',
    action: (bl: AvailabilityBlock) => {
      methods.setValue('startTime', bl.start);
      methods.setValue('endTime', bl.end);
      methods.setValue('maxSimultaneousAppointments', bl.sessionHolders);
      methods.setValue('appointmentLength', bl.appointmentLength);

      const covidServicesToRepopulate = [
        ...covidServices
          .filter(service => bl.services.includes(service.id))
          .map(_ => _.id),
      ];
      if (covidServicesToRepopulate.length === covidServices.length) {
        covidServicesToRepopulate.push('select-all');
      }
      methods.setValue('services.covidAges', covidServicesToRepopulate);
      if (covidServicesToRepopulate.length > 0) {
        methods.setValue('services.covid', true);
      }

      const fluServicesToRepopulate = [
        ...fluServices
          .filter(service => bl.services.includes(service.id))
          .map(_ => _.id),
      ];
      if (fluServicesToRepopulate.length === fluServices.length) {
        fluServicesToRepopulate.push('select-all');
      }
      methods.setValue('services.fluAges', fluServicesToRepopulate);
      if (fluServicesToRepopulate.length > 0) {
        methods.setValue('services.flu', true);
      }

      const shinglesServicesToRepopulate = [
        ...shinglesServices
          .filter(service => bl.services.includes(service.id))
          .map(_ => _.id),
      ];
      if (shinglesServicesToRepopulate.length === shinglesServices.length) {
        shinglesServicesToRepopulate.push('select-all');
      }
      methods.setValue('services.shinglesAges', shinglesServicesToRepopulate);
      if (shinglesServicesToRepopulate.length > 0) {
        methods.setValue('services.shingles', true);
      }

      const pneumoniaServicesToRepopulate = [
        ...pneumoniaServices
          .filter(service => bl.services.includes(service.id))
          .map(_ => _.id),
      ];
      if (pneumoniaServicesToRepopulate.length === pneumoniaServices.length) {
        pneumoniaServicesToRepopulate.push('select-all');
      }
      methods.setValue('services.pneumoniaAges', pneumoniaServicesToRepopulate);
      if (pneumoniaServicesToRepopulate.length > 0) {
        methods.setValue('services.pneumonia', true);
      }

      const rsvServicesToRepopulate = [
        ...rsvServices
          .filter(service => bl.services.includes(service.id))
          .map(_ => _.id),
      ];
      if (rsvServicesToRepopulate.length === rsvServices.length) {
        rsvServicesToRepopulate.push('select-all');
      }
      methods.setValue('services.rsvAges', rsvServicesToRepopulate);
      if (rsvServicesToRepopulate.length > 0) {
        methods.setValue('services.rsv', true);
      }
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

    if (methods.formState.errors.services?.message) {
      errors.push({
        message: methods.formState.errors.services?.message,
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
            primaryAction={editAction}
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
