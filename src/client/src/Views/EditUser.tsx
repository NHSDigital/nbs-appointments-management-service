import React from "react";
import Link from "next/link";
import {useUserService} from "../Services/UserService";
import {useRolesService} from "../Services/RolesService";
import {useSiteContext} from "../ContextProviders/SiteContextProvider";
import {Role} from "../Types/Permissions";
import {Site} from "../Types/Site";

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
    const [status, setStatus] = React.useState<"loading" | "loaded">("loading");
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
    
    const saveUser = () => {
        setUserRoles(site!.id, user, selectedRoles).then(r => r);
    }
    
    const clearUser = () => {
        setUser("");
        setSelectedRoles([] as string[])
    }
    
    return (
        <div>
            <div>
                <h3>Create new user</h3>
                <div>Set the details and roles of a new user</div>
                <label className="nhsuk-label">
                    Email
                </label>
                <input 
                    type="text" 
                    className="nhsuk-input nhsuk-input--width-20" 
                    value={user} 
                    onChange={e => setUser(e.target.value)}/>
                <h3>
                    Roles
                </h3>
                <div>
                    {roles.map(r => (
                        <div className="nhsuk-checkboxes__item">
                            <input 
                                type="checkbox"
                                className="nhsuk-checkboxes__input"
                                id={r.id}
                                checked={selectedRoles?.includes(r.id)}
                                onChange={() => handleOnChange(r.id)}
                            />
                            <label className="nhsuk-label nhsuk-checkboxes__label">{r.displayName}
                            </label>
                        </div>
                    ))}
                </div>
                <div className="nhsuk-navigation">
                    <button
                        id="submit-schedule"
                        type="button"
                        className="nhsuk-button nhsuk-u-margin-bottom-0"
                        onClick={saveUser}
                    >
                        Confirm and create user
                    </button>
                    <button
                        type="button"
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