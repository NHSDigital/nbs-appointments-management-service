import 'dayjs/locale/en-gb'
import 'react-big-calendar/lib/css/react-big-calendar.css'
import React from 'react';
import dayjs from 'dayjs';
import { Calendar, dayjsLocalizer, Views, View } from 'react-big-calendar';
import { useSiteContext } from '../ContextProviders/SiteContextProvider';
import { SiteConfiguration, Booking, DayOfWeek, TemplateAssignment, WeekTemplate} from '../Types/index';
import { useTemplateService } from '../Services/TemplateService';
import { useBookingService } from '../Services/BookingService';


type Event = {
    id?: string;
    from: Date;
    until: Date;
    title?: string;
    isBackgroundEvent?: boolean
}

const clusterBookings = (bookings: Booking[]): Event[] => {
    bookings.sort((a, b) => {
        return dayjs(a.from).isAfter(dayjs(b.from)) ? 1 : -1;
    })
    let clusters = [], temp = [];
    for (let i = 0; i <= bookings.length - 1; i++) {
        const startTime = dayjs(bookings[i].from);
        const lastEntry = temp[temp.length - 1];
        if (lastEntry) {
            const lastEnd = dayjs(lastEntry.from).add(lastEntry.duration, "minute");
            if (!startTime.isBefore(lastEnd.add(5, "minutes"))) {
                clusters.push(temp);
                temp = [];
            }
        }
        temp.push(bookings[i]);
    }
    if (temp.length) {
        clusters.push(temp);
    }

    return clusters.map(c => {
        const firstFrom = dayjs(c[0].from).toDate();
        const last = c[c.length - 1];
        const lastUntil = dayjs(last.from).add(last.duration, "minute").toDate();
        return { from: firstFrom, until: lastUntil, title: `${c.length} booking${c.length > 1 ? "s" : ""}` };
    });

}

const assignedTemplatesToBGEvents = (start: Date, end: Date, assignments:TemplateAssignment[], templates: WeekTemplate[]) => {
    //days array matches days to index returned by dayjs().day() - make locale aware?
    const days: DayOfWeek[] = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
    let events: Event[] = [];
    let day = dayjs(start);
    const endVal = end.valueOf();
    while(day.valueOf() < endVal){
        const [assignment] = assignments.filter(a => (day.isAfter(a.from) || day.isSame(a.from)) && (day.isBefore(a.until) || day.isSame(a.until)));
        if(assignment){
            const [template] = templates.filter(t => t.id === assignment.templateId);
            const scheduleBlocks = template.items.filter(i => i.days.includes(days[day.day()])).flatMap(i => i.scheduleBlocks);
            const formattedDate = day.format("YYYY/MM/DD");
            const converted = scheduleBlocks.map(sb => ({from: dayjs(`${formattedDate} ${sb.from}`).toDate(), until: dayjs(`${formattedDate} ${sb.until}`).toDate()}));
            events = events.concat(converted)
        }
        day = day.add(1, "day");
    }
    return events;
}

dayjs.locale("en-gb");
const localizer = dayjsLocalizer(dayjs);

export const AppointmentsCalendarCtx = () => {
    const { siteConfig } = useSiteContext();
    const {getTemplates,getAssignments} = useTemplateService();
    const { getBookings } = useBookingService();
    return <AppointmentsCalendar
                siteConfig={siteConfig!}
                getTemplates={() => getTemplates(siteConfig?.siteId!)}
                getAssignments={() => getAssignments(siteConfig?.siteId!)}
                getBookings={(from: Date, to: Date) => getBookings(siteConfig?.siteId!, from, to)}
            />
}

type ApptCalendarProps = {
    siteConfig: SiteConfiguration,
    getTemplates: () => Promise<WeekTemplate[]>
    getAssignments: () => Promise<TemplateAssignment[]>
    getBookings: (from:Date, to: Date) => Promise<Booking[]>
}

