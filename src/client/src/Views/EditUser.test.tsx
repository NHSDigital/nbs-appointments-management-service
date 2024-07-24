import {screen} from '@testing-library/react';
import userEvent from "@testing-library/user-event";
import {wrappedRender} from '../utils';
import {EditUser} from "./EditUser";
import {Role} from "../Types/Role";
import {Site} from "../Types/Site";

const site: Site = { name: "Test Site", id: "1", address: "123" };
const testRoles:  Role[] = [
    {
        displayName: "Test Role One",
        id: "test-role-one",
        description: "Description for test role one"
    },
    {
        displayName: "Test Role Two",
        id: "test-role-two",
        description: "Description for test role two"
    },
]

describe("<EditUser />", () => {
    it("shows available roles to user", async () => {
        const mockGetRoles = jest.fn().mockResolvedValue(testRoles);
        wrappedRender(<EditUser setUserRoles={jest.fn()} getRoles={mockGetRoles} site={site} navigate={jest.fn}/>);
        expect(mockGetRoles).toHaveBeenCalled();
        expect(await screen.findByText("Test Role One")).toBeVisible();
        expect(await screen.findByText("Description for test role one")).toBeVisible();
        expect(await screen.findByText("Test Role Two")).toBeVisible();
        expect(await screen.findByText("Description for test role two")).toBeVisible();
        expect(screen.queryByText("Loading roles...")).toBeNull();
    });
    
    it("displays validation message when submitting an invalid email address", async () => {
        const mockGetRoles = jest.fn().mockResolvedValue(testRoles);
        const mockSaveUser = jest.fn().mockResolvedValue(null);
        wrappedRender(<EditUser setUserRoles={mockSaveUser} getRoles={mockGetRoles} site={site} navigate={jest.fn}/>);
        
        const emailInput = screen.getByRole("textbox");
        await userEvent.type(emailInput, "invalid-email-address");
        
        const roleOneCheckbox = await screen.findByRole("checkbox", {name: "test-role-one"});
        await userEvent.click(roleOneCheckbox);
        
        const submitButton = screen.getByRole("button", {name: "save user"});
        await userEvent.click(submitButton)
        
        expect(await screen.findByText("You have not entered a valid nhs email address")).toBeVisible();
        expect(screen.queryByText("You have not selected any roles for this user")).toBeNull();
        expect(mockSaveUser).not.toHaveBeenCalled();
    });

    it("displays validation message when submitting with no roles selected", async () => {
        const mockGetRoles = jest.fn().mockResolvedValue(testRoles);
        const mockSaveUser = jest.fn().mockResolvedValue(null);
        wrappedRender(<EditUser setUserRoles={mockSaveUser} getRoles={mockGetRoles} site={site} navigate={jest.fn}/>);
        
        const emailInput = screen.getByRole("textbox");
        await userEvent.type(emailInput, "valid-email-address@test.com");
        
        const submitButton = screen.getByRole("button", {name: "save user"});
        await userEvent.click(submitButton)
        
        expect(await screen.findByText("You have not selected any roles for this user")).toBeVisible();
        expect(screen.queryByText("You have not entered a valid nhs email address")).toBeNull();
        expect(mockSaveUser).not.toHaveBeenCalled();
    });
    
    it("clears fields when user cancels", async () => {
        const mockGetRoles = jest.fn().mockResolvedValue(testRoles);
        wrappedRender(<EditUser setUserRoles={jest.fn()} getRoles={mockGetRoles} site={site} navigate={jest.fn}/>);

        const emailInput = screen.getByRole("textbox");
        await userEvent.type(emailInput, "test@test.com");
        
        const roleOneCheckbox = await screen.findByRole("checkbox", {name: "test-role-one"});
        await userEvent.click(roleOneCheckbox);
        
        const cancelButton = screen.getByRole("button", {name: "cancel"});
        await userEvent.click(cancelButton)

        expect(emailInput).toHaveValue("");
        expect(roleOneCheckbox).not.toBeChecked();
    });

    it("calls saveUser with correct user information", async () => {
        const mockGetRoles = jest.fn().mockResolvedValue(testRoles);
        const mockSaveUser = jest.fn().mockResolvedValue("fakeResult");
        wrappedRender(<EditUser setUserRoles={mockSaveUser} getRoles={mockGetRoles} site={site} navigate={jest.fn}/>);
        
        const emailInput = screen.getByRole("textbox");
        await userEvent.type(emailInput, "test@test.com");

        const roleTwoCheckbox = await screen.findByRole("checkbox", {name: "test-role-two"});
        await userEvent.click(roleTwoCheckbox);
        
        const submitButton =  screen.getByRole("button", {name: "save user"});
        await userEvent.click(submitButton);
        
        expect(mockSaveUser).toHaveBeenCalledWith(site.id, "test@test.com", ["test-role-two"] );
    });
})
