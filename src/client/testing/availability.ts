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
};

export type DaySessionOverview = {
  sessionTimeInterval: string;
  serviceName: string;
  booked: string;
  unbooked: number;
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
    week: '2025-10-20',
    day: '2025-10-25',
    dayCardHeader: 'Saturday 25 October',
    changeSessionHeader: '25 October 2025',
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
        services: 'RSV',
      },
      {
        time: '16:55',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV',
      },
    ],
    cancelDailyAppointments: [
      {
        time: '25 October 202510:00am',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV Adult',
      },
      {
        time: '25 October 202516:55pm',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV Adult',
      },
    ],
  },
  {
    week: '2025-10-20',
    day: '2025-10-26',
    dayCardHeader: 'Sunday 26 October',
    changeSessionHeader: '26 October 2025',
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
        services: 'RSV',
      },
      {
        time: '16:55',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV',
      },
    ],
    cancelDailyAppointments: [
      {
        time: '26 October 202510:00am',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV Adult',
      },
      {
        time: '26 October 202516:55pm',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV Adult',
      },
    ],
  },
  {
    week: '2025-10-27',
    day: '2025-10-27',
    dayCardHeader: 'Monday 27 October',
    changeSessionHeader: '27 October 2025',
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
        services: 'RSV',
      },
      {
        time: '16:55',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV',
      },
    ],
    cancelDailyAppointments: [
      {
        time: '27 October 202510:00am',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV Adult',
      },
      {
        time: '27 October 202516:55pm',
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
        services: 'RSV',
      },
      {
        time: '13:45',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV',
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
        services: 'RSV',
      },
      {
        time: '13:45',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV',
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
        services: 'RSV',
      },
      {
        time: '13:45',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV',
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
    week: '2025-10-20',
    weekHeader: '20 October to 26 October',
    previousWeek: '13-19 October 2025',
    nextWeek: '27-2 November 2025',
    dayOverviews: [
      {
        header: 'Monday 20 October',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Tuesday 21 October',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Wednesday 22 October',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Thursday 23 October',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Friday 24 October',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Saturday 25 October',
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
      },
      {
        header: 'Sunday 26 October',
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
      },
    ],
  },
  {
    week: '2025-10-27',
    weekHeader: '27 October to 2 November',
    previousWeek: '20-26 October 2025',
    nextWeek: '3-9 November 2025',
    dayOverviews: [
      {
        header: 'Monday 27 October',
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
      },
      {
        header: 'Tuesday 28 October',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Wednesday 29 October',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Thursday 30 October',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Friday 31 October',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Saturday 1 November',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Sunday 2 November',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
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
      },
      {
        header: 'Tuesday 24 March',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Wednesday 25 March',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Thursday 26 March',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Friday 27 March',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
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
      },
      {
        header: 'Tuesday 31 March',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Wednesday 1 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Thursday 2 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Friday 3 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Saturday 4 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Sunday 5 April',
        sessions: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
    ],
  },
];
