# Google Maps UI Test Automation

This repository contains UI tests for Google Maps search functionality using Selenium WebDriver and NUnit.

## Description

This project aims to automate the testing of Google Maps search functionality to ensure its accuracy and reliability. By using Selenium WebDriver with NUnit, we can simulate user interactions with the Google Maps interface and verify that search queries produce the expected results.

## Table of Contents

- [Installation](#installation)
- [Usage](#usage)
- [Test Cases Document](#test-cases-document)
- [Code Coverage Report](#code-coverage-report)
- [Continuous Integration](#continuous-integration)

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

## Usage

Run the tests using the test runner in your IDE or using the following command.

```bash
dotnet test -v n
```

## Test Cases Document

Refer to the [TestCases.md](TestCases.md) file for detailed descriptions of the test cases covered by the UI tests.

## Code Coverage Report

The code coverage report can be found in the [CoverageReport](CoverageReport) directory. Open the HTML file in a web browser to view the detailed coverage statistics.

## Continuous Integration

Continuous integration (CI) is set up for this project using GitHub Actions. Every push or pull request to the repository triggers the CI pipeline, which automatically builds the project, and runs the tests.
