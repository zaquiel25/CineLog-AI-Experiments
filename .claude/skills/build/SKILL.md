---
name: build
description: Build the FrameRoute project and report results. Use after code changes to verify compilation.
disable-model-invocation: false
allowed-tools: ["Bash"]
---

# Build Verification

Run `dotnet build` in the project directory and report the result clearly.

If the build fails:
1. Show the error messages
2. Identify the likely cause
3. Suggest a fix

If the build succeeds:
1. Confirm success with warning count (if any)

$ARGUMENTS
