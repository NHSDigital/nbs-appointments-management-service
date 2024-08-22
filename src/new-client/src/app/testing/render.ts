import { render as baseRender, RenderResult } from '@testing-library/react';
import userEvent, { UserEvent } from '@testing-library/user-event';
import { ReactNode } from 'react';

export interface CustomRenderResult extends RenderResult {
  user: UserEvent;
}

export default function render(component: ReactNode): CustomRenderResult {
  return {
    ...(baseRender(component) as RenderResult),
    user: userEvent.setup(),
  };
}
