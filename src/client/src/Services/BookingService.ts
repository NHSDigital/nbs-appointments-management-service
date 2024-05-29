import { Booking } from "../Types/Booking";
import { useAuthenticatedClient } from "./ApiClient";

export const useBookingService = () => {

    const apiClient = useAuthenticatedClient();

    const getBookings = async (site: string, from: Date, to: Date) : Promise<Booking[]> => {
        return apiClient.post("get-bookings", {site, from, to}).then(rsp => rsp.json());
    }

    const setBookingStatus = (site: string, bookingReference: string, status: string) => {
        return apiClient.post("booking/set-status", {site, bookingReference, status});
    }

    return { getBookings, setBookingStatus }
}