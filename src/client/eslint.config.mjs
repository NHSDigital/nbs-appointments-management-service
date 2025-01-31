import { fixupConfigRules, fixupPluginRules } from "@eslint/compat";
import typescriptEslint from "@typescript-eslint/eslint-plugin";
import reactHooks from "eslint-plugin-react-hooks";
import globals from "globals";
import tsParser from "@typescript-eslint/parser";
import path from "node:path";
import { fileURLToPath } from "node:url";
import js from "@eslint/js";
import { FlatCompat } from "@eslint/eslintrc";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const compat = new FlatCompat({
    baseDirectory: __dirname,
    recommendedConfig: js.configs.recommended,
    allConfig: js.configs.all
});

export default [...fixupConfigRules(compat.extends(
    "next/core-web-vitals",
    "plugin:@typescript-eslint/recommended",
    "plugin:react/recommended",
    "plugin:react-hooks/recommended",
    "plugin:jsx-a11y/recommended",
    "plugin:import/typescript",
    "plugin:@next/next/recommended",
    "prettier",
    "prettier/prettier",
)), {
    plugins: {
        "@typescript-eslint": fixupPluginRules(typescriptEslint),
        "react-hooks": fixupPluginRules(reactHooks),
    },

    languageOptions: {
        globals: {
            ...globals.browser,
            ...globals.jest,
            ...globals.node,
        },

        parser: tsParser,
    },

    settings: {
        "import/resolver": {
            typescript: {
                project: "./src",
            },
        },

        react: {
            version: "detect",
        },
    },

    rules: {
        "@typescript-eslint/ban-ts-ignore": "off",
        "@typescript-eslint/default-param-last": "error",
        "@typescript-eslint/explicit-function-return-type": "off",
        "@typescript-eslint/no-empty-function": "off",
        "@typescript-eslint/no-shadow": "error",

        "@typescript-eslint/no-unused-vars": ["warn", {
            argsIgnorePattern: "^_$",
        }],

        "@typescript-eslint/no-useless-constructor": "error",
        "@typescript-eslint/triple-slash-reference": "off",
        "@typescript-eslint/explicit-module-boundary-types": "off",
        "class-methods-use-this": "off",
        "default-param-last": "off",
        "import/extensions": "off",
        "import/no-cycle": "off",

        "import/no-extraneous-dependencies": ["error", {
            devDependencies: true,
        }],

        "import/no-unresolved": "off",

        "lines-between-class-members": ["error", "always", {
            exceptAfterSingleLine: true,
        }],

        "react/forbid-prop-types": "off",
        "react/function-component-definition": "off",

        "react/no-unstable-nested-components": ["error", {
            allowAsProps: true,
        }],

        "react/no-unescaped-entities": ["error", {
            forbid: [{
                char: ">",
                alternatives: ["&gt;"],
            }, {
                char: "}",
                alternatives: ["&#125;"],
            }],
        }],

        "react/jsx-filename-extension": ["error", {
            extensions: [".jsx", ".tsx"],
        }],

        "react/jsx-curly-newline": "off",

        "react/jsx-no-useless-fragment": ["error", {
            allowExpressions: true,
        }],

        "react/jsx-one-expression-per-line": "off",

        "react/jsx-props-no-spreading": ["error", {
            html: "enforce",
            custom: "ignore",
        }],

        "react/jsx-wrap-multilines": ["error", {
            prop: "ignore",
        }],

        "react/react-in-jsx-scope": "off",
        "react/require-default-props": "off",
        "react/state-in-constructor": "off",
        "react/static-property-placement": ["error", "static public field"],
        "react/prop-types": "off",
        "jsx-a11y/anchor-is-valid": "off",

        "jsx-a11y/label-has-associated-control": ["error", {
            assert: "htmlFor",
        }],

        "no-console": "warn",

        "no-param-reassign": ["error", {
            props: true,
            ignorePropertyModificationsFor: ["draft", "acc"],
        }],

        "no-promise-executor-return": "off",

        "no-restricted-exports": ["error", {
            restrictDefaultExports: {
                defaultFrom: false,
            },
        }],

        "no-shadow": "off",

        "no-underscore-dangle": ["error", {
            allow: ["_def"],
        }],

        "no-unreachable": "error",
        "no-use-before-define": "off",
        "no-useless-constructor": "off",
    },
}, {
    files: ["**/*.js"],

    rules: {
        "@typescript-eslint/explicit-module-boundary-types": "off",
        "@typescript-eslint/no-var-requires": "off",
        "global-require": "off",
        "import/no-dynamic-require": "off",
        "no-console": "off",
    },
}];