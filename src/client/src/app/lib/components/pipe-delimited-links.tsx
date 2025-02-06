import Link from 'next/link';
import { ReactNode } from 'react';

export type ActionLink = {
  text: string;
  href: string;
};

type PipeDelimitedLinksProps = {
  actionLinks: ActionLink[];
};

const PipeDelimitedLinks = ({ actionLinks }: PipeDelimitedLinksProps) => {
  return (
    <>
      {actionLinks.reduce<ReactNode[]>(
        (acc, { text, href }, index) => [
          ...acc,
          index > 0 && ' | ',
          <Link key={href} className="nhsuk-link" href={href}>
            {text}
          </Link>,
        ],
        [],
      )}
    </>
  );
};

export default PipeDelimitedLinks;