export const AppointmentsCalendar = ({ siteConfig, getTemplates, getAssignments, getBookings }: ApptCalendarProps) => {

    const [bookings, setBookings] = React.useState<Booking[]>([]);
    const [availability, setAvailability] = React.useState<Event[]>();
    const [templates, setTemplates] = React.useState<WeekTemplate[]>([]);
    const [assignments, setAssignments] = React.useState<TemplateAssignment[]>([])
    const [selectedBookings, setSelectedBookings] = React.useState<Booking[]>([]);


    const codeToDisplayNameMap = siteConfig?.serviceConfiguration.reduce((prv, cur) => {
        return { ...prv, [cur.code]: cur.displayName }
    }, {} as { [key: string]: string });

    React.useEffect(() => {
        getTemplates().then((templates) => {
            setTemplates(templates);
            getAssignments().then((assignments) => {
                assignments.sort((a,b) => a.from > b.from ? 1 : -1);
                setAssignments(assignments);
                const startOfWeek = dayjs().startOf("week").toDate();
                const endOfWeek = dayjs().endOf("week").toDate();
                const loadedAvailability = assignedTemplatesToBGEvents(startOfWeek, endOfWeek, assignments, templates);
                setAvailability(loadedAvailability);
                getBookings(startOfWeek, endOfWeek).then(bookings => {
                    setBookings(bookings);
                });
              })
          });
    }, [siteConfig?.siteId]);

    const showBookings = (calendarEvent: Event, reactEvent: React.SyntheticEvent) => {
        const bookingsToShow = bookings?.filter(b => {
            const bookingFrom = dayjs(b.from);
            return bookingFrom.isSame(calendarEvent.from) || (bookingFrom.isAfter(calendarEvent.from) && bookingFrom.isBefore(calendarEvent.until));
        });
        setSelectedBookings(bookingsToShow ? [...bookingsToShow] : []);
    }

    const handleNavigate = (newDate: Date, view: View, action: any) => {
        //get bookings for week navigated to
        const startOfWeek = dayjs(newDate).startOf("week");
        const endOfWeek = startOfWeek.add(1, "week").toDate();
        const existingBookings = bookings.filter(b => dayjs(b.from).isAfter(startOfWeek) && dayjs(b.from).isBefore(endOfWeek));
        if(!existingBookings.length){
            getBookings(startOfWeek.toDate(), endOfWeek).then(newBookings => {
                setBookings([...bookings, ...newBookings]);
            });
        }
        if (view === "week") {
            const eventArray = assignedTemplatesToBGEvents(startOfWeek.toDate(), endOfWeek, assignments, templates);
            setAvailability(eventArray);
        }
    }

    const min = availability?.reduce((rv, curr) => {
        return curr.from.getHours() < rv.getHours() ? curr.from : rv;
    }, availability[0]?.from);

    const max = availability?.reduce((rv, curr) => {
        return curr.until.getHours() > rv.getHours() ? curr.until : rv;
    }, availability[0]?.until);

    return (
        <div className="nhsuk-grid-row">
            <div className="nhsuk-grid-column-two-thirds">
                {/* TODO - rbc recommends to memoize/useCallback props that change */}
                <Calendar
                    dayLayoutAlgorithm="no-overlap"
                    localizer={localizer}
                    events={clusterBookings(bookings ?? [])}
                    backgroundEvents={availability}
                    startAccessor={"from"}
                    endAccessor={"until"}
                    style={{ height: 600 }}
                    //scrollToTime={new Date()}
                    step={15}
                    timeslots={1}
                    min={min}
                    max={max}
                    views={[Views.WEEK]}
                    defaultView={Views.WEEK}
                    formats={{
                        timeGutterFormat: (date, culture, localizer) => {
                            return localizer!.format(date, "HH:mm", culture);
                        }
                    }}
                    onNavigate={handleNavigate}
                    onSelectEvent={showBookings}
                />
            </div>
            <div className="nhsuk-grid-column-one-third">
                <div style={{height: 600, overflow: "auto"}}>
                    {!selectedBookings.length ? <div>Select a block of bookings to see more details</div> :
                        <table className="nhsuk-table">
                            <caption className="nhsuk-table__caption">Selected bookings on {dayjs(selectedBookings[0].from).format("DD/MM/YYYY")}</caption>
                            <thead className="nhsuk-table__head">
                                <tr role="row">
                                    <th role="columnheader" scope="col">
                                        Time
                                    </th>
                                    <th role="columnheader" scope="col">
                                        Name
                                    </th>
                                    <th role="columnheader" scope="col">
                                        Booking Ref.
                                    </th>
                                    <th role="columnheader" scope="col">
                                        Service
                                    </th>
                                </tr>
                            </thead>
                            <tbody className="nhsuk-table__body">
                                {selectedBookings.map(b => {
                                    return <tr key={b.reference}>
                                        <td>{dayjs(b.from).format("HH:mm")}</td>
                                        <td>{`${b.attendeeDetails.lastName}, ${b.attendeeDetails.firstName}`}</td>
                                        <td>{b.reference}</td>
                                        <td>{codeToDisplayNameMap[b.service]}</td>
                                    </tr>
                                })}
                            </tbody>
                        </table>
                    }
                </div>
            </div>
        </div>
    )
}