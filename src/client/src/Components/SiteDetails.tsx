import { Site } from "../Types";

export const SiteDetails = ({ site }: { site: Site }) => {
  return (
    <>
      <h3 className="nhsuk-heading-m">Your Site Details</h3>
      <dl className="nhsuk-summary-list">
        <div className="nhsuk-summary-list__row">
          <dt className="nhsuk-summary-list__key">Name</dt>
          <dd className="nhsuk-summary-list__value nhsuk-u-text-align-right">{site.name}</dd>
        </div>

        <div className="nhsuk-summary-list__row">
          <dt className="nhsuk-summary-list__key">Address</dt>
          <dd className="nhsuk-summary-list__value nhsuk-u-text-align-right">{site.address}</dd>
        </div>
      </dl>
    </>
  );
};
