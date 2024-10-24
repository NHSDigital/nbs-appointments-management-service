import Link from 'next/link';
import LeftChevron from './icons/left-chevron';

type Props = NavigationByHrefProps | NavigationByOnClickHandlerProps;

type NavigationByHrefProps = {
  renderingStrategy: 'server';
  href: string;
};

type NavigationByOnClickHandlerProps = {
  renderingStrategy: 'client';
  onClick: () => void;
};

/**
 * A Go Back component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/back-link
 */
const BackLink = (props: Props) => {
  if (props.renderingStrategy === 'server') {
    return (
      <div className="nhsuk-back-link">
        <Link role="link" className="nhsuk-back-link__link" href={props.href}>
          <LeftChevron />
          Go back
        </Link>
      </div>
    );
  }

  return (
    <div className="nhsuk-back-link">
      <Link
        role="link"
        className="nhsuk-back-link__link"
        href={''}
        onClick={props.onClick}
      >
        <LeftChevron />
        Go back
      </Link>
    </div>
  );
};

export default BackLink;
