# 🗝️ Exit.exe

A story-driven online escape experience where players solve puzzles to progress through mysterious narratives.

Players enter a story, uncover clues, and solve a sequence of puzzles before time runs out. Each story contains its own atmosphere, mechanics, and challenges.

---

## 🎮 Concept

Exit.exe is a **browser-based escape room experience**.

Each story contains:

- A narrative setting
- Multiple puzzles
- A time limit
- A success or failure outcome

### Example Story: Kazimir

A medieval cult-themed escape experience where players must uncover hidden clues inside mysterious dungeons.

Puzzles include:

- Hangman
- Hidden numbers in images
- Sliding puzzle

Solve them all before the ritual ends.

---

## ✨ Features

- 📚 Story-based gameplay
- 🧩 Multiple puzzle types
- ⏳ Timed challenges
- 🌐 Multi-language support (i18n)
- 🎨 Custom UI

---

## 🛠️ Tech Stack

**Frontend**

- React
- TypeScript
- Styled-components

**Backend**

- .NET (ASP.NET Core)
- Entity Framework Core
- Scalar

## ⚠️ REQUIRED SETUP (DO THIS FIRST)

Before running the project, you **MUST** run these commands in Visual Studio (or terminal).

If you skip this, the app will not work as intended.

### 🗄️ Database setup

Run these commands **in order**:

```bash
dotnet ef database update --context Exit.exe.Repository.Auth.AuthDbContext --project Exit.exe.Repository --startup-project Exit.exe.Web
```

```bash
dotnet ef database update --context Exit.exe.Repository.Data.App.AppDbContext --project Exit.exe.Repository --startup-project Exit.exe.Web
```

---

### Frontend .env file

Create a `.env` file in your frontend project.

Add this:

    VITE_BACKEND_URL=https://localhost:7007

---

### Run the project

go to \Exit.exe.AppHost and run

```bash
dotnet run
```
