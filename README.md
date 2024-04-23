# Google Maps UI Test Automation

This repository contains UI tests for Google Maps search functionality using Selenium WebDriver and NUnit.

## Description

This project aims to automate the testing of Google Maps search functionality to ensure its accuracy and reliability. By using Selenium WebDriver with NUnit, we can simulate user interactions with the Google Maps interface and verify that search queries produce the expected results.

## Installation

1. Ensure that you have .NET 8.0 installed in your system.

2. Clone the repository:

```bash
git clone git@github.com:hashirsarwar/google-maps-ui-testing.git
```

3. Navigate to the project directory:

```bash
cd google-maps-ui-tests
```

4. Install dependencies:

```bash
dotnet restore
```

5. Build the project:

```bash
dotnet build
```

Note: If you use an IDE, you may not need to restore and build manually.

## Usage

Run the tests using the test runner in your IDE or using the following command.

```bash
dotnet test
```

In case of a test method failure, a screenshot will be captured and stored at Tests/UI/Screenshots.

## Test Cases Document

Open the [TestCases.md](Tests/Docs/TestCases.md) file for detailed descriptions of the test cases covered by the UI tests.

## Code Coverage Report

The code coverage report can be found at [CodeCoverageReport.html](Tests/Docs/CodeCoverageReport.html).

## Continuous Integration

This project uses GitHub Actions to implement continuous integration. Every push or pull request to the repository triggers the CI pipeline, which automatically builds the project, and runs the tests.
