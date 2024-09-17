import { calculateAvailabilityInBlock } from '@services/availabilityService';
import { AvailabilityBlock } from '@types';

type Props = {
  block: AvailabilityBlock;
  removeBlock: (block: AvailabilityBlock) => void;
  cancel: () => void;
};

const ConfirmRemoveBlock = ({ block, removeBlock, cancel }: Props) => {
  return (
    <div className="nhsuk-warning-callout-custom" id="confirm-remove-block">
      <form>
        <div style={{ display: 'flex', flexDirection: 'column' }}>
          <div className="nhsuk-warning-callout-custom__container">
            {block.isBreak ? (
              <span>
                Are you sure you want to delete break session{' '}
                <b>
                  {block.start} - {block.end}
                </b>
                ?
              </span>
            ) : (
              <span>
                Are you sure you want to delete session{' '}
                <b>
                  {block.start} - {block.end}
                </b>{' '}
                containing availability for{' '}
                <b>{calculateAvailabilityInBlock(block)}</b> appointments?
              </span>
            )}
            <button
              type="button"
              className="nhsuk-warning-callout-custom__close-button"
              style={{
                fontWeight: 'bold',
                width: 'fit-content',
                paddingLeft: 0,
                marginTop: 0,
                alignSelf: 'flex-start',
              }}
              onClick={() => {
                cancel();
              }}
            >
              Cancel
            </button>
          </div>

          <button
            type="button"
            className="nhsuk-warning-callout-custom__close-button"
            style={{
              fontWeight: 'bold',
              width: 'fit-content',
              paddingLeft: 0,
              marginTop: 10,
            }}
            onClick={() => {
              removeBlock(block);
            }}
          >
            Confirm Delete
          </button>
        </div>
      </form>
    </div>
  );
};

export default ConfirmRemoveBlock;
