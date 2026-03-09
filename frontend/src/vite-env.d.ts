/// <reference types="vite/client" />

declare module "*.css";
declare module "react" {
  interface CSSProperties {
    [key: `--${string}`]: string | number;
  }
}
