const Spinner = () => {
  return (
    <div className="nhsuk-loader">
      <span className="nhsuk-loader__spinner" />
    </div>
  );
};

const SpinnerWithText = ({ text = 'Loading' }) => {
  return (
    <div className="nhsuk-loader">
      <span className="nhsuk-loader__spinner" />
      <span className="nhsuk-loader__text">{text}</span>
    </div>
  );
};

const SmallSpinnerWithText = ({ text = 'Loading' }) => {
  return (
    <div className={`nhsuk-loader nhsuk-loader--small`}>
      <span className="nhsuk-loader__spinner" />
      <span className="nhsuk-loader__text">{text}</span>
    </div>
  );
};

const WhiteSpinnerWithText = ({ text = 'Loading' }) => {
  return (
    <div className="nhsuk-loader nhsuk-loader--white">
      <span className="nhsuk-loader__spinner" />
      <span className="nhsuk-loader__text">{text}</span>
    </div>
  );
};

export { Spinner, SpinnerWithText, SmallSpinnerWithText, WhiteSpinnerWithText };
