import { ReactNode } from 'react';

const NhsMainContainer = ({ children }: { children: ReactNode }) => {
  return (
    <div className="nhsuk-width-container">
      <main className="nhsuk-main-wrapper" id="main-content" role="main">
        <div className="nhsuk-grid-row">
          <div className="nhsuk-grid-column-full">{children}</div>
        </div>
      </main>
    </div>
  );
};

export default NhsMainContainer;
