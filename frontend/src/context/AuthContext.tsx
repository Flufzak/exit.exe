import { createContext } from "react";
import { MeResponse } from "../types/auth";

export type AuthContextType = {
  user: MeResponse | null;
  loading: boolean;
  isAuthenticated: boolean;
  refetch: () => Promise<void>;
  loginWithGoogle: () => void;
  loginWithFacebook: () => void;
  logout: () => Promise<void>;
};

export const AuthContext = createContext<AuthContextType | undefined>(
  undefined,
);
