import React from "react";
import { useSiteContext } from "../ContextProviders/SiteContextProvider";
import { Booking, SiteConfiguration } from "../Types";
import { useBookingService } from "../Services/BookingService";
import { When } from "../Components";
import dayjs from "dayjs";

type DailyBookingsProps = {
    siteConfig: SiteConfiguration,
    getBookings: (siteId: string, from: Date, to: Date) => Promise<Booking[]>,
    setBookingStatus: (site: string, bookingRef: string, status: string) => Promise<unknown>
};

export const DailyBookingsCtx = () => {
    const { siteConfig } = useSiteContext();
    const { getBookings, setBookingStatus } = useBookingService();
    
    return <DailyBookings siteConfig={siteConfig!} getBookings={getBookings} setBookingStatus={setBookingStatus} />
}

export const DailyBookings = ({ siteConfig, getBookings, setBookingStatus }: DailyBookingsProps) => {

    const [bookingsList, setBookingsList] = React.useState<Booking[] | null>(null);
    const [currentDay, setCurrentDay] = React.useState(new Date());
    const [status, setStatus] = React.useState<null | "loading" | "errored">();
    const [filterTerm, setFilterTerm] = React.useState("");

    const codeToDisplayNameMap = siteConfig?.serviceConfiguration.reduce((prv, cur) => {
        return { ...prv, [cur.code]: cur.displayName }
    }, {} as { [key: string]: string });


    React.useEffect(() => {
        setStatus("loading");
        const from = dayjs(currentDay).startOf("day");
        const to = from.endOf("day");
        getBookings(siteConfig?.siteId, from.toDate(), to.toDate()).then(bookings => {
            setBookingsList(bookings);
            setStatus(null);
        }).catch(e => {
            setStatus("errored");
            console.log(e.message);
        })
    }, [siteConfig, currentDay, getBookings]);

    const toggleCheckedIn = (reference: string) => {
        const booking = bookingsList?.find(b => b.reference === reference);
        const newStatus = booking?.outcome === "CheckedIn" ? "Waiting": "CheckedIn";
        setStatus("loading");
        setBookingStatus(siteConfig.siteId, reference, newStatus)
            .then(() => {
                const bookings = bookingsList!.map(b => {
                    if(b.reference === reference){
                        b.outcome = newStatus;
                    }
                    return b;
                })
                setBookingsList(bookings);
                setStatus(null);
            })
    }

    const gotoNextDay = () => {
        const newDate = dayjs(currentDay).add(1, "day");
        setCurrentDay(newDate.toDate());
    }

    const gotoPreviousDay = () => {
        const newDate = dayjs(currentDay).subtract(1, "day");
        setCurrentDay(newDate.toDate());
    }

    const gotoToday = () => {
        setCurrentDay(new Date());
    }

    const isShowingToday = React.useMemo(() => dayjs(currentDay).isSame(new Date(), "day"), [currentDay])

    const filteredList = React.useMemo(() => {
        return bookingsList?.sort((a, b) => {
            // sort by time
            return a.from.localeCompare(b.from);
        })
        .filter((appt) => {
            if (filterTerm) {
            // Non-numeric values match against a combined name string so "John Smith" matches "John Smith" or "Smith, John".
            // Strips hyphens from input and reference so they match, but still works for hyphenated names
            const noHyphenTerm = filterTerm.replaceAll("-", "");
            return isNaN(Number(noHyphenTerm))
                ? filterTerm.toLowerCase()
                    .replaceAll(/[^a-zA-Z '-]+/g, "")
                    .split(" ")
                    .map((subterm) => {
                    return (
                        appt.attendeeDetails.firstName.toLowerCase() +
                        " " +
                        appt.attendeeDetails.lastName.toLowerCase()
                    ).includes(subterm);
                    })
                    .every((x) => x)
                : appt.reference.replaceAll("-", "").includes(noHyphenTerm);
            }
            return true;
        });
    }, [filterTerm, bookingsList]);

    return (
        <>
            <When condition={!siteConfig}>
                <div>No site config loaded</div>
            </When>
            <When condition={status === "errored"}>
                <div
                    className="nhsuk-error-summary"
                    aria-labelledby="error-summary-title"
                    role="alert"
                    tabIndex={-1}
                >
                    <h2 className="nhsuk-error-summary__title" id="error-summary-title">
                        <span className="nhsuk-u-visually-hidden">Error:</span>
                        There is a problem
                    </h2>
                    <div className="nhsuk-error-summary__body">
                        {/* <p>There has been a server error, please try again</p> */}
                        <ul className="nhsuk-list nhsuk-error-summary__list">
                            <li>
                                <a href="#!">Unable to get bookings</a>
                            </li>
                        </ul>
                    </div>
                </div>
            </When>
            <table className="nhsuk-table">
                <caption className="nhsuk-table__caption">
                    {dayjs(currentDay).format("DD/MM/YYYY - dddd")}
                    <div className="flex-row-start-nowrap" style={{marginLeft: "-14px"}}>
                        <div className="nhsuk-back-link">
                            <a className="nhsuk-back-link__link" aria-label="previous day" href="#!" onClick={gotoPreviousDay}>
                            <svg viewBox="0 0 32 32" width="24" height="24" version="1.1" xmlns="http://www.w3.org/2000/svg" style={{float: "left"}}>
                                    <g transform="translate(-423.000000, -1193.000000)" fill="#0066cc">
                                        <path d="M428.115,1209 L437.371,1200.6 C438.202,1199.77 438.202,1198.43 437.371,1197.6 C436.541,1196.76 435.194,1196.76 434.363,1197.6 L423.596,1207.36 C423.146,1207.81 422.948,1208.41 422.985,1209 C422.948,1209.59 423.146,1210.19 423.596,1210.64 L434.363,1220.4 C435.194,1221.24 436.541,1221.24 437.371,1220.4 C438.202,1219.57 438.202,1218.23 437.371,1217.4 L428.115,1209"></path>
                                    </g>
                            </svg>
                                Previous day
                            </a>
                        </div>
                        <div className="nhsuk-back-link">
                            <a className="nhsuk-back-link__link" aria-label="today" href="#!" onClick={gotoToday}>
                                Today
                            </a>
                        </div>
                        <div className="nhsuk-back-link">
                        <a className="nhsuk-back-link__link" aria-label="next day" href="#!" onClick={gotoNextDay}>
                            Next day
                            <svg viewBox="0 0 32 32" width="24" height="24" style={{float: "right", marginLeft: "8px"}} version="1.1" xmlns="http://www.w3.org/2000/svg">
                                <g transform="translate(-474.000000, -1229.000000) scale(1,1.03)" fill="#0066cc">
                                    <path d="M488.404,1207.36 L477.637,1197.6 C476.806,1196.76 475.459,1196.76 474.629,1197.6 C473.798,1198.43 473.798,1199.77 474.629,1200.6 L483.885,1209 L474.629,1217.4 C473.798,1218.23 473.798,1219.57 474.629,1220.4 C475.459,1221.24 476.806,1221.24 477.637,1220.4 L488.404,1210.64 C488.854,1210.19 489.052,1209.59 489.015,1209 C489.052,1208.41 488.854,1207.81 488.404,1207.36"></path>
                                    </g>
                            </svg>
                            </a>
                        </div>
                    </div>
                    <div className="nhsuk-hint">
                        Search by name or booking reference
                    </div>
                    <div className="nhsuk-form-group">
                        <input type="text" className="nhsuk-input nhsuk-u-width-one-quarter" aria-label="Filter bookings list"
                            value={filterTerm} onChange={e => setFilterTerm(e.target.value)}/>
                    </div>
                </caption>
                {filteredList?.length ?
                    <thead className="nhsuk-table__head">
                        <tr role="row">
                            <th role="columnheader" scope="col">
                                Time
                            </th>
                            <th role="columnheader" scope="col">
                                Last Name
                            </th>
                            <th role="columnheader" scope="col">
                                First Name
                            </th>
                            <th role="columnheader" scope="col">
                                Booking Ref.
                            </th>
                            <th role="columnheader" scope="col">
                                NHS No.
                            </th>
                            <th role="columnheader" scope="col">
                                DOB
                            </th>
                            <th role="columnheader" scope="col">
                                Service
                            </th>
                            <When condition={isShowingToday}>
                                <th role="columnheader" scope="col">
                                    Checked In
                                </th>
                            </When>
                        </tr>
                    </thead> : <thead><tr><td>{filterTerm ? "No results found" : "No bookings on this day"}</td></tr></thead>
                }
                <tbody className="nhsuk-table__body">
                    {filteredList?.map(booking =>
                        <tr key={booking.reference}>
                            <td>{dayjs(booking.from).format("HH:mm")}</td>
                            <td>{booking.attendeeDetails.lastName}</td>
                            <td>{booking.attendeeDetails.firstName}</td>
                            <td>{booking.reference}</td>
                            <td>{booking.attendeeDetails.nhsNumber}</td>
                            <td>{dayjs(booking.attendeeDetails.dateOfBirth).format("DD/MM/YYYY")}</td>
                            <td>{codeToDisplayNameMap[booking.service]}</td>
                            <When condition={isShowingToday}>
                                <td>
                                    <div className="nhsuk-checkboxes__item">
                                        <input className="nhsuk-checkboxes__input" type="checkbox" id={booking.reference}
                                            value={booking.outcome ?? ""}
                                            onChange={() => {toggleCheckedIn(booking.reference)}}
                                            checked={booking.outcome === "CheckedIn"} />
                                        <label className="nhsuk-label nhsuk-checkboxes__label" htmlFor={booking.reference}>
                                        </label>
                                    </div>
                                </td>
                            </When>
                        </tr>
                    )}
                </tbody>
            </table>
            <When condition={status === "loading"}>
                <div>Loading...</div>
            </When>
        </>
    )
}