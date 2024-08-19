const NhsPageTitle = ({ title }: { title: string }) => {
  return (
    <div className="nhsuk-grid-row">
      <h1 className="nhsuk-grid-column-two-thirds nhsuk-heading-l">{title}</h1>
    </div>
  );
};

export default NhsPageTitle;
