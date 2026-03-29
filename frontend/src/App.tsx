import AppLayout from "./layout/AppLayout";
import Home from "./pages/Home";
import LoginPage from "./pages/LoginPage";
import ProfilePage from "./pages/ProfilePage";
import NotFound from "./pages/NotFound";
import StoriesPage from "./pages/StoriesPage";
import ProtectedRoute from "./routes/ProtectedRoute";
import "./styles/App.css";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import LosePage from "./pages/LosePage";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<AppLayout />}>
          <Route path="/" element={<Home />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/stories" element={<StoriesPage />} />

          <Route element={<ProtectedRoute />}>
            <Route path="/profile" element={<ProfilePage />} />
            <Route path="/lost" element={<LosePage />} />
          </Route>

          <Route path="*" element={<NotFound />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
