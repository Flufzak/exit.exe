import AppLayout from "./layout/AppLayout";
import Home from "./pages/Home";
import LoginPage from "./pages/LoginPage";
import StoriesPage from "./pages/StoriesPage";
import "./styles/App.css";
import { BrowserRouter, Routes, Route } from "react-router-dom";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<AppLayout />}>
          <Route path="/" element={<Home />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/stories" element={<StoriesPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
