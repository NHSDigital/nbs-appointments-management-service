import { Link, NavLink } from "react-router-dom"
import { When } from "./When";

interface NhsHeaderProps {
    navLinks: { name: string, route: string }[]
    userEmail?: string
    signOut?: () => void;
}


export const NhsHeader = ({ navLinks, userEmail, signOut }: NhsHeaderProps) => {
    return <>
        <header className="nhsuk-header nhsuk-header__transactional" role="banner">
            <div className="nhsuk-header__container">
            <div className="nhsuk-header__logo nhsuk-header__transactional--logo">
                <Link className="nhsuk-header__link" to="/" aria-label="NHS Appointment Book Home">
                    <svg className="nhsuk-logo" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 40 16" height="40" width="100">
                        <path className="nhsuk-logo__background" fill="#005eb8" d="M0 0h40v16H0z"></path>
                        <path className="nhsuk-logo__text" fill="#fff" d="M3.9 1.5h4.4l2.6 9h.1l1.8-9h3.3l-2.8 13H9l-2.7-9h-.1l-1.8 9H1.1M17.3 1.5h3.6l-1 4.9h4L25 1.5h3.5l-2.7 13h-3.5l1.1-5.6h-4.1l-1.2 5.6h-3.4M37.7 4.4c-.7-.3-1.6-.6-2.9-.6-1.4 0-2.5.2-2.5 1.3 0 1.8 5.1 1.2 5.1 5.1 0 3.6-3.3 4.5-6.4 4.5-1.3 0-2.9-.3-4-.7l.8-2.7c.7.4 2.1.7 3.2.7s2.8-.2 2.8-1.5c0-2.1-5.1-1.3-5.1-5 0-3.4 2.9-4.4 5.8-4.4 1.6 0 3.1.2 4 .6"></path>
                    </svg>
                </Link>
                </div>
                <div className="nhsuk-header__transactional-service-name">
                    <Link className="nhsuk-header__transactional-service-name--link" to="/">Appointment Book</Link>
                </div>
                <When condition={userEmail !== undefined}>
                    <div className="header__content" id="content-header">
                        <div className="header__user-control" role="navigation" aria-label="Primary navigation">
                            <ul className="header__user-control-list">
                                <li className="header__user-control-item">
                                    {userEmail}
                                </li>
                                <li className="header__user-control-item">
                                    <button
                                        className="header__user-control-link"
                                        type="button"
                                        onClick={signOut}>
                                        Log out
                                    </button>
                                </li>
                            </ul>
                        </div>
                    </div>
                </When>
            </div>
            <div className="nhsuk-navigation-container">
                <nav className="nhsuk-navigation" id="header-navigation" role="navigation" aria-label="Primary navigation">
                    <ul className="nhsuk-header__navigation-list">
                        {
                            navLinks.map(link =>
                                <li className="nhsuk-header__navigation-item" key={link.name}>
                                    <NavLink to={link.route} className="nhsuk-header__navigation-link">
                                        {link.name}
                                    </NavLink>
                                </li>
                            )
                        }
                    </ul>
                </nav>
            </div>
        </header>
    </>
}