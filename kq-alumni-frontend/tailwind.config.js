/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './src/pages/**/*.{js,ts,jsx,tsx,mdx}',
    './src/components/**/*.{js,ts,jsx,tsx,mdx}',
    './src/app/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  theme: {
    extend: {
      colors: {
        'kq-red': '#ed1c24',
        'kq-red-dark': '#c41520',
        'kq-dark': '#0d0d0d',
        'kq-light': '#f5f5f5',
        'kq-border': '#e0e0e0',
        'navy-900': '#0f172a',
        'navy-800': '#1e293b',
      },
      fontFamily: {
        cabrito: ['Cabrito Flare', 'serif'],
        roboto: ['Roboto Flex', 'sans-serif'],
      },
    },
  },
  plugins: [],
};
