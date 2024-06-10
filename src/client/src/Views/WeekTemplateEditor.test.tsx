import { act, fireEvent, screen } from '@testing-library/react';
import { WeekTemplateEditor } from './WeekTemplateEditor';
import { wrappedRender } from '../utils';
import { ScheduleTemplate, WeekTemplate } from '../Types/Schedule';
import { SiteConfiguration } from '../Types/SiteConfiguration';

const site: SiteConfiguration = { siteId: "1", serviceConfiguration: [], informationForCitizen: "" };

const createTemplate = (id: string, name: string, from: string, until: string) : WeekTemplate => ({
    id,
    name,
    site: "",
    items: [{
    days: ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"],
    scheduleBlocks: [
        {
            from,
            until,
            services: ["COVID:18_74"]
        }
    ]}]
} as WeekTemplate);

describe("<WeekTemplateEditor />", () => {
        it("calls loadTemplates with template id and displays response", async () => {
            const testValues = { start: "09:00", end: "16:30"};
            const mockGetTemplates = jest.fn().mockResolvedValue([createTemplate("1", "Test Template", testValues.start, testValues.end)]);
            wrappedRender(<WeekTemplateEditor templateId='1' siteId='1' getTemplates={mockGetTemplates} saveTemplate={jest.fn()}  />);
            const startInput = await screen.findAllByDisplayValue(testValues.start);
            const endInputs =  await screen.findAllByDisplayValue(testValues.end);
            expect(mockGetTemplates).toHaveBeenCalled();
            expect(startInput[0]).toBeVisible()
            expect(endInputs[0]).toBeVisible();
        });

        it("sets the schedule correctly after changing a value", async () => {
            const selectableValue = "09:00";
            const mockGetTemplates = jest.fn().mockResolvedValue([createTemplate("1", "Test Template", selectableValue, "17:00")]);
            const mockSaveTemplate = jest.fn().mockResolvedValue(null);
            const newValue = "09:15";
            wrappedRender(<WeekTemplateEditor templateId='1' siteId='1' getTemplates={mockGetTemplates} saveTemplate={mockSaveTemplate} />);
            const startInputs = await screen.findAllByDisplayValue(selectableValue);
            // first input is for Monday
            fireEvent.change(startInputs[0], {target: {value: newValue}});
            await act(async ()=> {
                fireEvent.click(screen.getByText("Confirm template"));
            });
            const expectedScheduleItems : ScheduleTemplate[] = [
                {
                    days: ["Monday"],
                    scheduleBlocks: [
                        { from: newValue, until: "17:00", services: ["COVID:18_74"] }
                    ]
                },
                {
                    days: [ "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"],
                    scheduleBlocks: [
                        { from: selectableValue, until: "17:00", services: ["COVID:18_74"] }
                    ]
                }
            ]
            const expectedTemplate : WeekTemplate = {
                id: "1",
                site: "1",
                name: "Test Template",
                items: expectedScheduleItems
            }
            expect(mockSaveTemplate).toHaveBeenCalledWith(expectedTemplate);
        });

        it("disables the confirm button when loaded values haven't changed", async () => {
            const selectableValue = "09:00";
            const newValue = "09:01";
            const testValues = { start: "09:00", end: "16:30"};
            const mockGetTemplates = jest.fn().mockResolvedValue([createTemplate("1", "Test Template", testValues.start, testValues.end)]);
            wrappedRender(<WeekTemplateEditor templateId="1" siteId='1' getTemplates={mockGetTemplates} saveTemplate={jest.fn()} />);
            const startInputs = await screen.findAllByDisplayValue(selectableValue);
            const confirmButton = screen.getByText("Confirm template");
            fireEvent.change(startInputs[0], {target: {value: newValue}});
            expect(confirmButton).not.toBeDisabled();
            fireEvent.change(startInputs[0], {target: {value: selectableValue}});
            expect(confirmButton).toBeDisabled();
        })
    }
)