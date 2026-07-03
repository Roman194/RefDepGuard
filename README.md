# RefDepGuard
This extension for Visual Studio 2022 allows you to track changes in dependencies between projects and map these dependencies to the rules specified in a special configuration file. If the link changes don't match the specified settings, the extension issues an error about incorrect links and prevents the build from completing.

The purpose of this tool is to control the relationships in solutions and ensure that they comply with the standard view adopted in your company. Implemented as part of the build administration.

# Link on docs
[Starter Guide](https://github.com/Roman194/RefDepGuard/blob/master/STARTER_GUIDE.md)</br>

[User_Guide](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md)</br>

[Console app User Guide](https://github.com/Roman194/RefDepGuard/blob/master/CONSOLE_USER_GUIDE.md)</br>

# Features

- analysis of the current status of projects (dependencies / TargetFramework versions) for its compliance with the rules stated in the config files
- tracking changes in interproject relationships since the current commit
- notifying the user of detected inconsistencies and displaying into  VS22 Error List panel information about specific "problems"
- recording of the current status of projects and detected "problems" in the format of a tabular (XLSX) and graphical report (Graph Dependency / HTML)
- RU/EN full localization (including docs)
- console version that is designed to work in environments without an installed IDE (implements 1-st and 3rd features, can be integrated into CI/CD pipline as auto-test)

_To learn more about the RefDepGuard features, see [USER_GUIDE](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md)!_

# Used stack

- [Visual Studio SDK](https://www.nuget.org/packages/Microsoft.VisualStudio.SDK/17.0.32112.339?_src=template)
- [MSBuild API](https://www.nuget.org/packages/Microsoft.Build/17.3.2?_src=template)
- [NETStandard.Library](https://www.nuget.org/packages/NETStandard.Library/2.0.3?_src=template)
- [Microsoft.Extensions.Localization](https://www.nuget.org/packages/Microsoft.Extensions.Localization/10.0.7?_src=template)
- [Microsoft.Office.Interop.Excel](https://learn.microsoft.com/ru-ru/dotnet/api/microsoft.office.interop.excel?view=excel-pia)
- [Newtonsoft.JSON](https://www.nuget.org/packages/Newtonsoft.Json/13.0.4?_src=template)
- [HtmlAgilityPack](https://www.nuget.org/packages/HtmlAgilityPack/1.12.4?_src=template)
- [OneOf](https://www.nuget.org/packages/OneOf/3.0.271?_src=template)
- [Memaid API](https://mermaid.js.org/intro/getting-started.html)

# Requirements
- Extension
Currently works for Visual Studio 2022+. </br>There was an attempt to make it working on Visual Stuidio 2019 but it looks like it's not working(
- Console
Currently works for Windows 10+ PCs

# License
RefDepGuard uses [MIT License](https://github.com/Roman194/RefDepGuard/blob/master/LICENSE.txt)

# Support
If you are experiencing issues, please open an issue with details and reproduction steps

# Contributions
If you want to make RefDepGuard better, you're welcome!</br>
Please use Pull request functionality for that. Branch off from 'develop' and make your changes inside 'feature/X' and put a PR to 'develop' when you ends your changes. 
Once approved, all pending changes (multiple PRs) will be merged to 'main' for a release to be distributed via VS Marketplace.

# Screenshots

<img width="530" height="171" alt="image" src="https://github.com/user-attachments/assets/cc9814fa-bee9-4a3d-8421-957c9da1b072" />

<img width="1867" height="370" alt="image" src="https://github.com/user-attachments/assets/681f8302-f85a-456b-b505-12f18445107f" />

<img width="324" height="616" alt="image" src="https://github.com/user-attachments/assets/d09572d9-e98c-44ee-9bc6-b093f615693a" /></br>

<img width="326" height="554" alt="image" src="https://github.com/user-attachments/assets/37b2828f-8319-4e8a-8bc8-8dbd3d362bbf" /></br>

<img width="389" height="225" alt="image" src="https://github.com/user-attachments/assets/2068ea78-69fa-4422-82b5-a7acc51a7156" />

<img width="1456" height="392" alt="image" src="https://github.com/user-attachments/assets/415c5d49-f3de-41a5-b5d3-28ce1e5cfac0" />

<img width="654" height="672" alt="image" src="https://github.com/user-attachments/assets/5f03e08e-5cdd-4ee4-ace5-698d46f1187c" />

<img width="1531" height="956" alt="image" src="https://github.com/user-attachments/assets/aaee6dcb-d544-4ce1-a930-0f30a3641e43" />
</br>
<img width="710" height="943" alt="image" src="https://github.com/user-attachments/assets/46682050-9716-4e40-8c9e-3b01ae5ca13d" />
