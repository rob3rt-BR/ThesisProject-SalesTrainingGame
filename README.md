# Sales Training Game – Adaptive AI Dialogue vs. Traditional Training

A serious game for sales training that combines gamification and microlearning, developed with the Unity game engine as part of the bachelor's thesis
*"Adaptive AI Dialogue in a Sales Training Game: Effectiveness Compared to a Traditional Approach"*
(Technical University of Munich, TUM School of Computation, Information and Technology, 2026).

The game teaches the sales topic **Cold Outreach** in short microlearning modules and exists in two training versions:

- **Traditional version:** after each information text, the questions are presented as a fast sequence of questions and answers.
- **AI dialogue version:** the same questions are embedded in a simulated sales conversation. A large language model (LLM), accessed through [OpenRouter](https://openrouter.ai), plays a virtual customer that reacts to predefined salesperson statements based on the answer selected by the player.

Both versions share the same learning content, questions, time limits and scoring, so they can be compared in a user study. The AI dialogue version additionally needs the local **Dialogue Backend** service contained in this repository.

---

## Contents

- [Repository structure](#repository-structure)
- [Requirements](#requirements)
- [Getting started](#getting-started)
- [The game](#the-game)
- [The dialogue backend](#the-dialogue-backend)
- [Recorded session data](#recorded-session-data)
- [Known limitations](#known-limitations)

---

## Repository structure

```
ThesisProject-SalesTrainingGame/
├── Adaptive_Sales_Training/          Unity project
│   └── Assets/
│       ├── Data/                     Learning content and settings as ScriptableObjects
│       │   ├── Topics/               Sales topics (list of modules)
│       │   ├── Modules/              Microlearning modules
│       │   ├── AI Companion/         Dialogue scenarios for the AI version
│       │   └── ExplainScreenContent/ Introduction texts for both versions
│       ├── Scenes/                   Start, topic menu, training (traditional / AI), results, leaderboards
│       ├── Scripts/
│       │   ├── Gameplay/             Training flow, session management
│       │   ├── UI/                   Screen controllers and animations
│       │   ├── Data/                 ScriptableObject definitions
│       │   ├── DataLogging/          Session data recording structures
│       │   └── AIDialogue/           Request / response models for the backend
│       ├── Prefabs/ and UI/          UI prefabs and graphics
└── DialogueBackend/                  Local ASP.NET Core service between the game and OpenRouter
    ├── SalesTrainingAIBackend.sln
    └── SalesTrainingAIBackend/
        ├── Program.cs                Endpoint and error handling
        ├── OpenRouterService.cs      Prompt construction and OpenRouter request
        ├── DialogueModels.cs         Request / response models (game ↔ backend)
        ├── OpenRouterResponseModels.cs
        ├── ProgramExceptions.cs
        └── appsettings.json          Model configuration
```

## Requirements

| Component | Requirement |
|---|---|
| Game | [Unity](https://unity.com) **6000.6.0f1** |
| Backend | [.NET SDK 10](https://dotnet.microsoft.com/download) |
| AI dialogue | An [OpenRouter](https://openrouter.ai) account and API key (free models are supported) |

The backend is only needed for the AI dialogue version. The traditional version runs on its own.

## Getting started

### 1. Start the dialogue backend (AI dialogue version only)

1. Set your OpenRouter API key as an environment variable. The key is **never** stored in the repository.

   PowerShell:
   ```powershell
   $env:OPENROUTER_API_KEY = "<your-openrouter-api-key>"
   ```
   Bash:
   ```bash
   export OPENROUTER_API_KEY="<your-openrouter-api-key>"
   ```

2. Optionally adjust the model settings in `DialogueBackend/SalesTrainingAIBackend/appsettings.json` (see [Configuration](#configuration)).

3. Start the service from the same terminal:
   ```bash
   cd DialogueBackend/SalesTrainingAIBackend
   dotnet run
   ```
   The service must keep running while the AI version is being played so the game works as intended. It listens on `http://localhost:5065` by default. To use another port, change `backendUrl` in `Program.cs` and the **Backend URL** field of the `AIDialogueController` component in `AITrainingScene`.

### 2. Open and run the game

1. Open the folder `Adaptive_Sales_Training` with Unity Hub.
2. Open `Assets/Scenes/StartScene.unity` and press Play, or run the final game by creating a build via *File → Build Profiles*.

### 3. Set up a training session

Before a participant plays, the session is configured on a protected screen:

1. On the start screen, open the session setup. It is protected by a password defined in `StartScreenController.cs`, so participants cannot change the configuration.
2. Enter the **participant code**. It must have the format `P-` followed by eight characters (for example `P-1a2b3c4d`). The same code is used to link the game data to other study data (e.g. a questionnaire).
3. Select the **training version** (traditional or AI dialogue).
4. Return to the start screen and start the game.

## The game

### Session flow

1. **Introduction:** a few screens explain the module structure, both module phases, time limits, scoring and the assigned training version.
2. **Topic menu:** the player starts the topic (only *Cold Outreach* available at the moment). Completed topics are marked and cannot be started again.
3. **Modules:** a topic consists of five sequential modules. Each module has
   - an **information phase**: a short informative text in German,
   - an **interaction phase**: seven questions on the text (single-choice and true-or-false questions),
   - an **intermediate score screen** with the points and correct answers of the module.
4. **Final score screen:** total points, correct answers and average response time.

| Module | Title (German) | Content |
|---|---|---|
| CO-M01 | Grundlagen von Cold Outreach | Definition, contact channels, goal of the first contact, value proposition |
| CO-M02 | Targeting und Prospect Research | Ideal Customer Profile (ICP), trigger events, CRM documentation |
| CO-M03 | Die erste Nachricht gestalten | Structure of the first message, benefits over features, call-to-action |
| CO-M04 | Discovery und Objections | Open-ended questions, active listening, handling objections |
| CO-M05 | Follow-up, Cadence und Metriken | Contact cadence, persistence without pressure, quality metrics |

The repository also contains a second topic, *Upselling and Cross-Selling* (modules `UC-M01` to `UC-M05` and their dialogue scenarios). It is not included in the current version because the study was later limited to one topic.

### Gamification and feedback

- **Timer:** a timer (emptying bar and seconds counter) per question. A question that runs out of time counts as wrong.
- **Scoring:** a correct answer gives base points plus a speed bonus proportional to the remaining time:

  `points = base points + round(bonus points × remaining time / time limit)`.
- **Immediate feedback:** correct / wrong answer indicators, and the awarded points at a random position around the question box.
- **Animations:** animated screen transitions, text effects and counting score values.

### AI dialogue version

Each module starts with a sales scenario and an opening message from the virtual customer. After every answer, a predefined salesperson statement for the selected answer is displayed, and the backend generates the customer's reply. After a short waiting time for the backend response, the customer reply is displayed for the player to read and decide when to continue to the next question.

If no valid reply is received, the dialogue is shown as unavailable, but the player can still continue to the next question. The answer and score are recorded as usual, and the missing reply is not added to the conversation history.

### Leaderboards

The start screen links to a leaderboard scene. It ranks players by points and by response time, separately for each training version and overall, and has a top-five view. The entries come from `PlayerStats` ScriptableObjects in `Assets/Data/PlayerStats`. Currently predefined assets contain player data and are **excluded from the repository**.

To use the leaderboards, create your own `PlayerStats` assets (*Create → Scriptable Objects → PlayerStats*) and assign them in the `LeaderboardsController` of `LeaderboardsScene`.

### Editing the learning content

All content is stored as ScriptableObjects in `Assets/Data` and can be edited in the Unity Inspector:

| Asset type | Content |
|---|---|
| `SalesTopic` | Topic ID, title and list of modules |
| `MicroModule` | Module title, information text and questions (answer options, correct answer, time limit, base and bonus points) |
| `AIDialogueScenario` | Sales scenario, customer persona, opening message, and per question the dialogue context and the salesperson statements for each answer option |
| `ExplainScreenContent` | Introduction texts for each training version |

## The dialogue backend

A small ASP.NET Core minimal API that runs locally next to the game. It receives the dialogue requests of the game, builds the prompt, calls the OpenRouter chat completions API and returns the customer reply.

```
Unity game ──▶ HTTP POST /api/dialogue ──▶ Dialogue backend ──▶ OpenRouter API ──▶ LLM
           ◀────── customer reply ────────                                     ◀──
```

### Endpoint

`POST http://localhost:5065/api/dialogue`

(default port; see [Getting started](#getting-started) to change it)

Request body (JSON):

| Field | Description |
|---|---|
| `scenarioId`, `moduleId`, `questionId` | Identifiers of the scenario, module and question |
| `customerPersona`, `scenarioContext` | Role / persona of the virtual customer and the sales scenario |
| `questionDialogueContext` | How the customer should react to this question, including a transition question to the next one |
| `questionText`, `selectedAnswer` | The current question and the answer selected by the player |
| `traineeDialogueText` | The predefined salesperson statement for the selected answer |
| `conversationHistory` | Earlier messages of the module (`role`, `content`). The game resets it at the start of each module. |

Response body (JSON): `responseId`, `model`, `promptTokens`, `completionTokens`, `responseTimeMilliseconds` and `replyText`.

### Prompt structure

The backend sends four parts to the model, all in German:

1. **System message:** how the model should act *(customer in a sales scenario)* and the rules it must follow when replying.
2. **Scenario context:** the sales scenario and the customer persona.
3. **Conversation history** of the current module.
4. **User message:** the dialogue context of the current question, the salesperson statement and a short reminder of the rules.

### Configuration

`appsettings.json`, section `OpenRouter`:

| Key | Meaning |
|---|---|
| `BaseUrl` | OpenRouter API base URL (`https://openrouter.ai/api/v1/`) |
| `Model` | OpenRouter model ID, e.g. a free model ending in `:free` |
| `MaxTokens` | Maximum length of the reply |
| `Temperature` | Sampling temperature (0.3 by default, for consistent replies) |

Model reasoning is turned off to reduce the response time, and OpenRouter provider fallbacks are allowed. The API key is read only from the environment variable `OPENROUTER_API_KEY`.

### Error handling

| Situation | Status code returned to the game |
|---|---|
| OpenRouter rate limit or provider at capacity (OpenRouter 429) | 429 |
| OpenRouter request failed or returned an invalid response | 502 |
| Missing API key, missing model configuration or empty model reply | 500 |

The maximum waiting time for an OpenRouter response is 75 seconds. Every request is logged to the console with model, IDs, token usage and response time, which helps to identify failing requests.

## Recorded session data

At the end of a session, the game stores the results of the participant as JSON. It saves them in Unity's `Application.persistentDataPath` and exports a copy to:

```
Documents/SalesTrainingData/Participant_Sessions/FinalTrainingData/<participant code>.json
```

If a session ends after a topic while other topics are still open, intermediate results are written to `.../IntermediateTrainingData/` instead.

The data follows the training hierarchy `TrainingResultData → TopicResult → ModuleResult → QuestionResult`:

- **Session:** participant code, training version, start and end time, total points, correct answers, final average response time.
- **Topic / module:** points, correct answers (and the average response time per topic).
- **Question:** selected answer, correct or timed out, response time, points. In the AI dialogue version, each question also records the model's reply (`AIResponseLog`): response ID, model, total request time and reply text.

The participant code is the only identifier in these files.

## Known limitations

- The backend runs locally, so the AI dialogue version only works on the machine where the backend is running.
- Free OpenRouter models can be rate-limited, run out of capacity, or be removed at short notice. The model used in the thesis study (`inclusionai/ling-3.0-flash-sante:free`) is no longer available. Change `Model` in `appsettings.json` if replies fail.
