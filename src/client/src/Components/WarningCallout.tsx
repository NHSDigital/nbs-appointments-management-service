
export const WarningCallout = ({title, children}:{title:string, children:React.ReactNode}) => {
      return(
            <div className="nhsuk-warning-callout">
                  <h3 className="nhsuk-warning-callout__label">
                        <span role="note">
                              <span className="nhsuk-u-visually-hidden">Important: </span>
                              {title}
                        </span>
                  </h3>
                  {children}
            </div>
      );
}