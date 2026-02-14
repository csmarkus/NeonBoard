export interface Label {
  id: string;
  name: string;
  color: string;
}

export interface AddLabelRequest {
  name: string;
  color: string;
}

export interface UpdateLabelRequest {
  name: string;
  color: string;
}

export const LABEL_COLORS = [
  'red', 'orange', 'yellow', 'lime', 'cyan', 'blue', 'purple', 'violet', 'magenta', 'pink'
] as const;

export type LabelColor = typeof LABEL_COLORS[number];

export function getLabelColorClasses(color: string): { bg: string; text: string; border: string } {
  const colorMap: Record<string, { bg: string; text: string; border: string }> = {
    red: { bg: 'bg-red-500/20', text: 'text-red-400', border: 'border-red-500/30' },
    orange: { bg: 'bg-orange-500/20', text: 'text-orange-400', border: 'border-orange-500/30' },
    yellow: { bg: 'bg-yellow-600/20', text: 'text-yellow-500', border: 'border-yellow-600/30' },
    lime: { bg: 'bg-lime-500/20', text: 'text-lime-400', border: 'border-lime-500/30' },
    cyan: { bg: 'bg-cyan-500/20', text: 'text-cyan-400', border: 'border-cyan-500/30' },
    blue: { bg: 'bg-blue-500/20', text: 'text-blue-400', border: 'border-blue-500/30' },
    purple: { bg: 'bg-purple-500/20', text: 'text-purple-400', border: 'border-purple-500/30' },
    violet: { bg: 'bg-violet-500/20', text: 'text-violet-400', border: 'border-violet-500/30' },
    magenta: { bg: 'bg-fuchsia-500/20', text: 'text-fuchsia-400', border: 'border-fuchsia-500/30' },
    pink: { bg: 'bg-pink-500/20', text: 'text-pink-400', border: 'border-pink-500/30' },
  };
  return colorMap[color] ?? { bg: 'bg-gray-500/20', text: 'text-gray-400', border: 'border-gray-500/30' };
}
