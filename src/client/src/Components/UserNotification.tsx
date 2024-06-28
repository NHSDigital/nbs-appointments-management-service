import "./UserNotification.css"

export const UserNotification = ({title, handleClose}:{title:string, handleClose:()=>void}) => {
    return(
          <div role="status" className="user-notification">
            <div className="user-notification__content">
                  <span className="nhsuk-u-visually-hidden">Information: </span>
                  <div className="user-notification__text">
                        {title}
                  </div>
            </div>
            <button className="user-notification__close" onClick={handleClose}>close</button>
          </div>
    );
}