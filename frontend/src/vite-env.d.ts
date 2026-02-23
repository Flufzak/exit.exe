import { StrictMode } from "react";
import { createRoot } from "react-dom/client";

declare module "*.css";
declare module "react" {
  interface CSSProperties {
    [key: `--${string}`]: string | number;
  }
}
