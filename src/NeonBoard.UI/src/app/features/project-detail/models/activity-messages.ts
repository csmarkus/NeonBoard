import { ActivityEntry } from './activity.model';

export interface ActivityMessage {
  icon: string;
  text: string;
}

export function getActivityMessage(entry: ActivityEntry): ActivityMessage {
  const d = entry.data;
  const displayId = d['prefix'] && d['cardNumber'] ? `${d['prefix']}-${d['cardNumber']}` : '';

  switch (entry.actionType) {
    case 'Created':
      if (entry.entityType === 'Card')
        return { icon: 'plus', text: `**${displayId}** "${d['cardTitle']}" added to **${d['columnName']}**` };
      if (entry.entityType === 'Column')
        return { icon: 'plus', text: `Column **${d['columnName']}** added` };
      if (entry.entityType === 'Label')
        return { icon: 'tag', text: `Label **${d['labelName']}** created` };
      return { icon: 'plus', text: `Board **${d['boardName']}** created` };

    case 'Updated':
      if (entry.entityType === 'Card')
        return { icon: 'pencil', text: `**${displayId}** "${d['cardTitle']}" updated` };
      if (entry.entityType === 'Label')
        return { icon: 'tag', text: `Label **${d['labelName']}** updated` };
      return { icon: 'pencil', text: 'Updated' };

    case 'Moved':
      return { icon: 'arrows-left-right', text: `**${displayId}** "${d['cardTitle']}" moved from **${d['sourceColumn']}** to **${d['targetColumn']}**` };

    case 'Deleted':
      if (entry.entityType === 'Card')
        return { icon: 'trash', text: `**${displayId}** "${d['cardTitle']}" deleted` };
      if (entry.entityType === 'Column')
        return { icon: 'trash', text: `Column **${d['columnName']}** deleted` };
      if (entry.entityType === 'Label')
        return { icon: 'trash', text: `Label **${d['labelName']}** deleted` };
      return { icon: 'trash', text: `Board **${d['boardName']}** deleted` };

    case 'Archived':
      return { icon: 'box-archive', text: `**${displayId}** "${d['cardTitle']}" archived` };

    case 'Restored':
      return { icon: 'rotate-left', text: `**${displayId}** "${d['cardTitle']}" restored` };

    case 'Renamed':
      return { icon: 'pencil', text: `Board renamed from **${d['oldName']}** to **${d['newName']}**` };

    case 'Reordered':
      return { icon: 'arrows-up-down', text: 'Columns reordered' };

    case 'LabelAdded':
      return { icon: 'tag', text: `Label **${d['labelName']}** added to **${displayId}**` };

    case 'LabelRemoved':
      return { icon: 'tag', text: `Label **${d['labelName']}** removed from **${displayId}**` };

    case 'PrefixUpdated':
      return { icon: 'pencil', text: `Board prefix changed from **${d['oldPrefix']}** to **${d['newPrefix']}**` };

    default:
      return { icon: 'circle-info', text: 'Unknown activity' };
  }
}
