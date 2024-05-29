import { screen } from "@testing-library/react";
import { wrappedRender } from "../utils";
import { Session } from "../Types/Schedule";
import { TimeBlocks } from "./TimeBlocks";

const testValidation = async (block: Session[], expectedMessage: string) => {
    wrappedRender(<TimeBlocks dayLabel="Monday" scheduleBlocks={block} setDayBlocks={jest.fn()} setIsValid={jest.fn()} copyToAllDays={jest.fn()} />)
    const validationError = await screen.findByText(expectedMessage);
    expect(validationError).toBeVisible();
    const button = await screen.findByText("Add time period");
    expect(button).toBeDisabled();
}

describe("<TimeBlocks />", () => {
    it("validates and prevents the submission of invalid values", async () => {
        await testValidation([{from: "invalid-value", until: "invalid-value", services: [" "]}], "Please enter a valid start time");
    });

    it("validates and prevents adding a session < 1 hour long", async () => {
        await testValidation([{from: "16:01", until: "17:00", services: [" "]}], "Time periods must be a minimum of 60 minutes");
    });

    it("validates and prevents the user setting an end time before a start time", async () => {
        await testValidation([{from: "17:01", until: "17:00", services: [" "]}], "End time must be after start time");
    });

    it("validates and prevents the user adding overlapping sessions", async ()=> {
        const overlappedBlocks = [{from: "09:00", until: "12:00", services: [" "]}, {from: "11:00", until: "14:00", services:[" "] }]
        await testValidation(overlappedBlocks, "Time periods cannot overlap");
    })

    it("prevents the user adding more than 24 hours of availability per day", async () => {
        const block: Session[] = [{from: "00:00", until: "00:00", services: [" "]}];
        wrappedRender(<TimeBlocks dayLabel="Monday" scheduleBlocks={block} setDayBlocks={jest.fn()} setIsValid={jest.fn()} copyToAllDays={jest.fn()} />);
        const button = await screen.findByText("Add time period");
        expect(button).toBeDisabled();
    });

    it("validates and prevents the user submitting a block with no services allocated", async () => {
        await testValidation([{from: "09:00", until: "17:00", services: []}], "At least one service is required");

    })
})
