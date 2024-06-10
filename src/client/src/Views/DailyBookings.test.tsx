import {act, screen} from '@testing-library/react';
import userEvent from "@testing-library/user-event";
import { DailyBookings } from './DailyBookings';
import { Site, SiteConfiguration, defaultServiceConfigurationTypes } from '../Types';
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
    it("calls getBookings with site id and displays response", async () => {
        const mockGetBookings = jest.fn().mockResolvedValue([testBooking]);
        wrappedRender(<DailyBookings siteConfig={siteConfig} getBookings={mockGetBookings} setBookingStatus={jest.fn()}/>);
        await expectText(testBooking.reference);
        await expectText(testBooking.attendeeDetails.firstName);
        await expectText(testBooking.attendeeDetails.lastName);
        await expectText(testBooking.attendeeDetails.nhsNumber);
        const expectedStart = new Date(), expectedEnd = new Date();
        expectedStart.setHours(0, 0, 0, 0);
        expectedEnd.setHours(23, 59, 59, 999);
        expect(mockGetBookings).toHaveBeenCalledWith(site.id, expectedStart, expectedEnd);
    });
    
    it("shows appropriate message when no bookings are retrieved", async () => {
        const mockGetBookings = jest.fn().mockResolvedValue([]);
        wrappedRender(<DailyBookings siteConfig={siteConfig} getBookings={mockGetBookings} setBookingStatus={jest.fn()}/>);
        await expectText("No bookings on this day");
    });
    
    it("shows error state when request for bookings fails", async () => {
        const mockGetBookings = jest.fn().mockRejectedValue(new Error("test server error"));
        wrappedRender(<DailyBookings siteConfig={siteConfig} getBookings={mockGetBookings} setBookingStatus={jest.fn()}/>);
        await expectText("Unable to get bookings");
    });
    
    it("filters by name and booking reference correctly", async () => {
        const mockGetBookings = jest.fn().mockResolvedValue([testBooking]);
        wrappedRender(<DailyBookings siteConfig={siteConfig} getBookings={mockGetBookings} setBookingStatus={jest.fn()}/>);
        const searchInput = await screen.findByRole("textbox", { name: "Filter bookings list" });
        await userEvent.type(searchInput, "XYZ");
        expect(screen.queryByText(testBooking.reference)).not.toBeInTheDocument();
        expect(await screen.findByText("No results found")).toBeVisible();
        await userEvent.clear(searchInput);
        await userEvent.type(searchInput, testBooking.reference);
        await expectText(testBooking.reference);
    });

    it("starts by displays today's date", async () => {
        const date = new Date();
        const expectedDate = dayjs(date).format("DD/MM/YYYY - dddd")
        const mockGetBookings = jest.fn().mockResolvedValue([testBooking]);
        const mockSetBookingStatus = jest.fn().mockResolvedValue("fakeResponse");
        wrappedRender(<DailyBookings siteConfig={siteConfig} getBookings={mockGetBookings} setBookingStatus={mockSetBookingStatus} />);
        expect(screen.getByText(expectedDate)).toBeVisible();
    })

    it("allows the user to move to the next day", async () => {
        const date = new Date();
        const nextDay = dayjs(date).add(1, "day");
        const expectedDate = dayjs(nextDay).format("DD/MM/YYYY - dddd")
        const mockGetBookings = jest.fn().mockResolvedValue([testBooking]);
        const mockSetBookingStatus = jest.fn().mockResolvedValue("fakeResponse");
        wrappedRender(<DailyBookings siteConfig={siteConfig} getBookings={mockGetBookings} setBookingStatus={mockSetBookingStatus} />);
        const nextDayLink = await screen.findByRole("link", {name: "next day"})
        await userEvent.click(nextDayLink);
        expect(await screen.findByText(expectedDate)).toBeVisible();
    })

    it("allows the user to move to the previous day", async () => {
        const date = new Date();
        const nextDay = dayjs(date).subtract(1, "day");
        const expectedDate = dayjs(nextDay).format("DD/MM/YYYY - dddd")
        const mockGetBookings = jest.fn().mockResolvedValue([testBooking]);
        const mockSetBookingStatus = jest.fn().mockResolvedValue("fakeResponse");
        wrappedRender(<DailyBookings siteConfig={siteConfig} getBookings={mockGetBookings} setBookingStatus={mockSetBookingStatus} />);
        const previousDayLink = await screen.findByRole("link", {name: "previous day"})
        await userEvent.click(previousDayLink);
        expect(await screen.findByText(expectedDate)).toBeVisible();
    })

    it("allows the user to return to the current day", async () => {
        const date = new Date();        
        const expectedDate = dayjs(date).format("DD/MM/YYYY - dddd")
        const mockGetBookings = jest.fn().mockResolvedValue([testBooking]);
        const mockSetBookingStatus = jest.fn().mockResolvedValue("fakeResponse");
        wrappedRender(<DailyBookings siteConfig={siteConfig} getBookings={mockGetBookings} setBookingStatus={mockSetBookingStatus} />);
        const previousDayLink = await screen.findByRole("link", {name: "previous day"})
        await userEvent.click(previousDayLink);
        const todayLink = await screen.findByRole("link", {name: "today"})
        await userEvent.click(todayLink);
        expect(await screen.findByText(expectedDate)).toBeVisible();
    })

    it("hides the check in box when viewing a day other than today", async () => {
        const mockGetBookings = jest.fn().mockResolvedValue([testBooking]);
        const mockSetBookingStatus = jest.fn().mockResolvedValue("fakeResponse");
        wrappedRender(<DailyBookings siteConfig={siteConfig} getBookings={mockGetBookings} setBookingStatus={mockSetBookingStatus} />);
        const nextDayLink = await screen.findByRole("link", {name: "next day"})
        await userEvent.click(nextDayLink);
        expect(screen.queryByRole("checkbox")).toBeNull();
    })

    it("changes the status of the booking when Check In box is checked", async ()=> {
        const mockGetBookings = jest.fn().mockResolvedValue([testBooking]);
        const mockSetBookingStatus = jest.fn().mockResolvedValue("fakeResponse");
        wrappedRender(<DailyBookings siteConfig={siteConfig} getBookings={mockGetBookings} setBookingStatus={mockSetBookingStatus} />);
        const firstCheckBox = await screen.findByRole("checkbox");
        await act(async () => {
            await userEvent.click(firstCheckBox);
        });
        expect(mockSetBookingStatus).toHaveBeenCalledWith(site.id, testBooking.reference, "CheckedIn" );
        expect(firstCheckBox).toBeChecked();
    });
})
