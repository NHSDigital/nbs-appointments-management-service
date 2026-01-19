import { Footer } from 'nhsuk-react-components';
import { HTMLAttributeAnchorTarget, ReactNode } from 'react';

type supportLink = {
  text: string;
  href: string;
  target?: HTMLAttributeAnchorTarget;
  internal: boolean;
};

type FooterProps = {
  supportLinks?: supportLink[];
  children?: ReactNode;
};

export const NewFooter = ({ supportLinks = [], children }: FooterProps) => {
  return (
    <Footer>
      <Footer.Meta>
        {supportLinks.map((link, index) => (
          <Footer.ListItem
            key={`support-link-${index}`}
            href={link.href}
            target={link.target ?? '_self'}
            rel="noopener noreferrer"
          >
            {link.text}
          </Footer.ListItem>
        ))}
        <Footer.Copyright />
        <div>{children}</div>
      </Footer.Meta>
    </Footer>
  );
};
