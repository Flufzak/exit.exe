import Loader from "./components/ui/Loader";
import "./styles/App.css";

function App() {
  return (
    <div className="app">
      <div className="container">
        <h1>Exit.exe</h1>
        <Loader />
        <p className="message">UI scaffold ready</p>
      </div>
    </div>
  );
}

export default App;
