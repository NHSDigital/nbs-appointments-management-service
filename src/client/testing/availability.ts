type ServiceOverview = {
  serviceName: string;
  bookedAppointments: number;
};

export type WeekOverview = {
  header: string;
  sessions: ServiceOverview[];
  totalAppointments: number;
  booked: number;
  unbooked: number;
};

export type DayOverview = {
  header: string;
  sessions: DaySessionOverview[];
  totalAppointments: number;
  booked: number;
  unbooked: number;
  orphaned: number;
};

export type DaySessionOverview = {
  sessionTimeInterval: string;
  serviceName: string;
  booked: string;
  unbooked: number;
};

export type RemovedServicesOverview = {
  date: string;
  sessionTimeInterval: string;
  serviceNames: string;
};

type SessionTestCase = {
  week: string;
  day: string;
  dayCardHeader: string;
  changeSessionHeader: string;
  timeRange: string;
  startHour: string;
  startMins: string;
  endHour: string;
  endMins: string;
  service: string;
  booked: number;
  unbooked: number;
  //the contents of these two SHOULD be identical, but there seems to be some indiscrepancies over these pages...
  viewDailyAppointments: ViewDailyAppointment[];
  cancelDailyAppointments: CancelDailyAppointment[];
};

type WeekViewTestCase = {
  week: string;
  weekHeader: string;
  previousWeek: string;
  nextWeek: string;
  dayOverviews: DayOverview[];
};

type ViewDailyAppointment = {
  time: string;
  nameNhsNumber: string;
  dob: string;
  contactDetails: string;
  services: string;
};

type CancelDailyAppointment = {
  time: string;
  name: string;
  nhsNumber: string;
  dob: string;
  contactDetails: string;
  services: string;
};

//update this number to stop datetime test collisionsß∂
//TODO this needs remedying ASAP as this is crap.
export const staticHackyDayIncrementToBump = 119;

//session test cases to verify, should have session&booking data in the seeder
export const sessionTestCases: SessionTestCase[] = [
  {
    week: '2026-04-20',
    day: '2026-04-25',
    dayCardHeader: 'Saturday 25 April',
    changeSessionHeader: '25 April 2026',
    timeRange: '10:00 - 17:00',
    startHour: '10',
    startMins: '00',
    endHour: '17',
    endMins: '00',
    service: 'RSV Adult',
    booked: 2,
    unbooked: 418,
    viewDailyAppointments: [
      {
        time: '10:00',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV Adult',
      },
      {
        time: '16:55',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV Adult',
      },
    ],
    cancelDailyAppointments: [
      {
        time: '25 April 202610:00am',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV Adult',
      },
      {
        time: '25 April 202616:55pm',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV Adult',
      },
    ],
  },
  {
    week: '2026-04-20',
    day: '2026-04-26',
    dayCardHeader: 'Sunday 26 April',
    changeSessionHeader: '26 April 2026',
    timeRange: '10:00 - 17:00',
    startHour: '10',
    startMins: '00',
    endHour: '17',
    endMins: '00',
    service: 'RSV Adult',
    booked: 2,
    unbooked: 418,
    viewDailyAppointments: [
      {
        time: '10:00',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV Adult',
      },
      {
        time: '16:55',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV Adult',
      },
    ],
    cancelDailyAppointments: [
      {
        time: '26 April 202610:00am',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV Adult',
      },
      {
        time: '26 April 202616:55pm',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV Adult',
      },
    ],
  },
  {
    week: '2026-04-27',
    day: '2026-04-27',
    dayCardHeader: 'Monday 27 April',
    changeSessionHeader: '27 April 2026',
    timeRange: '10:00 - 17:00',
    startHour: '10',
    startMins: '00',
    endHour: '17',
    endMins: '00',
    service: 'RSV Adult',
    booked: 2,
    unbooked: 418,
    viewDailyAppointments: [
      {
        time: '10:00',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV Adult',
      },
      {
        time: '16:55',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV Adult',
      },
    ],
    cancelDailyAppointments: [
      {
        time: '27 April 202610:00am',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV Adult',
      },
      {
        time: '27 April 202616:55pm',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV Adult',
      },
    ],
  },
  {
    week: '2026-03-23',
    day: '2026-03-28',
    dayCardHeader: 'Saturday 28 March',
    changeSessionHeader: '28 March 2026',
    timeRange: '08:00 - 14:00',
    startHour: '08',
    startMins: '00',
    endHour: '14',
    endMins: '00',
    service: 'RSV Adult',
    booked: 2,
    unbooked: 238,
    viewDailyAppointments: [
      {
        time: '08:00',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV Adult',
      },
      {
        time: '13:45',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV Adult',
      },
    ],
    cancelDailyAppointments: [
      {
        time: '28 March 20268:00am',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV Adult',
      },
      {
        time: '28 March 202613:45pm',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV Adult',
      },
    ],
  },
  {
    week: '2026-03-23',
    day: '2026-03-29',
    dayCardHeader: 'Sunday 29 March',
    changeSessionHeader: '29 March 2026',
    timeRange: '08:00 - 14:00',
    startHour: '08',
    startMins: '00',
    endHour: '14',
    endMins: '00',
    service: 'RSV Adult',
    booked: 2,
    unbooked: 238,
    viewDailyAppointments: [
      {
        time: '08:00',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV Adult',
      },
      {
        time: '13:45',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV Adult',
      },
    ],
    cancelDailyAppointments: [
      {
        time: '29 March 20268:00am',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV Adult',
      },
      {
        time: '29 March 202613:45pm',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV Adult',
      },
    ],
  },
  {
    week: '2026-03-30',
    day: '2026-03-30',
    dayCardHeader: 'Monday 30 March',
    changeSessionHeader: '30 March 2026',
    timeRange: '08:00 - 14:00',
    startHour: '08',
    startMins: '00',
    endHour: '14',
    endMins: '00',
    service: 'RSV Adult',
    booked: 2,
    unbooked: 238,
    viewDailyAppointments: [
      {
        time: '08:00',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV Adult',
      },
      {
        time: '13:45',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV Adult',
      },
    ],
    cancelDailyAppointments: [
      {
        time: '30 March 20268:00am',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV Adult',
      },
      {
        time: '30 March 202613:45pm',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV Adult',
      },
    ],
  },
];

