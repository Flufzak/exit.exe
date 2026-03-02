import { Outlet } from "react-router-dom";

export default function AppLayout() {
  return (
    <>
      {/* <Header /> */}
      <main className="content">
        <Outlet />
      </main>
    </>
  );
}
