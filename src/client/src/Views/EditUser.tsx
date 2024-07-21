import React from "react";
import {useUserService} from "../Services/UserService";
import {useRolesService} from "../Services/RolesService";
import {useSiteContext} from "../ContextProviders/SiteContextProvider";
import {Role} from "../Types/Role";
import {Site} from "../Types/Site";
import {When} from "../Components/When";
import {ValidationError} from "../Types/ValidationError";
import { useRouter } from "next/navigation";

type EditUserProps = {
    setUserRoles: (user: string, site: string, roles: string[]) => Promise<any>
    getRoles: () => Promise<any>
    site: Site
}

export const EditUserCtx = () => {
    const { setUserRoles } = useUserService();
    const { getRoles } = useRolesService();
    const { site } = useSiteContext();
    return ( <EditUser setUserRoles={setUserRoles} getRoles={getRoles} site={site!} />)
}
export const EditUser = ({setUserRoles, getRoles, site} : EditUserProps) => {
    const router = useRouter();
    const [validationErrors, setValidationErrors] = React.useState<ValidationError[]>([]);
    const [status, setStatus] = React.useState<"loading" | "loaded" >("loading");
    const [ selectedRoles, setSelectedRoles] = React.useState<string[]>([] as string[]);
    const [user, setUser] = React.useState("");
    const [roles, setRoles ] = React.useState<Role[]>([] as Role[])

    React.useEffect(() => {
        loadRoles();
    }, [])

    const loadRoles = () => {
        setStatus("loading");
        getRoles().then(rsp => {
            setRoles(rsp);
            setStatus("loaded");
        })
    }
    
    const handleOnChange = (roleId: string) => {
        selectedRoles?.includes(roleId) ?
            setSelectedRoles( selectedRoles?.filter(r => r != roleId) ) :
            setSelectedRoles( [...selectedRoles, roleId] );
    }
    
    const validateFields = (user: string, selectedRoles: string[]) => {
        const newValidationErrors: ValidationError[] = [];
        const emailRegex = /[\w-.]+@([\w-]+\.)+[\w-]{2,4}/
        if (!emailRegex.test(user)) {
            newValidationErrors.push({message: "You have not entered a valid nhs email address", field: "email"})
        }
        if (selectedRoles.length < 1) {
            newValidationErrors.push({message: "You have not selected any roles for this user", field: "roles"})
        }
        setValidationErrors(newValidationErrors);
        return newValidationErrors.length === 0;
    }
    
    const saveUser = () => {
        if (validateFields(user, selectedRoles)) {
            setUserRoles(site!.id, user, selectedRoles).then(r => {
                router.push("/")
            });
        }
    }
    
    const clearUser = () => {
        setUser("");
        setSelectedRoles([] as string[]);
        setValidationErrors([] as ValidationError[]);
    }
    
    const emailErrorMessage = React.useMemo(
        () =>  validationErrors.find(val => val.field === 'email')?.message
    ,[validationErrors])

    const roleSelectErrorMessage = React.useMemo(
        () =>  validationErrors.find(val => val.field === 'roles')?.message
        ,[validationErrors])
    
    return (
        <div className="nhsuk-grid-row">
            <div className="nhsuk-grid-column-one-half">

                
                <div className="nhsuk-form-group">
                    <h3>Create new user</h3>
                    <div className="nhsuk-hint">
                        Set the details and roles of a new user
                    </div>
                </div>
                    
                <div className={`nhsuk-form-group ${emailErrorMessage ? "nhsuk-form-group--error" : ""}`}>
                    <label className="nhsuk-label">Email</label>
                    <When condition={emailErrorMessage !== undefined}>
                        <span className="nhsuk-error-message">
                                <span className="nhsuk-u-visually-hidden">Error:</span>{" "}
                            {emailErrorMessage}
                            </span>
                    </When>
                    <input 
                        type="text"
                        className="nhsuk-input nhsuk-input--width-20" 
                        value={user}
                        onChange={e => setUser(e.target.value)}/>
                </div>

                    <div className={`nhsuk-form-group ${validationErrors?.find(x => x.field?.includes("roles")) ? "nhsuk-form-group--error" : ""}`}>
                        <h3>Roles</h3>
                        <When condition={roleSelectErrorMessage !== undefined}>
                            <span className="nhsuk-error-message" style={{marginTop: "-24px"}}>
                                    <span className="nhsuk-u-visually-hidden">Error:</span>{" "}
                                {roleSelectErrorMessage}
                                </span>
                        </When>
                        <When condition={status === "loading"}>
                            <span>Loading roles...</span>
                        </When>
                        <When condition={status === "loaded"}>
                            <div>
                                {roles.map(r => (
                                    <div key={r.id} className="nhsuk-checkboxes__item"> 
                                        <input 
                                            type="checkbox"
                                            className="nhsuk-checkboxes__input"
                                            aria-label={r.id}
                                            checked={selectedRoles?.includes(r.id)}
                                            onChange={() => handleOnChange(r.id)}
                                        />
                                        <label className="nhsuk-label nhsuk-checkboxes__label">{r.displayName}
                                        </label>
                                    </div>
                                ))}
                            </div>
                        </When>
                    </div>
                
                <div className="nhsuk-navigation">
                    <button
                        type="button"
                        aria-label="save user"
                        className="nhsuk-button nhsuk-u-margin-bottom-0"
                        disabled={status === "loading"}
                        onClick={saveUser}
                    >
                        Confirm and create user
                    </button>
                    <button
                        type="button"
                        aria-label="cancel"
                        className="nhsuk-button nhsuk-button--secondary nhsuk-u-margin-left-3 nhsuk-u-margin-bottom-0"
                        onClick={clearUser}
                    >
                        Cancel
                    </button>
                </div>
            </div>
        </div> 
    )
}