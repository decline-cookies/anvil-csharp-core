![License](https://img.shields.io/github/license/decline-cookies/anvil-csharp-core?label=License)&nbsp;&nbsp;&nbsp;
![.NET Build Status](https://github.com/decline-cookies/anvil-csharp-core/actions/workflows/dotnet-build.yml/badge.svg)&nbsp;&nbsp;&nbsp;
![Mono Build Status](https://github.com/decline-cookies/anvil-csharp-core/actions/workflows/mono-build.yml/badge.svg)&nbsp;&nbsp;&nbsp;
![Unit Tests Status](https://github.com/decline-cookies/anvil-csharp-core/actions/workflows/unit-tests.yml/badge.svg)&nbsp;&nbsp;&nbsp;

# anvil-csharp-core
An opinionated collection of systems and utilities that help you build applications in C# quickly, coherently and ready to scale. While Anvil is designed with realtime interactive experiences in mind it's well suited as the foundation for any application or platform where flexibility is key.
Features of Anvil are as simple to use as possible while remaining performant.

This is the ~~fourth~~🤞last rewrite of a common set of tools, patterns, and practices that are the result of hard lessons building countless novel interactive products built over the last 13 years in advertising, AR/VR, education, and gaming. The approach to organizing applications outlined by Anvil was the technical backbone of two award winning interactive studios and easily adopted by dozens of developer of all skill levels.

Most of the projects that leverage Anvil are built with [Unity](https://unity.com) but there are a few others including a console application built with [Spectre](https://spectreconsole.net) and [Terminal.Gui](https://github.com/migueldeicaza/gui.cs).

### Expectations
This library is currently being used and built along side a few actively developed projects. At the moment, Anvil is fairly barebones but its functionality will be fleshed out as the projects that depend on it move towards release.

The code is reasonably clean but documentation and examples are sparse. Feel free to [reach out on Twitter](https://twitter.com/declinecookies) or open issues with questions.

⚠️ We welcome PRs and bug reports but making this repo a public success is not our priority. No promises on when it will be addressed!

## Dependencies
 - [.NET Standard 2.1+](https://docs.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-1)

## Features
 - [ ] TODO: [Issue #75](https://github.com/decline-cookies/anvil-csharp-core/issues/75)

## Project Setup

*Note: To include this library in a Unity project, see [anvil-unity-core Project Setup](https://github.com/decline-cookies/anvil-unity-core#project-setup)*

1. Install anvil-csharp-core along side your project, for example:
    ```
    - YourProject
      - YourProject
        - YourProject.csproj
      - Anvil
        - anvil-csharp-core
      - YourProject.sln
    ```
    - *Note: If necessary, Anvil can be installed within a project, but must be ignored in the .csproj so it can act as a separate C# project*
2. Add `anvil-csharp-core.csproj` and `Logging/.CSProject/anvil-csharp-logging.csproj` to your .sln
    - *Optional: Add `Tests/anvil-csharp-tests.csproj` too if you want to write/run unit tests.*
3. Add a reference to anvil-csharp-core to your .csproj file.
4. Done!
