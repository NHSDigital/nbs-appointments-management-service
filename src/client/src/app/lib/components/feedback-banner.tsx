import Tag from '@components/nhsuk-frontend/tag';

type Props = {
  originPage: string;
};

const FeedbackBanner = ({ originPage }: Props) => {
  return (
    <div className="feedback-banner">
      <div className="feedback-banner__container">
        <Tag text="Feedback" />{' '}
        <a
          href={`https://feedback.digital.nhs.uk/jfe/form/SV_0I2qLDukSOJtvjU?origin=${originPage}`}
          target="_blank"
          rel="noopener noreferrer"
        >
          Give page or site feedback (opens in a new tab)
        </a>{' '}
        <span>
          - this is a new service, your feedback will help us improve it.
        </span>
      </div>
    </div>
  );
};

export default FeedbackBanner;
