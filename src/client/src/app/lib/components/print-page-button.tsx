'use client';

import { Button } from '@components/nhsuk-frontend';

const PrintPageButton = () => {
  const onClick = () => {
    window.print();
  };

  return (
    <Button className="no-print" onClick={() => onClick()}>
      Print page
    </Button>
  );
};

export default PrintPageButton;
