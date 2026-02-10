import Link from 'next/link';

export type BackLinkProps =
  | NavigationByHrefProps
  | NavigationByOnClickHandlerProps;

export type NavigationByHrefProps = {
  renderingStrategy: 'server';
  href: string;
  text: string;
};

type NavigationByOnClickHandlerProps = {
  renderingStrategy: 'client';
  onClick: () => void;
  text: string;
};

/**
 * A Go Back component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/back-link
 */
const BackLink = (props: BackLinkProps) => {
  if (props.renderingStrategy === 'server') {
    return (
      <div className="nhsuk-back-link no-print">
        <Link
          role="link"
          className="nhsuk-back-link__link"
          href={props.href}
          prefetch={false}
        >
          {props.text}
        </Link>
      </div>
    );
  }

  return (
    <div className="nhsuk-back-link no-print">
      <Link
        role="link"
        className="nhsuk-back-link__link"
        href={''}
        onClick={e => {
          e.preventDefault();
          props.onClick();
        }}
      >
        {props.text}
      </Link>
    </div>
  );
};

export default BackLink;
