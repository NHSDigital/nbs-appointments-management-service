export type NhsHeadingProps = {
  title: string;
  caption?: string;
};

const NhsHeading = ({ title, caption }: NhsHeadingProps) => {
  return (
    <h1 className="nhsuk-heading-l">
      {caption && <span className="nhsuk-caption-l">{caption}</span>}
      {title}
    </h1>
  );
};

export default NhsHeading;
