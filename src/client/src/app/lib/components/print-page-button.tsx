'use client';

import { Button } from '@components/nhsuk-frontend';

const PrintPageButton = () => {
  const onClick = () => {
    window.print();
  };

  return (
    <Button
      className="no-print"
      styleType="secondary"
      onClick={() => onClick()}
    >
      Print page
    </Button>
  );
};

export default PrintPageButton;
