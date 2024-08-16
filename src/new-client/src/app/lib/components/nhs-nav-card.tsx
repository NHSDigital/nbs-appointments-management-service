import Link from 'next/link';
import NhsIcon from '@components/nhs-icon';

type Props = {
  href: string;
  title: string;
  description: string;
};

const NhsNavCard = ({ href, title, description }: Props) => {
  return (
    <div className="nhsuk-card nhsuk-card--clickable">
      <div className="nhsuk-card__content nhsuk-card__content--primary">
        <h2 className="nhsuk-card__heading nhsuk-heading-m">
          <Link className="nhsuk-card__link" href={href}>
            {title}
          </Link>
        </h2>
        <p className="nhsuk-card__description">{description}</p>
        <NhsIcon />
      </div>
    </div>
  );
};

export default NhsNavCard;
