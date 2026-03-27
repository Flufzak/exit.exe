import AppLayout from "./layout/AppLayout";
import HangmanPage from "./pages/Hangman";
import Home from "./pages/Home";
import StoriesPage from "./pages/StoriesPage";
import "./styles/App.css";
import { BrowserRouter, Routes, Route } from "react-router-dom";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<AppLayout />}>
          <Route path="/" element={<Home />} />
          <Route path="/stories" element={<StoriesPage />} />
          <Route path="/hangman" element={<HangmanPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
