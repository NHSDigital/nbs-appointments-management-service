import styles from "./UserNotification.module.css"

type UserNotificationProps =  {
      title: string;
      handleClose: () => void;
}

export const UserNotification = ({title, handleClose}: UserNotificationProps) => {
    return(
          <div role="status" className={styles["user-notification"]}>
            <div className={styles["user-notification__content"]}>
                  <span className="nhsuk-u-visually-hidden">Information: </span>
                  <div className={styles["user-notification__text"]}>
                        {title}
                  </div>
            </div>
            <button className={styles["user-notification__close"]} onClick={handleClose}>close</button>
          </div>
    );
}