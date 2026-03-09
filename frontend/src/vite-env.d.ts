/// <reference types="vite/client" />

export {};

declare module "*.css";
declare module "react" {
  interface CSSProperties {
    [key: `--${string}`]: string | number;
  }
}
