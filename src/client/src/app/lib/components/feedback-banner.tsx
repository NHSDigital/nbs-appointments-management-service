import Tag from '@components/nhsuk-frontend/tag';

const FeedbackBanner = () => {
  return (
    <div className="feedback-banner">
      <Tag text="Feedback" />{' '}
      <a
        href="https://feedback.digital.nhs.uk/jfe/form/SV_5AROlphcOb5wfEq"
        target="_blank"
        rel="noopener noreferrer"
      >
        Give page or site feedback (opens in a new tab)
      </a>{' '}
      <span>
        - this is a new service, your feedback will help us improve it.
      </span>
    </div>
  );
};

export default FeedbackBanner;
