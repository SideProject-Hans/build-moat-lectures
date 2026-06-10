# CLAUDE.md — 12-rule template

These rules apply to every task in this project unless explicitly overridden.
Bias: caution over speed on non-trivial work. Use judgment on trivial tasks.

## Rule 1 — Think Before Coding
State assumptions explicitly. If uncertain, ask rather than guess.
Present multiple interpretations when ambiguity exists.
Push back when a simpler approach exists.
Stop when confused. Name what's unclear.

## Rule 2 — Simplicity First
Minimum code that solves the problem. Nothing speculative.
No features beyond what was asked. No abstractions for single-use code.
Test: would a senior engineer say this is overcomplicated? If yes, simplify.

## Rule 3 — Surgical Changes
Touch only what you must. Clean up only your own mess.
Don't "improve" adjacent code, comments, or formatting.
Don't refactor what isn't broken. Match existing style.

## Rule 4 — Goal-Driven Execution
Define success criteria. Loop until verified.
Don't follow steps. Define success and iterate.
Strong success criteria let you loop independently.

## Rule 5 — Use the model only for judgment calls
Use me for: classification, drafting, summarization, extraction.
Do NOT use me for: routing, retries, deterministic transforms.
If code can answer, code answers.

## Rule 6 — Token budgets are not advisory
Per-task: 4,000 tokens. Per-session: 30,000 tokens.
If approaching budget, summarize and start fresh.
Surface the breach. Do not silently overrun.

## Rule 7 — Surface conflicts, don't average them
If two patterns contradict, pick one (more recent / more tested).
Explain why. Flag the other for cleanup.
Don't blend conflicting patterns.

## Rule 8 — Read before you write
Before adding code, read exports, immediate callers, shared utilities.
"Looks orthogonal" is dangerous. If unsure why code is structured a way, ask.

## Rule 9 — Tests verify intent, not just behavior
Tests must encode WHY behavior matters, not just WHAT it does.
A test that can't fail when business logic changes is wrong.

## Rule 10 — Checkpoint after every significant step
Summarize what was done, what's verified, what's left.
Don't continue from a state you can't describe back.
If you lose track, stop and restate.

## Rule 11 — Match the codebase's conventions, even if you disagree
Conformance > taste inside the codebase.
If you genuinely think a convention is harmful, surface it. Don't fork silently.

## Rule 12 — Fail loud
"Completed" is wrong if anything was skipped silently.
"Tests pass" is wrong if any were skipped.
Default to surface uncertainty, not hiding it.

## Rule 13 — TDD first, SDD as assistive
Use TDD as the default delivery mode.
Every behavior change must start with a failing test in xUnit and implement only enough code to pass.
After passing, refactor only inside the same behavior contract.
Use SDD only for human-readable acceptance of high-value user flows, not as a substitute for unit/component tests.

## Rule 14 — Test-stack order for this project
Priority order is fixed:
1. Unit tests (xUnit) for service/domain/business logic.
2. Component tests (bUnit + xUnit) for Razor component behavior.
3. Integration tests (xUnit + WebApplicationFactory) for route/request/user-flow sanity.
4. SDD/BDD (Reqnroll + xUnit) only when process alignment and regression traceability are needed.

## Rule 15 — Red-Green-Refactor discipline
Do not write production code to satisfy a test without first proving the test fails.
Each TDD cycle must include:
Red -> Green -> Refactor
before declaring a task "done".
If a behavior can be described with one `.feature`, keep it there, but keep unit/component tests as the implementation contract.

## Rule 16 — Merge quality gates
No change is considered complete unless:
- The related tests exist and are tied to the requirement.
- `dotnet test` passes for the touched test layer(s).
- If `.feature` docs exist, they are updated together with implementation and test updates.

## Rule 17 — Open source references for TDD/SDD adoption
Use these projects/docs as implementation references when starting a new feature:

- bUnit component testing: https://github.com/bUnit-dev/bUnit
- bUnit usage examples: https://github.com/DevExpress-Examples/blazor-bunit-tests
- BDD (Reqnroll): https://github.com/reqnroll/Reqnroll
- ASP.NET Core integration test samples: https://learn.microsoft.com/aspnet/core/test/integration-tests
- Blazor official sample repository: https://github.com/dotnet/blazor-samples

## Rule 18 — Project-level workflow for this repo
- Start every behavior change from a failing test at the smallest layer first (service/domain), then move upward.
- TDD cycle:
  1. Red: add/update a failing unit test for the behavior change.
  2. Green: implement the minimum production code to pass.
  3. Refactor: keep the same contract and simplify.
- Only add SDD (`.feature`) when a feature needs human-readable acceptance criteria or cross-team handoff.
- Keep test layering strict:
  1. `tests/*/Unit` for behavior logic.
  2. `tests/*/Component` for Razor interaction.
  3. `tests/*/Integration` for route and middleware/user-flow checks.
- For each story, record:
  - Why test is needed.
  - Which layer proves it.
  - Whether SDD exists and what behavior it describes.
