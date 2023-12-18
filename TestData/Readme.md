# Test Data for Ethanol Solution

This directory is dedicated to housing various sets of test data, each tailored to specific functionalities within the Ethanol solution. The test data is organized into separate folders, reflecting the different components of Ethanol. These individual test folders allow for focused testing on specific functionalities.

## Test Folders Overview

Each folder contains test cases and data relevant to a particular component of the Ethanol solution:

### 1. ContextBuilder

- **Purpose**: Contains test data for testing the ContextBuilder component. This component is responsible for building context from raw data inputs.
- **Tests Included**: Various scenarios to validate the correctness and efficiency of the context building process.
- **Running Tests**: Navigate to the `ContextBuilder` folder and execute `test.ps1` to perform tests specific to this component.

### 2. ContextProvider

- **Purpose**: Includes tests for the ContextProvider functionality, which serves context data to other consumers via REST API.
- **Tests Included**: Tests to ensure the reliability and accuracy of the context data provided.
- **Running Tests**: In the `ContextProvider` folder, use `test.ps1` to run all tests related to context provision.

### 3. MalwareSonar

- **Purpose**: Dedicated to testing the MalwareSonar component, which is crucial for identifying and analyzing potential malware-related activities.
- **Tests Included**: A range of test cases to assess the detection capabilities and response accuracy of MalwareSonar.
- **Running Tests**: Use `test.ps1` within the `MalwareSonar` folder to execute these specific malware-related tests.

## Running Tests

You have the flexibility to run tests either individually for each component or globally for the entire Ethanol solution.

### Running Individual Tests

- Navigate to the desired test folder (`ContextBuilder`, `ContextProvider`, or `MalwareSonar`).
- Execute the `test.ps1` script to run tests specific to that component:

  ```bash
  $ pwsh ./ContextBuilder/test.ps1
  ```

### Running All Tests Globally

- To run all tests across all components, use the global `test.ps1` script located in the root test data folder:

  ```bash
  $ pwsh test.ps1
  ```

This global script sequentially executes the individual tests for each component, providing a comprehensive coverage of the Ethanol solution.
