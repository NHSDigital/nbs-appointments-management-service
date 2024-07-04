import React from "react";
import { useSiteContext } from "../ContextProviders/SiteContextProvider";
import { Booking } from "../Types/Booking";
import { useBookingService } from "../Services/BookingService";
import { When } from "../Components/When";
import { SiteConfiguration } from "../Types/SiteConfiguration";
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
        getBookings(siteConfig?.site, from.toDate(), to.toDate()).then(bookings => {
            setBookingsList(bookings);
            setStatus(null);
        }).catch(e => {
            setStatus("errored");
            console.log(e.message);
        })
    }, [siteConfig, currentDay, getBookings]);

    const toggleCheckedIn = (reference: string) => {
        const booking = bookingsList?.find(b => b.reference === reference);
        const newStatus = booking?.outcome === "CheckedIn" ? "Waiting" : "CheckedIn";
        setStatus("loading");
        setBookingStatus(siteConfig.site, reference, newStatus)
            .then(() => {
                const bookings = bookingsList!.map(b => {
                    if (b.reference === reference) {
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
                    <div className="nhsuk-navigation">
                        <div className="nhsuk-back-link">
                            <button className="nhsuk-back-link__link" type="button" aria-label="previous day" onClick={gotoPreviousDay}>
                                <svg className="nhsuk-icon nhsuk-icon__chevron-left" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" aria-hidden="true" height="24" width="24">
                                    <path d="M8.5 12c0-.3.1-.5.3-.7l5-5c.4-.4 1-.4 1.4 0s.4 1 0 1.4L10.9 12l4.3 4.3c.4.4.4 1 0 1.4s-1 .4-1.4 0l-5-5c-.2-.2-.3-.4-.3-.7z"></path>
                                </svg>
                                Previous day
                            </button>
                        </div>
                        <div className="nhsuk-back-link">
                            <button className="nhsuk-back-link__link" type="button" aria-label="today" onClick={gotoToday}>
                                Today
                            </button>
                        </div>
                        <div className="nhsuk-back-link">
                            <button className="nhsuk-back-link__link" type="button" aria-label="next day" onClick={gotoNextDay}>
                                Next day
                                <svg className="nhsuk-icon nhsuk-icon__chevron-right" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" aria-hidden="true" height="24" width="24">
                                    <path d="M8.5 12c0-.3.1-.5.3-.7l5-5c.4-.4 1-.4 1.4 0s.4 1 0 1.4L10.9 12l4.3 4.3c.4.4.4 1 0 1.4s-1 .4-1.4 0l-5-5c-.2-.2-.3-.4-.3-.7z"></path>
                                </svg>
                            </button>
                        </div>
                    </div>
                    <div className="nhsuk-hint">
                        Search by name or booking reference
                    </div>
                    <div className="nhsuk-form-group">
                        <input type="text" className="nhsuk-input nhsuk-input--width-20" aria-label="Filter bookings list"
                            value={filterTerm} onChange={e => setFilterTerm(e.target.value)} />
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
                                            onChange={() => { toggleCheckedIn(booking.reference) }}
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