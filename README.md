# ValkWelding Kunststof Lassen Project
ValkWelding Kunststof Lassen project for the Minor Mechatronics at the Rotterdam University of Applied Sciences.

This repository contains the code for the PolyTouch Cobot control application. The application allows users to define a series of positions for the robot to move to, detect objects at those positions, and perform milling operations.

## Prerequisites
Before you begin, ensure you have met the following requirements:

* You have installed the latest version of .NET Framework 4.7.2 or later.
* You have installed the latest version of Visual Studio 2019 or later.

### Installing
[Github Repository](https://github.com/Inzui/ValkWeldingKunststof)

    1. Clone the repository to your local machine.
    2. Open the solution in Visual Studio.
    3. Build the solution.

# Running the Application
Once you have built the solution, you can run the application using the Visual Studio Debugger. The main window of the application allows you to define a series of positions for the robot to move to. You can add, remove, import, and export positions. The application also provides functionality to connect to the Cobot and Detection Module, and to start the milling process.

# Understanding the Structure
The solution consists of three key components:

### Touch PoC and ObjectFollowerPoc
    These directories contain the initial prototypes of the application. They serve as a foundation for the subsequent development.
### Valkwelding Kunststof Lassen\Valkwelding Kunststof Lassen
    This directory contains the main application code for the PolyTouch software.
### Valkwelding Kunststof Lassen\DetectionModule
    This directory contains the code for the Detection Module, which is responsible for detecting objects at the measure points.

## Built With
* [.NET Framework](https://dotnet.microsoft.com/) - The framework used
* [Visual Studio](https://visualstudio.microsoft.com/) - The IDE used

## Authors
* **I. Zuiderent (1004784)** - *Computer Engineering*
* **J. van Wamelen (1009652)** - *Computer Engineering*
* **R. Kornet (1003214)** - *Computer Engineering*
* **N. van Hughten (1010482)** - *Mechanical Engineering*