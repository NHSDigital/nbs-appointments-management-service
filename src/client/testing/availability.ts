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
    week: '2026-04-20',
    weekHeader: '20 April to 26 April',
    previousWeek: '13-19 April 2026',
    nextWeek: '27 April - 3 May 2026',
    dayOverviews: [
      {
        header: 'Monday 20 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Tuesday 21 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Wednesday 22 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Thursday 23 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Friday 24 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Saturday 25 April',
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
        header: 'Sunday 26 April',
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
    week: '2026-04-27',
    weekHeader: '27 April to 3 May',
    previousWeek: '20-26 April 2026',
    nextWeek: '4-10 May 2026',
    dayOverviews: [
      {
        header: 'Monday 27 April',
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
        header: 'Tuesday 28 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Wednesday 29 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Thursday 30 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Friday 1 May',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Saturday 2 May',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Sunday 3 May',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
    ],
  },
  {
    week: '2026-03-23',
    weekHeader: '23 March to 29 March',
    previousWeek: '16-22 March 2026',
    nextWeek: '30-5 April 2026',
    dayOverviews: [
      {
        header: 'Monday 23 March',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Tuesday 24 March',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Wednesday 25 March',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Thursday 26 March',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Friday 27 March',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Saturday 28 March',
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
        header: 'Sunday 29 March',
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
    week: '2026-03-30',
    weekHeader: '30 March to 5 April',
    previousWeek: '23-29 March 2026',
    nextWeek: '6-12 April 2026',
    dayOverviews: [
      {
        header: 'Monday 30 March',
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
        header: 'Tuesday 31 March',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Wednesday 1 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Thursday 2 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Friday 3 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Saturday 4 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
      {
        header: 'Sunday 5 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
        orphaned: 0,
      },
    ],
  },
];
