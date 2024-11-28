type PageProps = {
  searchParams: {
    from: string;
    until: string;
  };
};

export const ViewWeekAvailabilityPage = ({ searchParams }: PageProps) => {
  return <p>This is the view week availability page.</p>;
};
