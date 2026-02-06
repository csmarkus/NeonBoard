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
    },
  },
  plugins: [],
}
