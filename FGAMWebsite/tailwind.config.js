/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#0e4391', // Jia Hsin Royal Blue
          dark: '#0a3575',
          light: '#1e5bb3',
        },
        accent: {
          DEFAULT: '#00df89', // Jia Hsin Mint Green Accent
          light: '#34d399',
        },
      },
      fontFamily: {
        sans: ['Inter', 'Outfit', 'system-ui', 'sans-serif'],
      }
    },
  },
  plugins: [],
}
