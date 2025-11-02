/** @type {import("prettier").Config} */

const config = {
  // Line width
  printWidth: 100,

  // Tabs vs Spaces
  useTabs: false,
  tabWidth: 2,

  // Semicolons
  semi: true,

  // Quotes
  singleQuote: true,
  quoteProps: "as-needed",

  // JSX
  jsxSingleQuote: false,
  jsxBracketSameLine: false,

  // Trailing commas
  trailingComma: "es5",

  // Spacing
  bracketSpacing: true,
  arrowParens: "always",

  // Prose wrapping
  proseWrap: "preserve",

  // HTML whitespace sensitivity
  htmlWhitespaceSensitivity: "css",

  // Line endings
  endOfLine: "lf",

  // Plugins
  plugins: [],
};

module.exports = config;
