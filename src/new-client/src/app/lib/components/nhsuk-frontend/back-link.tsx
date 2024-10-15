import Link from 'next/link';
import LeftChevron from './icons/left-chevron';

type Props = {
  href: string;
};

/**
 * A Go Back component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/back-link
 */
const BackLink = ({ href }: Props) => {
  return (
    <div className="nhsuk-back-link">
      <Link className="nhsuk-back-link__link" href={href}>
        <LeftChevron />
        Go back
      </Link>
    </div>
  );
};

export default BackLink;
