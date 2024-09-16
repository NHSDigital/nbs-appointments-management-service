'use client';
import React from 'react';
import { AvailabilityBlock, Site } from '@types';
import { DaySummary } from '@components/day-summary';
import { useAvailability } from '@hooks/useAvailability';
import { Button, ButtonGroup, Pagination } from '@components/nhsuk-frontend';
import { formatDateForUrl, parseDate } from '@services/timeService';
import AvailabilityTabs from './availabilityTabs';
import CardWithBlueHeader from '@components/card-with-blue-header';
import Link from 'next/link';
import AppointmentsSummaryText from './appointments-summary-text';

// const sessionHolderOptions = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
// const appointmentLengthOptions = [4, 5, 6, 10, 12, 15];

type DayViewProps = {
  referenceDate: string;
  site: Site;
};

const DayViewPage = ({ referenceDate, site }: DayViewProps) => {
  // const [bannerError, setBannerError] = useState<ErrorSummaryProps | undefined>(
  //   undefined,
  // );

  const parsedDate = parseDate(referenceDate);
  const { blocks, saveBlock, removeBlock } = useAvailability();

  // const searchParams = useSearchParams();
  // const pathname = usePathname();
  // const { replace } = useRouter();

  // const date = searchParams.get('date');
  // const day = dayjs(date);

  // const [conflictBlock, setConflictBlock] = React.useState<
  //   string | undefined
  // >();
  // const [errors, setErrors] = React.useState<Errors>({});
  // const [startTime, setStartTime] = React.useState('09:00');
  // const [endTime, setEndTime] = React.useState('12:00');
  // const [sessionHolders, setSessionHolders] = React.useState(1);
  // const [appointmentLength, setAppointmentLength] = React.useState(5);
  // const [selectedServices, setSelectedServices] = React.useState<string[]>([]);
  // const [targetBlock, setTargetBlock] =
  //   React.useState<AvailabilityBlock | null>(null);
  // const [showUnsavedChangesMessage, setShowUnsavedChangesMessage] =
  //   React.useState(false);

  // const hasErrors = React.useMemo(
  //   () =>
  //     errors.time !== undefined ||
  //     errors.services !== undefined ||
  //     errors.break !== undefined,
  //   [errors],
  // );

  // const addBreak = () => {
  //   if (checkForUnsavedChanges()) {
  //     setShowUnsavedChangesMessage(true);
  //   } else {
  //     setShowUnsavedChangesMessage(false);
  //     setTargetBlock({
  //       day,
  //       start: '09:00',
  //       end: '10:00',
  //       appointmentLength: 5,
  //       sessionHolders: 0,
  //       services: [],
  //       isBreak: true,
  //       isPreview: true,
  //     });
  //   }
  // };

  // const addSession = () => {
  //   if (checkForUnsavedChanges()) {
  //     setShowUnsavedChangesMessage(true);
  //   } else {
  //     setShowUnsavedChangesMessage(false);
  //     setTargetBlock({
  //       day,
  //       start: '09:00',
  //       end: '10:00',
  //       appointmentLength: 5,
  //       sessionHolders: 1,
  //       services: [],
  //       isPreview: true,
  //       isBreak: false,
  //     });
  //   }
  // };

  // const cancelChanges = () => {
  //   setTargetBlock(null);
  //   setShowUnsavedChangesMessage(false);
  //   setErrors({});
  // };

  // const backToWeek = () => {
  //   const weekNumber = Math.floor(day.diff(dayjs('2024-01-01'), 'day') / 7) + 1;
  //   const params = new URLSearchParams(searchParams);
  //   params.delete('date');
  //   params.delete('block');
  //   params.set('wn', weekNumber.toString());

  //   replace(`${pathname.replace('/session', '')}?${params.toString()}`);
  // };

  // const checkForUnsavedChanges = () =>
  //   targetBlock &&
  //   (targetBlock?.isPreview ||
  //     targetBlock?.start != startTime ||
  //     targetBlock?.end != endTime ||
  //     targetBlock.sessionHolders != sessionHolders ||
  //     targetBlock?.services != selectedServices);

  // const validate = () => {
  //   const err = {} as Errors;
  //   const st = timeAsInt(startTime);
  //   const et = timeAsInt(endTime);

  //   if (et !== 0 && et <= st) {
  //     err.time = 'The start time must be earlier than the end time.';
  //   } else if (conflictBlock) {
  //     const hit = blocks.find(b => b.start === conflictBlock);
  //     if (hit)
  //       err.time = `A conflicting session already exists between ${hit.start} and ${hit.end}`;
  //   }

  //   if (selectedServices.length == 0 && !targetBlock?.isBreak) {
  //     err.services = 'You must select at least one service.';
  //   }

  //   if (targetBlock?.isBreak) {
  //     if (
  //       blocks.filter(b => !b.isBreak && isWithin(targetBlock, b)).length == 0
  //     ) {
  //       err.break = 'Breaks must exist within a session.';
  //     }
  //   }

  //   setErrors(err);
  //   return (
  //     err.time === undefined &&
  //     err.services === undefined &&
  //     err.break === undefined
  //   );
  // };

  // const save = () => {
  //   if (validate()) {
  //     saveBlock(
  //       {
  //         day,
  //         start: startTime,
  //         end: endTime,
  //         appointmentLength,
  //         sessionHolders,
  //         services: selectedServices,
  //         isBreak: targetBlock?.isBreak,
  //       },
  //       targetBlock !== null && !targetBlock.isPreview
  //         ? targetBlock
  //         : undefined,
  //     );
  //     setShowUnsavedChangesMessage(false);
  //     setTargetBlock(null);
  //   }
  // };

  // const edit = (bl: AvailabilityBlock) => {
  //   if (checkForUnsavedChanges()) {
  //     setShowUnsavedChangesMessage(true);
  //   } else {
  //     setShowUnsavedChangesMessage(false);
  //     setTargetBlock(bl);
  //   }
  // };

  // const dayBlocks = React.useMemo(() => {
  //   return blocks.filter(b => b.day.isSame(day)).toSorted(timeSort);
  // }, [blocks, day]);

  // const editAction = {
  //   title: 'Edit',
  //   action: edit,
  //   test: (b: AvailabilityBlock) => !b.isPreview,
  // };

  const removeAction = {
    title: 'Delete',
    action: (bl: AvailabilityBlock) => {
      removeBlock(bl);
    },
    test: (b: AvailabilityBlock) => !b.isPreview,
  };

  // const targetBlockAppointments = React.useMemo(
  //   () =>
  //     calculateNumberOfAppointments(
  //       {
  //         day,
  //         start: startTime,
  //         end: endTime,
  //         appointmentLength,
  //         services: [],
  //         sessionHolders,
  //         isBreak: false,
  //       },
  //       blocks,
  //     ),
  //   [startTime, endTime, appointmentLength, sessionHolders, day, blocks],
  // );

  // React.useEffect(() => {
  //   if (targetBlock) {
  //     setStartTime(targetBlock.start);
  //     setEndTime(targetBlock.end);
  //     setSessionHolders(targetBlock.sessionHolders);
  //     setSelectedServices(targetBlock.services);
  //     setAppointmentLength(targetBlock.appointmentLength);
  //   }
  // }, [targetBlock]);

  // React.useEffect(() => {
  //   const test = { start: startTime, end: endTime };
  //   const hit = dayBlocks.find(
  //     b => b.isBreak === targetBlock?.isBreak && conflictsWith(b, test),
  //   );
  //   setConflictBlock(hit?.start);
  // }, [startTime, endTime, targetBlock, blocks, dayBlocks]);

  const yesterday = parsedDate.subtract(1, 'day');
  const tomorrow = parsedDate.add(1, 'day');

  return (
    <>
      <Pagination
        previous={{
          title: yesterday.format('DD MMMM YYYY'),
          href: `/site/${site.id}/availability/day-two?date=${formatDateForUrl(yesterday)}`,
        }}
        next={{
          title: tomorrow.format('DD MMMM YYYY'),
          href: `/site/${site.id}/availability/day-two?date=${formatDateForUrl(tomorrow)}`,
        }}
      />

      {/* {bannerError && <ErrorSummaryCard {...bannerError} />}
      <ErrorSummaryCard
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
      <CardWithBlueHeader title="Schedule Preview">
        <DaySummary
          blocks={blocks}
          showBreaks={true}
          hasError={() => false}
          // primaryAction={editAction}
          secondaryAction={removeAction}
        />
        <AppointmentsSummaryText total={84} />
      </CardWithBlueHeader>
      <ButtonGroup>
        <Button type="submit">Confirm schedule</Button>
        <Button type="button" styleType="secondary">
          Cancel
        </Button>
      </ButtonGroup>
      <Link href="#maincontent">Back to top</Link>
    </>
  );
};

export default DayViewPage;
