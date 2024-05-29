import { act, screen } from '@testing-library/react';
import userEvent from "@testing-library/user-event";
import { DailyBookings } from './DailyBookings';
import { SiteConfiguration, defaultServiceConfigurationTypes } from '../Types/SiteConfiguration';
import { Site } from '../Types/Site';
import { Booking } from '../Types/Booking';
import { wrappedRender } from '../utils';
import dayjs from 'dayjs';

const site: Site = { name: "Test Site", id: "1", address: "123" };
const siteConfig: SiteConfiguration = { siteId: site.id, informationForCitizen: "", serviceConfiguration: defaultServiceConfigurationTypes };
const testBooking: Booking = {
    reference: "123",
    from: new Date().toString(),
    duration: 10,
    service: "COVID:_18-74",
    site: "1",
    sessionHolder: "default",
    outcome: "",
    attendeeDetails: {
        nhsNumber: "0123456789",
        firstName: "First",
        lastName: "Last",
        dateOfBirth: "1990-01-01"
    }
}

const expectText = async (text: string) => {
    const element = await screen.findByText(text);
    expect(element).toBeVisible();
}

describe("<DailyBookings />", () => {
    it("calls getBookings with site id and displays response", () => {
        const mockGetBookings = jest.fn().mockResolvedValue([testBooking]);
        wrappedRender(<DailyBookings siteConfig={siteConfig} getBookings={mockGetBookings} setBookingStatus={jest.fn()}/>);
        expectText(testBooking.reference);
        expectText(testBooking.attendeeDetails.firstName);
        expectText(testBooking.attendeeDetails.lastName);
        expectText(testBooking.attendeeDetails.nhsNumber);
        const expectedStart = new Date(), expectedEnd = new Date();
        expectedStart.setHours(0, 0, 0, 0);
        expectedEnd.setHours(23, 59, 59, 999);
        expect(mockGetBookings).toHaveBeenCalledWith(site.id, expectedStart, expectedEnd);
    });

    it("shows appropriate message when no bookings are retrieved", () => {
        const mockGetBookings = jest.fn().mockResolvedValue([]);
        wrappedRender(<DailyBookings siteConfig={siteConfig} getBookings={mockGetBookings} setBookingStatus={jest.fn()}/>);
        expectText("No bookings");
    });

    it("shows error state when request for bookings fails", () => {
        const mockGetBookings = jest.fn().mockRejectedValue(new Error("test server error"));
        wrappedRender(<DailyBookings siteConfig={siteConfig} getBookings={mockGetBookings} setBookingStatus={jest.fn()}/>);
        expectText("Unable to get bookings");
    });

    it("filters by name and booking reference correctly", async () => {
        const mockGetBookings = jest.fn().mockResolvedValue([testBooking]);
        wrappedRender(<DailyBookings siteConfig={siteConfig} getBookings={mockGetBookings} setBookingStatus={jest.fn()}/>);
        const searchInput = await screen.findByRole("textbox", { name: "Filter bookings list" });
        await act(async () => {
            userEvent.type(searchInput, "XYZ");
        });
        expect(screen.queryByText(testBooking.reference)).not.toBeInTheDocument();
        expect(screen.getByText("No results found")).toBeVisible();
        await act(async () => {
            userEvent.type(searchInput, testBooking.reference);
        });
        expectText(testBooking.reference);
    });

    it("starts by displays today's date", async () => {
        const date = new Date();
        const expetctedDate = dayjs(date).format("DD/MM/YYYY - dddd")
        const mockGetBookings = jest.fn().mockResolvedValue([testBooking]);
        const mockSetBookingStatus = jest.fn().mockResolvedValue("fakeResponse");
        wrappedRender(<DailyBookings siteConfig={siteConfig} getBookings={mockGetBookings} setBookingStatus={mockSetBookingStatus} />);
        expect(screen.getByText(expetctedDate)).toBeVisible();
    })

    it("allows the user to move to the next day", async () => {
        const date = new Date();
        const nextDay = dayjs(date).add(1, "day");
        const expetctedDate = dayjs(nextDay).format("DD/MM/YYYY - dddd")
        const mockGetBookings = jest.fn().mockResolvedValue([testBooking]);
        const mockSetBookingStatus = jest.fn().mockResolvedValue("fakeResponse");
        wrappedRender(<DailyBookings siteConfig={siteConfig} getBookings={mockGetBookings} setBookingStatus={mockSetBookingStatus} />);
        const nextDayLink = await screen.findByRole("link", {name: "next day"})
        userEvent.click(nextDayLink);
        expect(await screen.findByText(expetctedDate)).toBeVisible();
    })

    it("allows the user to move to the previous day", async () => {
        const date = new Date();
        const nextDay = dayjs(date).subtract(1, "day");
        const expetctedDate = dayjs(nextDay).format("DD/MM/YYYY - dddd")
        const mockGetBookings = jest.fn().mockResolvedValue([testBooking]);
        const mockSetBookingStatus = jest.fn().mockResolvedValue("fakeResponse");
        wrappedRender(<DailyBookings siteConfig={siteConfig} getBookings={mockGetBookings} setBookingStatus={mockSetBookingStatus} />);
        const previousDayLink = await screen.findByRole("link", {name: "previous day"})
        userEvent.click(previousDayLink);
        expect(await screen.findByText(expetctedDate)).toBeVisible();
    })

    it("allows the user to return to the current day", async () => {
        const date = new Date();        
        const expetctedDate = dayjs(date).format("DD/MM/YYYY - dddd")
        const mockGetBookings = jest.fn().mockResolvedValue([testBooking]);
        const mockSetBookingStatus = jest.fn().mockResolvedValue("fakeResponse");
        wrappedRender(<DailyBookings siteConfig={siteConfig} getBookings={mockGetBookings} setBookingStatus={mockSetBookingStatus} />);
        const previousDayLink = await screen.findByRole("link", {name: "previous day"})
        userEvent.click(previousDayLink);
        const todayLink = await screen.findByRole("link", {name: "today"})
        userEvent.click(todayLink);
        expect(await screen.findByText(expetctedDate)).toBeVisible();
    })

    it("hides the check in box when viewing a day other than today", async () => {
        const date = new Date();
        const nextDay = dayjs(date).add(1, "day");
        const expetctedDate = dayjs(nextDay).format("DD/MM/YYYY - dddd")
        const mockGetBookings = jest.fn().mockResolvedValue([testBooking]);
        const mockSetBookingStatus = jest.fn().mockResolvedValue("fakeResponse");
        wrappedRender(<DailyBookings siteConfig={siteConfig} getBookings={mockGetBookings} setBookingStatus={mockSetBookingStatus} />);
        const nextDayLink = await screen.findByRole("link", {name: "next day"})
        userEvent.click(nextDayLink);
        expect(await screen.queryByRole("checkbox")).toBeNull();
    })

    it("changes the status of the booking when Check In box is checked", async ()=> {
        const mockGetBookings = jest.fn().mockResolvedValue([testBooking]);
        const mockSetBookingStatus = jest.fn().mockResolvedValue("fakeResponse");
        wrappedRender(<DailyBookings siteConfig={siteConfig} getBookings={mockGetBookings} setBookingStatus={mockSetBookingStatus} />);
        const firstCheckBox = await screen.findByRole("checkbox");
        await act(async () => {
            userEvent.click(firstCheckBox);
        });
        expect(mockSetBookingStatus).toHaveBeenCalledWith(site.id, testBooking.reference, "CheckedIn" );
        expect(firstCheckBox).toBeChecked();
    });
})
