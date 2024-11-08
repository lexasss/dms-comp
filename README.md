# DMS comparison tools

Display and compare DMS data collected with IonVision device (Olfactomics)

## Requirements:

- [.NET 8 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-8.0.10-windows-x64-installer)

## Features

Operates over DMS measurements collected from IonVision device (software version 1.5.x)

- Visualizes two DMS measurements
- Plots the difference between these two measurements
- Computes the distance between these two measurements

## Usage

Download and upzip the [latest release](https://github.com/lexasss/dms-comp/releases), then run the executable.

Click on the upper field to select a JSON file with DMS measurements,
Click on the lower field to select a JSON file with DMS measurements,

One two measurements are loaded, the software computes and display the distance between these measurements using the currently selected algorithm and data pre-processing options.