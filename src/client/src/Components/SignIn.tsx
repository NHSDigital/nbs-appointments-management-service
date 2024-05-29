import { NhsFooter } from "./NhsFooter"
import { NhsHeader } from "./NhsHeader"

type SignInProps = {
    signIn: () => void
}

export const SignIn = ({signIn}:SignInProps) => {
    return(
        <>
            <NhsHeader navLinks={[]} />
            <div className="nhsuk-width-container">
                <main className="nhsuk-main-wrapper " id="maincontent" role="main">
                    <div className="nhsuk-grid-row">
                        <div className="nhsuk-grid-column-full">
        
                            <h2>NHS Appointments Book</h2>
                            <div><button className="nhsuk-button" onClick={signIn}>Sign In</button></div>
                        </div>
                    </div>
                </main>
            </div>
            <NhsFooter/>
        </>
    )
}