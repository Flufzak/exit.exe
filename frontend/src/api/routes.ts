const API_URL = import.meta.env.VITE_BACKEND_URL;

export const authRoutes = {
  google: `${API_URL}/api/auth/login/google`,
  facebook: `${API_URL}/api/auth/login/facebook`,
};
