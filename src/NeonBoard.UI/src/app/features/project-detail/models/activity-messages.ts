import { ActivityEntry } from './activity.model';

export interface ActivityMessage {
  icon: string;
  text: string;
  cardEntityId?: string;
  cardDisplayId?: string;
  labelName?: string;
  labelColor?: string;
}

export function getActivityMessage(entry: ActivityEntry): ActivityMessage {
  const d = entry.data;
  const user = `**${entry.userName}**`;
  const displayId = d['prefix'] && d['cardNumber'] ? `${d['prefix']}-${d['cardNumber']}` : '';
  const cardLink = entry.entityType === 'Card' ? { cardEntityId: entry.entityId, cardDisplayId: displayId } : {};
  const labelInfo = d['labelName'] ? { labelName: String(d['labelName']), labelColor: d['labelColor'] ? String(d['labelColor']) : undefined } : {};

  switch (entry.actionType) {
    case 'Created':
      if (entry.entityType === 'Card')
        return { icon: 'plus', text: `${user} added **${displayId}** "${d['cardTitle']}" to **${d['columnName']}**`, ...cardLink };
      if (entry.entityType === 'Column')
        return { icon: 'plus', text: `${user} added column **${d['columnName']}**` };
      if (entry.entityType === 'Label')
        return { icon: 'tag', text: `${user} created label **${d['labelName']}**`, ...labelInfo };
      return { icon: 'plus', text: `${user} created board **${d['boardName']}**` };

    case 'Updated':
      if (entry.entityType === 'Card')
        return { icon: 'pencil', text: `${user} updated **${displayId}** "${d['cardTitle']}"`, ...cardLink };
      if (entry.entityType === 'Label')
        return { icon: 'tag', text: `${user} updated label **${d['labelName']}**`, ...labelInfo };
      return { icon: 'pencil', text: `${user} updated` };

    case 'Moved':
      return { icon: 'arrows-left-right', text: `${user} moved **${displayId}** "${d['cardTitle']}" from **${d['sourceColumn']}** to **${d['targetColumn']}**`, ...cardLink };

    case 'Deleted':
      if (entry.entityType === 'Card')
        return { icon: 'trash', text: `${user} deleted **${displayId}** "${d['cardTitle']}"` };
      if (entry.entityType === 'Column')
        return { icon: 'trash', text: `${user} deleted column **${d['columnName']}**` };
      if (entry.entityType === 'Label')
        return { icon: 'trash', text: `${user} deleted label **${d['labelName']}**`, ...labelInfo };
      return { icon: 'trash', text: `${user} deleted board **${d['boardName']}**` };

    case 'Archived':
      return { icon: 'box-archive', text: `${user} archived **${displayId}** "${d['cardTitle']}"`, ...cardLink };

    case 'Restored':
      return { icon: 'rotate-left', text: `${user} restored **${displayId}** "${d['cardTitle']}"`, ...cardLink };

    case 'Held':
      return { icon: 'pause', text: `${user} put **${displayId}** "${d['cardTitle']}" on hold`, ...cardLink };

    case 'Resumed':
      return { icon: 'play', text: `${user} resumed **${displayId}** "${d['cardTitle']}"`, ...cardLink };

    case 'Renamed':
      return { icon: 'pencil', text: `${user} renamed board from **${d['oldName']}** to **${d['newName']}**` };

    case 'Reordered':
      return { icon: 'arrows-up-down', text: `${user} reordered columns` };

    case 'LabelAdded':
      return { icon: 'tag', text: `${user} added label **${d['labelName']}** to **${displayId}**`, cardEntityId: entry.entityId, cardDisplayId: displayId, ...labelInfo };

    case 'LabelRemoved':
      return { icon: 'tag', text: `${user} removed label **${d['labelName']}** from **${displayId}**`, cardEntityId: entry.entityId, cardDisplayId: displayId, ...labelInfo };

    case 'PrefixUpdated':
      return { icon: 'pencil', text: `${user} changed board prefix from **${d['oldPrefix']}** to **${d['newPrefix']}**` };

    default:
      return { icon: 'circle-info', text: 'Unknown activity' };
  }
}
