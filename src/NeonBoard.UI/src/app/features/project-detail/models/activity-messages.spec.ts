import { getActivityMessage } from './activity-messages';
import { ActivityEntry } from './activity.model';

function makeEntry(overrides: Partial<ActivityEntry>): ActivityEntry {
  return {
    id: '1', boardId: 'b1', userId: 'u1', userName: 'Alice',
    entityType: 'Card', entityId: 'e1',
    actionType: 'Created', data: {}, occurredAt: '2026-01-01T00:00:00Z',
    ...overrides,
  };
}

describe('getActivityMessage', () => {
  it('should render card created message', () => {
    const msg = getActivityMessage(makeEntry({
      actionType: 'Created',
      entityType: 'Card',
      data: { cardTitle: 'Fix bug', cardNumber: 3, columnName: 'To Do', prefix: 'SPR' },
    }));
    expect(msg.text).toContain('SPR-3');
    expect(msg.text).toContain('Fix bug');
    expect(msg.text).toContain('To Do');
    expect(msg.icon).toBe('plus');
  });

  it('should render card moved message', () => {
    const msg = getActivityMessage(makeEntry({
      actionType: 'Moved',
      data: { cardTitle: 'Fix bug', cardNumber: 3, sourceColumn: 'To Do', targetColumn: 'Done', prefix: 'SPR' },
    }));
    expect(msg.text).toContain('To Do');
    expect(msg.text).toContain('Done');
    expect(msg.icon).toBe('arrows-left-right');
  });

  it('should render board renamed message', () => {
    const msg = getActivityMessage(makeEntry({
      actionType: 'Renamed',
      entityType: 'Board',
      data: { oldName: 'Sprint 1', newName: 'Sprint 2' },
    }));
    expect(msg.text).toContain('Sprint 1');
    expect(msg.text).toContain('Sprint 2');
  });

  it('should render column added message', () => {
    const msg = getActivityMessage(makeEntry({
      actionType: 'Created',
      entityType: 'Column',
      data: { columnName: 'In Review' },
    }));
    expect(msg.text).toContain('In Review');
  });

  it('should render label created message with label info', () => {
    const msg = getActivityMessage(makeEntry({
      actionType: 'Created',
      entityType: 'Label',
      data: { labelName: 'Bug', labelColor: 'red' },
    }));
    expect(msg.text).toContain('Bug');
    expect(msg.icon).toBe('tag');
    expect(msg.labelName).toBe('Bug');
    expect(msg.labelColor).toBe('red');
  });

  it('should render card archived message', () => {
    const msg = getActivityMessage(makeEntry({
      actionType: 'Archived',
      data: { cardTitle: 'Fix bug', cardNumber: 3, prefix: 'SPR' },
    }));
    expect(msg.text).toContain('archived');
    expect(msg.icon).toBe('box-archive');
  });

  it('should render label added to card message with label info', () => {
    const msg = getActivityMessage(makeEntry({
      actionType: 'LabelAdded',
      data: { cardTitle: 'Fix bug', cardNumber: 3, labelName: 'Bug', labelColor: 'red', prefix: 'SPR' },
    }));
    expect(msg.text).toContain('Bug');
    expect(msg.text).toContain('SPR-3');
    expect(msg.labelName).toBe('Bug');
    expect(msg.labelColor).toBe('red');
  });

  it('should render prefix updated message', () => {
    const msg = getActivityMessage(makeEntry({
      actionType: 'PrefixUpdated',
      entityType: 'Board',
      data: { oldPrefix: 'SPR', newPrefix: 'PRJ' },
    }));
    expect(msg.text).toContain('SPR');
    expect(msg.text).toContain('PRJ');
  });

  it('should handle unknown action type', () => {
    const msg = getActivityMessage(makeEntry({ actionType: 'Unknown' }));
    expect(msg.text).toBe('Unknown activity');
  });

  it('should include cardEntityId and cardDisplayId for card-type entries', () => {
    const msg = getActivityMessage(makeEntry({
      actionType: 'Moved',
      entityType: 'Card',
      entityId: 'card-42',
      data: { cardTitle: 'Fix bug', cardNumber: 3, sourceColumn: 'To Do', targetColumn: 'Done', prefix: 'SPR' },
    }));
    expect(msg.cardEntityId).toBe('card-42');
    expect(msg.cardDisplayId).toBe('SPR-3');
  });

  it('should not include cardEntityId for non-card entries', () => {
    const msg = getActivityMessage(makeEntry({
      actionType: 'Created',
      entityType: 'Column',
      data: { columnName: 'Backlog' },
    }));
    expect(msg.cardEntityId).toBeUndefined();
    expect(msg.cardDisplayId).toBeUndefined();
  });

  it('should not include cardEntityId for deleted cards', () => {
    const msg = getActivityMessage(makeEntry({
      actionType: 'Deleted',
      entityType: 'Card',
      entityId: 'card-99',
      data: { cardTitle: 'Old card', cardNumber: 5, prefix: 'TST' },
    }));
    expect(msg.cardEntityId).toBeUndefined();
  });

  it('should include labelName without color for deleted labels', () => {
    const msg = getActivityMessage(makeEntry({
      actionType: 'Deleted',
      entityType: 'Label',
      data: { labelName: 'Urgent' },
    }));
    expect(msg.labelName).toBe('Urgent');
    expect(msg.labelColor).toBeUndefined();
  });

  it('should not include label info for non-label entries', () => {
    const msg = getActivityMessage(makeEntry({
      actionType: 'Created',
      entityType: 'Column',
      data: { columnName: 'Backlog' },
    }));
    expect(msg.labelName).toBeUndefined();
  });

  it('should include user name in message text', () => {
    const msg = getActivityMessage(makeEntry({
      actionType: 'Created',
      entityType: 'Card',
      userName: 'Bob',
      data: { cardTitle: 'Fix bug', cardNumber: 3, columnName: 'To Do', prefix: 'SPR' },
    }));
    expect(msg.text).toContain('**Bob**');
  });

  it('should include cardEntityId for LabelAdded entries', () => {
    const msg = getActivityMessage(makeEntry({
      actionType: 'LabelAdded',
      entityType: 'Card',
      entityId: 'card-7',
      data: { cardTitle: 'Fix bug', cardNumber: 3, labelName: 'Bug', labelColor: '#ef4444', prefix: 'SPR' },
    }));
    expect(msg.cardEntityId).toBe('card-7');
    expect(msg.cardDisplayId).toBe('SPR-3');
  });
});