export const weekTestCases: WeekViewTestCase[] = [
  {
    week: '2027-03-22',
    weekHeader: '22 March to 28 March',
    previousWeek: '15-21 March 2027',
    nextWeek: '29-4 April 2027',
    dayOverviews: [
      {
        header: 'Monday 22 March',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Tuesday 23 March',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Wednesday 24 March',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Thursday 25 March',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Friday 26 March',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Saturday 27 March',
        sessions: [
          {
            serviceName: 'RSV Adult',
            booked: '2 booked',
            unbooked: 238,
            sessionTimeInterval: '08:00 - 14:00',
          },
        ],
        totalAppointments: 240,
        booked: 2,
        unbooked: 238,
        orphaned: 0,
      },
      {
        header: 'Sunday 28 March',
        sessions: [
          {
            serviceName: 'RSV Adult',
            booked: '2 booked',
            unbooked: 238,
            sessionTimeInterval: '08:00 - 14:00',
          },
        ],
        totalAppointments: 240,
        booked: 2,
        unbooked: 238,
        orphaned: 0,
      },
    ],
  },
  {
    week: '2027-03-29',
    weekHeader: '29 March to 4 April',
    previousWeek: '22-28 March 2027',
    nextWeek: '5-11 April 2027',
    dayOverviews: [
      {
        header: 'Monday 29 March',
        sessions: [
          {
            serviceName: 'RSV Adult',
            booked: '2 booked',
            unbooked: 238,
            sessionTimeInterval: '08:00 - 14:00',
          },
        ],
        totalAppointments: 240,
        booked: 2,
        unbooked: 238,
        orphaned: 0,
      },
      {
        header: 'Tuesday 30 March',
        sessions: [
          {
            serviceName: 'RSV Adult',
            booked: '2 booked',
            unbooked: 238,
            sessionTimeInterval: '08:00 - 14:00',
          },
        ],
        totalAppointments: 240,
        booked: 2,
        unbooked: 238,
        orphaned: 0,
      },
      {
        header: 'Wednesday 31 March',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Thursday 1 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Friday 2 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Saturday 3 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Sunday 4 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
    ],
  },
  {
    week: '2027-04-19',
    weekHeader: '19 April to 26 April',
    previousWeek: '12-18 April 2027',
    nextWeek: '26-2 May 2027',
    dayOverviews: [
      {
        header: 'Monday 19 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Tuesday 20 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Wednesday 21 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Thursday 22 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Friday 23 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Saturday 24 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Sunday 25 April',
        sessions: [
          {
            serviceName: 'RSV Adult',
            booked: '2 booked',
            unbooked: 418,
            sessionTimeInterval: '10:00 - 17:00',
          },
        ],
        totalAppointments: 420,
        booked: 2,
        unbooked: 418,
        orphaned: 0,
      },
    ],
  },
  {
    week: '2027-04-26',
    weekHeader: '26 April to 2 May',
    previousWeek: '19-25 April 2027',
    nextWeek: '3-9 May 2027',
    dayOverviews: [
      {
        header: 'Monday 26 April',
        sessions: [
          {
            serviceName: 'RSV Adult',
            booked: '2 booked',
            unbooked: 418,
            sessionTimeInterval: '10:00 - 17:00',
          },
        ],
        totalAppointments: 420,
        booked: 2,
        unbooked: 418,
        orphaned: 0,
      },
      {
        header: 'Tuesday 27 April',
        sessions: [
          {
            serviceName: 'RSV Adult',
            booked: '2 booked',
            unbooked: 418,
            sessionTimeInterval: '10:00 - 17:00',
          },
        ],
        totalAppointments: 420,
        booked: 2,
        unbooked: 418,
        orphaned: 0,
      },
      {
        header: 'Wednesday 28 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Thursday 29 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Friday 30 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Saturday 1 May',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Sunday 2 May',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
    ],
  },
];
