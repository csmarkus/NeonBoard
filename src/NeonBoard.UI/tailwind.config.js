/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        void: '#09090b',
        surface: {
          DEFAULT: '#18181b',
          elevated: '#27272a',
        },
        subtle: '#27272a',
        dim: '#3f3f46',
        primary: '#fafafa',
        secondary: '#a1a1aa',
        muted: '#71717a',
        accent: '#22d3ee',
        danger: '#f87171',
        status: {
          todo: '#22d3ee',
          progress: '#fbbf24',
          review: '#a78bfa',
          done: '#4ade80',
        },
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', '-apple-system', 'sans-serif'],
      },
      keyframes: {
        'slide-in-right': {
          '0%': { transform: 'translateX(100%)', opacity: '0' },
          '100%': { transform: 'translateX(0)', opacity: '1' },
        },
      },
      animation: {
        'slide-in-right': 'slide-in-right 200ms ease-out',
      },
    },
  },
  plugins: [],
}
