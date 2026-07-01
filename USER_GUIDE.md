# General Information
This extension for Visual Studio 2022 allows you to track changes in dependencies between projects and map these dependencies to the rules specified in a special configuration file. If the link changes don't match the specified settings, the extension issues an error about incorrect links and prevents the build from completing.

The purpose of this tool is to control the relationships in solutions and ensure that they comply with the standard view adopted in your company. Implemented as part of the build administration.

Information about installing the extension (Starter guide) can be found at [link](https://github.com/Roman194/RefDepGuard/blob/master/STARTER_GUIDE.md)

The extension also has a "lightweight" console version designed for integration into the CI/CD pipeline as an automated test. See the corresponding [console app USER_GUIDE](https://github.com/Roman194/RefDepGuard/blob/master/CONSOLE_USER_GUIDE.md) for a description of the features of the console program, as well as the actions required for its correct operation

# User guide-relevant version of the tool
2.1.0

# Accepted designations and abbreviations
- VS22 (abbreviated) - Visual Studio 22 is an integrated development environment (IDE), which is the target platform for the extension in question. This tool is a native solution for VS22 and has many implementation features related to this IDE.
-Solution - represents a single project space (container), which is the VS22 unit for separating the "project" workspace from everything else. VS22 assumes that any workspace must have exactly one solution. Simultaneous output of multiple solutions in a single window is not supported in VS22.
-Project - a program or component part of a program that performs certain functionality. One solution can contain either one or many projects. Projects can have links (references) between each other.
- Communication/Reference - some dependency of the project on a third-party or system library, as well as other projects. Within the framework of this document, reference and reference between projects are understood as synonymous concepts (since other types of project relationships are not considered).
- targetFramework - target working environment of the project. Indicates the project type and version. Example: netstandard2.0.
- TFM (Target framework monikier) - a standardized format token that specifies the current targetFramework of the project. Example: netstandard.
- Mandatory reference - a reference between projects that, according to the rules set in the configuration file, must necessarily exist for this project (or several projects at the Global and Solution levels).
- Invalid reference - a reference between projects that, according to the rules set in the configuration file, should never exist.
- Global Configuration file - A file that contains all the rules that are common to all file located in the current folder (for all root Solutions).
- Solution-specific configuration file - A file that contains all the rules that are common to all projects of a particular Solution, as well as rules for each project separately.
- Issues (Detected issues) - Represent a set of errors (error) and warnings (warning) detected by the extension.
- Rules (Restrictions) - Parameter values specified in the configuration files and entailing the need for the solution or a specific project to match any value (presence/absence of a reference, version of targetFramework).
- Outgoing project - A project from which the reference between projects "leaves". It is the "beneficiary" of using this reference, as it gives access to the content of the incoming project. It is the parameters of this project that specify the name of the incoming project to indicate this reference.
- Incoming project - A project that "includes" a reference between projects. 

# Extension Features
## Table of Contents
[0. Ways to interact with extension](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#0-%D1%81%D0%BF%D0%BE%D1%81%D0%BE%D0%B1%D1%8B-%D0%B2%D0%B7%D0%B0%D0%B8%D0%BC%D0%BE%D0%B4%D0%B5%D0%B9%D1%81%D1%82%D0%B2%D0%B8%D1%8F-%D1%81-%D1%80%D0%B0%D1%81%D1%88%D0%B8%D1%80%D0%B5%D0%BD%D0%B8%D0%B5%D0%BC)</br>

[1. Extension activation](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#1-%D0%B0%D0%BA%D1%82%D0%B8%D0%B2%D0%B0%D1%86%D0%B8%D1%8F-%D1%80%D0%B0%D1%81%D1%88%D0%B8%D1%80%D0%B5%D0%BD%D0%B8%D1%8F)</br>

[2. Working with extension at the background](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#2-%D1%80%D0%B0%D0%B1%D0%BE%D1%82%D0%B0-%D1%80%D0%B0%D1%81%D1%88%D0%B8%D1%80%D0%B5%D0%BD%D0%B8%D1%8F-%D0%B2-%D1%84%D0%BE%D0%BD%D0%BE%D0%B2%D0%BE%D0%BC-%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B5)</br>
- [a. Possible tool errors and warnings](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#%D0%B0-%D0%B2%D0%BE%D0%B7%D0%BC%D0%BE%D0%B6%D0%BD%D1%8B%D0%B5-%D0%BE%D1%88%D0%B8%D0%B1%D0%BA%D0%B8-%D0%B8-%D0%BF%D1%80%D0%B5%D0%B4%D1%83%D0%BF%D1%80%D0%B5%D0%B6%D0%B4%D0%B5%D0%BD%D0%B8%D1%8F-%D1%80%D0%B0%D1%81%D1%88%D0%B8%D1%80%D0%B5%D0%BD%D0%B8%D1%8F)</br>

[3. Working with configuration files](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#3-%D1%80%D0%B0%D0%B1%D0%BE%D1%82%D0%B0-%D1%81-%D0%BA%D0%BE%D0%BD%D1%84%D0%B8%D0%B3%D1%83%D1%80%D0%B0%D1%86%D0%B8%D0%BE%D0%BD%D0%BD%D1%8B%D0%BC%D0%B8-%D1%84%D0%B0%D0%B9%D0%BB%D0%B0%D0%BC%D0%B8)</br>
- [a. "global_config_guard.rdg" file template and features of the  "framework_max_version" parameter](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#%D0%B0-%D1%88%D0%B0%D0%B1%D0%BB%D0%BE%D0%BD-%D1%84%D0%B0%D0%B9%D0%BB%D0%B0-global_config_guardrdg-%D0%B8-%D0%BE%D1%81%D0%BE%D0%B1%D0%B5%D0%BD%D0%BD%D0%BE%D1%81%D1%82%D0%B8-%D0%BF%D0%B0%D1%80%D0%B0%D0%BC%D0%B5%D1%82%D1%80%D0%B0-framework_max_version)</br>
- [b. "{ Solution_Name}_config_guard.rdg" file sample](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#%D0%B1-%D1%88%D0%B0%D0%B1%D0%BB%D0%BE%D0%BD-%D1%84%D0%B0%D0%B9%D0%BB%D0%B0-%D0%BD%D0%B0%D0%B7%D0%B2%D0%B0%D0%BD%D0%B8%D0%B5-solution_config_guardrdg)</br>
- [c. Different rule levels and their priority](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#%D0%B2-%D1%80%D0%B0%D0%B7%D0%BB%D0%B8%D1%87%D0%BD%D1%8B%D0%B5-%D1%83%D1%80%D0%BE%D0%B2%D0%BD%D0%B8-%D0%BF%D1%80%D0%B0%D0%B2%D0%B8%D0%BB-%D0%B8-%D0%B8%D1%85-%D0%BF%D1%80%D0%B8%D0%BE%D1%80%D0%B8%D1%82%D0%B8%D0%B7%D0%B0%D1%86%D0%B8%D1%8F)</br>
- [d. How  a reference between project works](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#%D0%B3-%D0%BA%D0%B0%D0%BA-%D1%80%D0%B0%D0%B1%D0%BE%D1%82%D0%B0%D0%B5%D1%82-%D1%80%D0%B5%D1%84%D0%B5%D1%80%D0%B5%D0%BD%D1%81-%D0%BC%D0%B5%D0%B6%D0%B4%D1%83-%D0%BF%D1%80%D0%BE%D0%B5%D0%BA%D1%82%D0%B0%D0%BC%D0%B8)</br>
- [e. An extension behavior when 1 or 2 config-fles are not found](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#%D0%B4-%D0%BF%D0%BE%D0%B2%D0%B5%D0%B4%D0%B5%D0%BD%D0%B8%D0%B5-%D1%80%D0%B0%D1%81%D1%88%D0%B8%D1%80%D0%B5%D0%BD%D0%B8%D1%8F-%D0%BF%D1%80%D0%B8-%D0%BD%D0%B5%D0%BE%D0%B1%D0%BD%D0%B0%D1%80%D1%83%D0%B6%D0%B5%D0%BD%D0%B8%D0%B8-%D0%BE%D0%B4%D0%BD%D0%BE%D0%B3%D0%BE-%D0%B8%D0%BB%D0%B8-%D0%B4%D0%B2%D1%83%D1%85-%D0%BA%D0%BE%D0%BD%D1%84%D0%B8%D0%B3%D1%83%D1%80%D0%B0%D1%86%D0%B8%D0%BE%D0%BD%D0%BD%D1%8B%D1%85-%D1%84%D0%B0%D0%B9%D0%BB%D0%BE%D0%B2)</br>
- [f. An extension behavior when incorrect values are found inside config-file](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#%D0%B5-%D0%BF%D0%BE%D0%B2%D0%B5%D0%B4%D0%B5%D0%BD%D0%B8%D0%B5-%D1%80%D0%B0%D1%81%D1%88%D0%B8%D1%80%D0%B5%D0%BD%D0%B8%D1%8F-%D0%BF%D1%80%D0%B8-%D0%BE%D0%B1%D0%BD%D0%B0%D1%80%D1%83%D0%B6%D0%B5%D0%BD%D0%B8%D0%B8-%D0%BD%D0%B5%D0%BA%D0%BE%D1%80%D1%80%D0%B5%D0%BA%D1%82%D0%BD%D1%8B%D1%85-%D0%B7%D0%BD%D0%B0%D1%87%D0%B5%D0%BD%D0%B8%D0%B9-%D0%B2-%D1%84%D0%B0%D0%B9%D0%BB%D0%B5)</br>
- [g. An extension behavior when an incongruity between solution state projects and config-file projects is found](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#%D0%B6-%D0%BF%D0%BE%D0%B2%D0%B5%D0%B4%D0%B5%D0%BD%D0%B8%D0%B5-%D1%80%D0%B0%D1%81%D1%88%D0%B8%D1%80%D0%B5%D0%BD%D0%B8%D1%8F-%D0%BF%D1%80%D0%B8-%D0%BE%D0%B1%D0%BD%D0%B0%D1%80%D1%83%D0%B6%D0%B5%D0%BD%D0%B8%D0%B8-%D0%BD%D0%B5%D1%81%D0%BE%D0%BE%D1%82%D0%B2%D0%B5%D1%82%D1%81%D1%82%D0%B2%D0%B8%D1%8F-%D0%B2-%D0%BF%D1%80%D0%BE%D0%B5%D0%BA%D1%82%D0%B0%D1%85-%D0%BC%D0%B5%D0%B6%D0%B4%D1%83-%D1%84%D0%B0%D0%B9%D0%BB%D0%BE%D0%BC-%D0%BA%D0%BE%D0%BD%D1%84%D0%B8%D0%B3%D1%83%D1%80%D0%B0%D1%86%D0%B8%D0%B8-%D0%B8-%D1%84%D0%B0%D0%BA%D1%82%D0%B8%D1%87%D0%B5%D1%81%D0%BA%D0%B8%D0%BC-%D1%81%D0%BE%D1%81%D1%82%D0%BE%D1%8F%D0%BD%D0%B8%D0%B5%D0%BC-%D1%80%D0%B5%D1%88%D0%B5%D0%BD%D0%B8%D1%8F)</br>
- [h. An extension behavior when a project that is declared in reference name is not found](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#%D0%B7-%D0%BF%D0%BE%D0%B2%D0%B5%D0%B4%D0%B5%D0%BD%D0%B8%D0%B5-%D1%80%D0%B0%D1%81%D1%88%D0%B8%D1%80%D0%B5%D0%BD%D0%B8%D1%8F-%D0%BF%D1%80%D0%B8-%D0%BD%D0%B5%D0%BE%D0%B1%D0%BD%D0%B0%D1%80%D1%83%D0%B6%D0%B5%D0%BD%D0%B8%D0%B8-%D0%B2-%D1%80%D0%B5%D1%88%D0%B5%D0%BD%D0%B8%D0%B8-%D0%BF%D1%80%D0%BE%D0%B5%D0%BA%D1%82%D0%B0-%D0%B7%D0%B0%D1%8F%D0%B2%D0%BB%D0%B5%D0%BD%D0%BD%D0%BE%D0%B3%D0%BE-%D0%B2-%D1%81%D0%B2%D1%8F%D0%B7%D0%B8)</br>
- [i. An extension behavior when an unexsisting TFM is found inside "framework_max_version" parameter](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#%D0%B8-%D0%BF%D0%BE%D0%B2%D0%B5%D0%B4%D0%B5%D0%BD%D0%B8%D0%B5-%D1%80%D0%B0%D1%81%D1%88%D0%B8%D1%80%D0%B5%D0%BD%D0%B8%D1%8F-%D0%BF%D1%80%D0%B8-%D0%BE%D0%B1%D0%BD%D0%B0%D1%80%D1%83%D0%B6%D0%B5%D0%BD%D0%B8%D0%B8-%D0%B2-framework_max_version-%D0%BD%D0%B5%D1%81%D1%83%D1%89%D0%B5%D1%81%D1%82%D0%B2%D1%83%D1%8E%D1%89%D0%B5%D0%B9-tfm)</br>
- [g. Ways to create config-file samples](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#%D0%BB-%D1%81%D0%BF%D0%BE%D1%81%D0%BE%D0%B1%D1%8B-%D1%81%D0%BE%D0%B7%D0%B4%D0%B0%D0%BD%D0%B8%D1%8F-%D1%84%D0%B0%D0%B9%D0%BB%D0%BE%D0%B2-%D0%BA%D0%BE%D0%BD%D1%84%D0%B8%D0%B3%D1%83%D1%80%D0%B0%D1%86%D0%B8%D0%B8)</br>

[4. Show all current references between projects](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#4-%D0%B2%D1%8B%D0%B2%D0%BE%D0%B4-%D0%B2%D1%81%D0%B5%D1%85-%D1%82%D0%B5%D0%BA%D1%83%D1%89%D0%B8%D1%85-%D1%80%D0%B5%D1%84%D0%B5%D1%80%D0%B5%D0%BD%D1%81%D0%BE%D0%B2-%D0%BF%D1%80%D0%BE%D0%B5%D0%BA%D1%82%D0%B0)</br>
- [a. Show all straight references](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#%D0%B0-%D0%B2%D1%8B%D0%B2%D0%BE%D0%B4-%D0%BF%D1%80%D1%8F%D0%BC%D1%8B%D1%85-%D1%80%D0%B5%D1%84%D0%B5%D1%80%D0%B5%D0%BD%D1%81%D0%BE%D0%B2)</br>
- [b. Show all transitive references](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#%D0%B1-%D0%B2%D1%8B%D0%B2%D0%BE%D0%B4-%D1%82%D1%80%D0%B0%D0%BD%D0%B7%D0%B8%D1%82%D0%B8%D0%B2%D0%BD%D1%8B%D1%85-%D1%80%D0%B5%D1%84%D0%B5%D1%80%D0%B5%D0%BD%D1%81%D0%BE%D0%B2)</br>
[5. Show changes between project references since last state commit](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#5-%D0%B2%D1%8B%D0%B2%D0%BE%D0%B4-%D0%B8%D0%B7%D0%BC%D0%B5%D0%BD%D0%B5%D0%BD%D0%B8%D0%B9-%D1%80%D0%B5%D1%84%D0%B5%D1%80%D0%B5%D0%BD%D1%81%D0%BE%D0%B2-%D1%81-%D0%BC%D0%BE%D0%BC%D0%B5%D0%BD%D1%82%D0%B0-%D0%B8%D1%85-%D0%BF%D0%BE%D1%81%D0%BB%D0%B5%D0%B4%D0%BD%D0%B5%D0%B9-%D1%84%D0%B8%D0%BA%D1%81%D0%B0%D1%86%D0%B8%D0%B8)</br>

[6. Force solution state commit](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#6-%D0%BF%D1%80%D0%B8%D0%BD%D1%83%D0%B4%D0%B8%D1%82%D0%B5%D0%BB%D1%8C%D0%BD%D0%B0%D1%8F-%D1%84%D0%B8%D0%BA%D1%81%D0%B0%D1%86%D0%B8%D1%8F-%D1%80%D0%B5%D1%84%D0%B5%D1%80%D0%B5%D0%BD%D1%81%D0%BE%D0%B2)</br>

[7. Export to XLSX](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#7-%D1%82%D0%B0%D0%B1%D0%BB%D0%B8%D1%87%D0%BD%D1%8B%D0%B9-%D1%8D%D0%BA%D1%81%D0%BF%D0%BE%D1%80%D1%82-%D1%81%D0%BE%D1%81%D1%82%D0%BE%D1%8F%D0%BD%D0%B8%D1%8F-%D0%BF%D1%80%D0%BE%D0%B5%D0%BA%D1%82%D0%B0)</br>
- [a. Project sample list](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#%D0%B0-%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B8%D1%86%D0%B0-%D0%B2%D1%8B%D0%B1%D0%BE%D1%80%D0%BA%D0%B8-%D0%BF%D0%BE-%D0%BF%D1%80%D0%BE%D0%B5%D0%BA%D1%82%D0%B0%D0%BC)</br>
- [b. Reference sample list](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#%D0%B1-%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B8%D1%86%D0%B0-%D0%B2%D1%8B%D0%B1%D0%BE%D1%80%D0%BA%D0%B8-%D0%BF%D0%BE-%D1%80%D0%B5%D1%84%D0%B5%D1%80%D0%B5%D0%BD%D1%81%D0%B0%D0%BC)</br>
- [c. RefDepGuard errors list](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#%D0%B2-%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B8%D1%86%D0%B0-%D1%82%D0%B5%D0%BA%D1%83%D1%89%D0%B8%D1%85-%D0%BE%D1%88%D0%B8%D0%B1%D0%BE%D0%BA-refdepguard)</br>
- [d. RefDepGuard warnings list](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#%D0%B3-%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B8%D1%86%D0%B0-%D1%82%D0%B5%D0%BA%D1%83%D1%89%D0%B8%D1%85-%D0%BF%D1%80%D0%B5%D0%B4%D1%83%D0%BF%D1%80%D0%B5%D0%B6%D0%B4%D0%B5%D0%BD%D0%B8%D0%B9-refdepguard)</br>

[8. Export to HTML (Dependency graph)](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#8-%D0%B3%D1%80%D0%B0%D1%84%D0%B8%D1%87%D0%B5%D1%81%D0%BA%D0%B8%D0%B9-%D1%8D%D0%BA%D1%81%D0%BF%D0%BE%D1%80%D1%82-%D1%81%D0%BE%D1%81%D1%82%D0%BE%D1%8F%D0%BD%D0%B8%D1%8F-%D0%BF%D1%80%D0%BE%D0%B5%D0%BA%D1%82%D0%B0)</br>
- [a. Prioritization of special designations on links drawing](https://github.com/Roman194/RefDepGuard/blob/master/USER_GUIDE.md#%D0%B0-%D0%BF%D1%80%D0%B8%D0%BE%D1%80%D0%B8%D1%82%D0%B8%D0%B7%D0%B0%D1%86%D0%B8%D1%8F-%D0%BE%D1%81%D0%BE%D0%B1%D1%8B%D1%85-%D0%BE%D0%B1%D0%BE%D0%B7%D0%BD%D0%B0%D1%87%D0%B5%D0%BD%D0%B8%D0%B9--%D0%BF%D1%80%D0%B8-%D1%80%D0%B8%D1%81%D0%BE%D0%B2%D0%B0%D0%BD%D0%B8%D0%B8-%D1%81%D0%B2%D1%8F%D0%B7%D0%B5%D0%B9)

## 0. Ways to interact with the extension
After installation, the extension supports 2 ways of interaction:
- Via the Extensions menu

To do this, select the "Extensions" section on the top toolbar </br>

<img width="1074" height="122" alt="Items" src="https://github.com/user-attachments/assets/72cdd957-2403-4d2b-ad31-665e6eb29143" />
    
Among the extensions, select the "RefDepGuard" extension.
The full functionality of the extension is described in the main points of this document and is shown in the figure below:< /br>
<img width="732" height="214" alt="image" src="https://github.com/user-attachments/assets/fa78db42-4080-4a2c-bec0-f485e900d50d" />

To perform the required functionality, click on the corresponding button

- Using hotkeys
As you can see from the picture above, all the functionality of the extension is duplicated by "hot keys".</br>
In other words, to start the required function, just press the appropriate key combination.</br>
</br>
Currently, the extension implements the following hotkeys:</br>
**ALT + R** - Displays straight references currently contained in the solution (see point 4A)</br>
**ALT + T** - Displays transitive references currently contained in the solution (see point 4B)</br>
**ALT + E** - Displays changes to references that have occurred since the last commit of the project state (see point 5)</br>
**ALT + C** - Commit the current version of the solution (see point 6)</br>
**ALT + X** - Export the current state of the solution in a tabular report format (see point 7)</br>
**ALT + H** - Export the current state of the solution in a graphical report format (see point 8)</br>
</br>
_!Important: After downloading / opening the solution, the extension takes some time (about 10 seconds) to perform a preliminary check in the background (see point 2). Before performing this action, the extension is considered not loaded yet and cannot be interacted with directly: when you try to interact with it, the following message will be displayed:!_ </br>

<img width="475" height="185" alt="image" src="https://github.com/user-attachments/assets/28d5f7e4-10e7-4e40-97e5-6269d0229c75" />

_Extension **will be considered loaded** when either a list of detected "problems" is displayed, or a warning about the lack of references in the solution, or a message that the extension did not detect "problems" (see point 2)!_

## 1. Activating the extension
When the solution is loaded for the first time, the extension displays a message where you can choose whether to use the extension in this solution or not. If you agree to use it, the extension starts working in the background (see point 2), and all its functionality becomes available via hotkeys and the main menu of the extension (see point 0).</br>

<img width="472" height="223" alt="image" src="https://github.com/user-attachments/assets/f44bdbb9-49c5-4016-aecb-dff75e38ff62" /></br>

Otherwise, the extension remains inactive, but the user can activate the extension using a special button that appears instead of the main extension menu.:</br>

<img width="647" height="140" alt="image" src="https://github.com/user-attachments/assets/bc290738-e2f2-44da-ac10-7258baabdf93" />

_!Important: at the moment, the **extension cannot be deactivated**, so the decision to activate it on a specific solution should be considered!_

## 2. Running the extension in the background
The extension works not only at the user's command, but also in the background.
In the background, it captures the current state of links between projects and versions TargetFramework of the project's targetFramework, as well as checks whether these parameters meet the constraints specified in the **configuration file**. 
The extension works in the background after the initial loading/opening of the solution and during the build of the project (solution). If the current project parameters and relationships between them are found to be inconsistent, the extension displays the VS corresponding extension errors and warnings in the VS 22 "error list".
If errors are detected during the build, the **extension cancels the build** and does not allow it to complete successfully until all errors found by the extension are resolved.

When running in the background the extension also loads rules from configuration files. </br>

<img width="1280" height="285" alt="image" src="https://github.com/user-attachments/assets/94efbe55-8a9c-49cc-ba82-638bac91d6bd" />

The list of Errors and Warnings that the extension can issue is given below.
The extension's errors and warnings are described using the following template:
 ````JSON
{extension_name} {error_name_or_warnings} [error/warning]: {error_description_or_warnings}. {suggested_action_for_problem_resolution}
````
If no problems were detected, the corresponding message will be displayed when the compliance check is completed:</br>

<img width="730" height="151" alt="image" src="https://github.com/user-attachments/assets/61147c52-d160-42cd-bb98-83c088c7aac3" />


A separate case is highlighted when the extension failed to detect any references between projects in the solution. It is considered a potentially erroneous action (considering that the extension is supposed to be used in solutions where there are many references between projects), so it is marked as an error. If no references are found, the configuration file rules **are not checked** and this warning is displayed in the error panel as the only notification from the extension.:</br>

<img width="1163" height="128" alt="image" src="https://github.com/user-attachments/assets/6bd53807-42a6-43a1-a2ca-f6e5a7a93b25" />

Also, if there is such a valid warning, any export is prohibited:</br>

<img width="539" height="201" alt="image" src="https://github.com/user-attachments/assets/2e7c9f3f-dc20-4367-9508-fc404d620aa1" />

_!Important: The reason that the extension doesn't detect references after loading solution may be because the solution still wasn't fully loaded at the time of checking them. If such a warning occurs after loading the solution, try to force fixing the solution state (point 6)!_

### a) Possible extension errors and warnings

***Errors:***

**Reference error** - Occurs when a reference is detected that was not allowed to be created within the configuration file, or when a reference that must necessarily exist on demand from the configuration file is not detected.</br>
**Match error** - Occurs when a contradiction in the reference rule is detected at the same level (see point 3b for levels): the reference is both declared mandatory and invalid.</br>
**Null property error** - Occurs when one or more of the properties required by the configuration file template were not found in the configuration file (see point 3).</br>
**Framework version comparability error** - Occurs when the current targetFramework version of a project exceeds the maximum allowed version for this project (limit within the "max_framework_version" parameter of the configuration file, see point 3).</br>
**framework_max_version deviant value error** - Occurs when a max_framework_version invalid value is found in the value of the "max_framework_version" parameter, including when a Project template for multiple types of constraints is specified at the Project level.</br>
**framework_max_version template illegal usage error** - Occurs when the value of the "max_framework_version" parameter detects incorrect use of the template for setting various restrictions for different project types. The error may be caused by using a template at the project level in a project that has only one TFM type, or by trying to set a constraint for a TFM that is not relevant for a project with several different TFMs.</br>

***Warnings:***

**warning** - An unnamed warning occurs if an incorrect targetFramework property value is detected TargetFramework (according to the extension) in the project's. csproj. **If the extension works correctly, it should not occur**.</br>
**Reference Match Warning** - Occurs when a reference rule is found to be duplicated or contradicted at different levels (i.e., when the rule of "interrupting" the logic of local rules with global ones is applied, see point 3b).</br>
**framework_max_version conflict warning** - Occurs when the value of the more global "framework_max_version" parameter in the config file max_version has a lower value than the more local one; or when at one level the value of the all supertype is greater than the value of one of the types.</br>
**framework_max_version reference conflict warning** - Occurs when a reference is detected between projects, among which the referring project has a lower "framework_max_version" value than the one it refers to (potential "smaller to larger" references are undesirable behavior that can lead to problems in the future).
This type of warning also includes problems when the current version of a "netstandard" reference is incompatible with the current version of the project (according to its TFM).</br>
**framework_max_version TFM not found warning** - Occurs when the template for setting different constraints for different project types (see point 3A) sets a TFM that does not match any of the existing ones according to [official documentation](https://learn.microsoft.com/en-us/dotnet/standard/frameworks://learn.microsoft.com/en-us/dotnet/standard/frameworks). This may be caused by a typo or the fact that TFM belongs to the deprecated category (according to the official documentation).</br>
**framework_max_version deviant value warning** - Occurs when the value of the parameter "max_framework_version" is found to be inconsistent with the format "x.0", where x is any number.</br>
**framework_max_version illegal template usage warning** - Occurs when the template for various restrictions at the Solution/Global levels sets an existing TFM that is not relevant for any project of the current solution.</br>
**Project not found warning** - Occurs when a required value that does not correspond to any of the names of existing solution projects is found in the connection between projects (based on the results of the required references and / or unacceptable references configuration files at any of the levels).</br>
**Project Match Warning** - Occurs when a project is found in the solution that is not in the config file of this Solution, or when the project is in the config file, but not in the solution itself. It is displayed to the user only if they refuse to update the config file when initializing the solution, forcibly fixing references, or when starting the solution build.</br>
**Project name semantic warning** - Output if the solution contains projects that contain dots (" .") in the name, and there are at least 2 projects that have the same fragment in the name, separated by dots, and the third project that has a similar fragment, but different occurrence of characters in the name (they differ by 1-2 characters if the fragment length between points is 3 or more characters). Requires the appropriate permission at the Solution and Global levels.</br>
**Transit references warning** - Displayed if the solution detects transitive links between projects and the configuration file has permission to display transitive links at all 3 levels (see point 3).</br>
**Transit references duplicate warning** - Occurs if the solution detects transitive links between projects that duplicate existing direct links. It also requires permission on all 3 levels.</br>

## 3. Working with configuration files
Configuration files consist of 2 files with the .rdg extension, which record all the rules that projects and references between them must meet in the Solution under consideration.
For the extension to read correctly, both files must be located in the root directory of Solution (in the same place as the corresponding .sln file).
Their structure consists of JSON files with values written next to the parameters, which will be used to compare these parameters with the actual state of affairs in the loaded Solution.

The names of these files are:
````JSON
"global_config_guard.rdg" and "{Solution_name}_config_guard.rdg"
````
**global_config_guard.rdg** is a global configuration file that sets rules that are common to multiple Solutions. 
Although VS22 **does not support** simultaneous loading of multiple Solutions in the same window, it is still possible to place 2 or more Solutions in the same folder and work with them separately, sharing common files. In this case, the restrictions specified in the global configuration file apply to all Solutions located in the shared folder.
Example of such usage:</br>
<img width="704" height="233" alt="image" src="https://github.com/user-attachments/assets/4d29ab9e-df2c-4471-9782-b055b957572f" />

The figure shows files that represent different Solutions.

**{Solution_Name}_config_guard.rdg** is a configuration file for a specific Solution. It contains all the rules related to the Solution that is specified in its name.

### a) File template "global_config_guard.rdg" and features of the "framework_max_version" parameter:
````JSON
{
	"name":"Global",
	"framework_max_version":"-",
	"report_on_transit_references": false,
    "project_names_semantic_check": false,
	"global_required_references":[
              "Mir.Controller.Model"
        ],
	"global_unacceptable_references":[
               "Mir.Controller.Cfg.Meters.CntCfg"
        ]
}
````
Parameters for this file:

**name** - File name. "Global", if the global rules file is considered, and "{Solution_Name}", if the configuration file of a particular Solution is considered.

**"framework_max_version"** - Contains information about the maximum allowed version for the **targetFramework** parameter or its equivalent (TargetFrameworkVersion / TargetFrameworks). This restriction must be entered manually. It is assumed that it will be used to prevent the project version from being upgraded to an undesirable value due to the loss of support for another project or a third-party library.
The limit is set in the following format:
````JSON
"4.2.7"
````
, where the dot (".") is the separator between digits.
There is no limit to the number of digits in the constraint, but it doesn't make sense to specify more than 3 digits in most cases. 
If there is no restriction, then enter a dash ("-").

_!Important: if you migrate a project from one type of framework to another (for example, from .netnet framework to .netnet core), the "framework_max_version" parameter will have an incorrect value, which may lead to false errors and warnings. Don't forget to change the version of this parameter when migrating your project!_

You can also set **different restrictions for different types of projects**. For this purpose, a special template (format) is used:

```JSON
"netstandard: 2.1; net: 7.5"
```
, where a semicolon (";") is a separator between individual constraints
a colon (":") is a separator between the targetFramework type name and the maximum allowed version within the rule.
Spaces are insignificant.

_!Important: specifying the same max_framework_version constraint type more than once within a single row will result in **framework_max_version deviant value error**!_

The template also supports **specifying a single constraint**: to do this, specify this constraint without a semicolon at the end:
```JSON
"netstandard: 2.1"
```

_!Important: This template can only be used at the Project level only for projects (see point 3B for levels) that have "TargetFrameworks" parameter in .csproj, which includes [VARIOUS types of the projects](https://learn.microsoft.com/en-us/dotnet/standard/frameworks#how-to-specify-a-target-framework://learn.microsoft.com/en-us/dotnet/standard/frameworks#how-to-specify-a-target-framework) (Let's say "net" and "netstandard"). An attempt to use it at this level under all other conditions (including in MAUI projects) will result in **framework_max_version template illegal usage error**!_

_!Important: **It is recommended to set an existing version of netstandard**. Otherwise, it will be moved by the extension to the nearest existing one. The above version can be displayed on received warnings and in generated exports!_

This template also supports the **supertype all**, which indicates a restriction **on all** project types. In fact, all is equivalent to the standard format for setting a constraint (it's just implicitly set in the standard version), but it also allows you to set a more strict constraint for specific project types:
```JSON
"all: 3.0; net: 2.5"
```
_!Important: A more strict constraint means a constraint that has a smaller version than the constraint version specified in the all supertype. If a particular constraint has a version greater than the one specified in all, it is considered a contradiction and results in a **framework_max_version conflict warning**!_

There are a number of features in using the **.net framework** type:

Since this type in earlier versions has a targetFramework record of the type "v3.5", and in later versions-of the type "net45", in order to avoid conflicts with projects of the type .NET decided to use it as a project type name within the extension .net framework "**netf**" for all its versions.
To set a limit for this project type, you must specify the following entry type:
```JSON
"netf: 3.0; net: 7.5"
```
_!Important: for the extension to work correctly, you must specify the "netf" version value in the configuration file separated by a dot!_

**report_on_transit_references** - Specifies whether to display warnings about detected transitive links (**Transit references warning** / **Transit references duplicate warning**). Has false as the default value. 

_!Important: to display warnings about transitive relationships of a particular project, the value true must be set at all 3 levels: global, solution, and the level of this project.
In general, the logic for prioritizing values **for this parameter** is as follows: Global > Solution > Project!_

**project_names_semantic_check** - Specifies whether to check project names for rules and deviations from them (**Project name semantic warning**). Has false as the default value. For semantics, the prioritization logic is similar to the output logic of transitive warnings, but without the Project level.

**global_required_references** - Contains information about the required references at the global level: it is assumed that references between projects listed in the value of this parameter should be in all projects in this folder (except for those that have disabled checking the Global and Solution level reference rules, see subclause b). If the constraint is not met, a **Reference error** occurs.

**global_unacceptable_references** - Contains information about references that are prohibited at the global level for all projects in the folder under consideration (except for those that have disabled checking the Global and Solution level reference rules, see subclause b). If the constraint is not met, a **Reference error** occurs.

The restriction is set for required and invalid references in the following format:
```JSON
"Mir.Controller.Main",
"Mir.Controller.Tests.Cfg"
```
, where a comma (",") is the separator between the names of references

### b) File template "{Solution_Name}_config_guard.rdg"
````JSON
{
  "name": "WinFormApp",
  "framework_max_version": "netstandard: 2.0.0; net: 8.0",
  "report_on_transit_references": false,
  "project_names_semantic_check": false,
  "solution_required_references":[
    "Mir.Controller.Main",
    "Mir.Controller.Tests.Cfg"
  ],
  "solution_unacceptable_references":[

  ],
  "projects": {
    "Mir.Controller.Model":{
      "framework_max_version": "3.7.0",
      "report_on_transit_references": false,
      "consider_global_and_solution_references": {
          "required": true,
          "unacceptable": true
      }
      "required_references":[
         "Mir.Controller.Tests",
         "Mir.Controller.Project1"
      ],
      "unacceptable_references":[
        "Mir.Controller.WinProject"
      ]
    },
    "Mir.Controller.ARP56":{
      "framework_max_version": "4.7.2",
      "report_on_transit_references": false,
      "consider_global_and_solution_references": {
          "required": true,
          "unnacceptable": true
      }
      "required_references":[
         "Mir.Controller.Tests",
         "Mir.Controller.Project1"
      ],
      "unacceptable_references":[
         "Mir.Controller.WinProject"
      ]
    }
  }
}

````
Parameters of this file (those that are not discussed above in the global configuration file template):

**solution_required_references_required** - Similar to global, it contains information about required references, but only at the level of a specific solution.

**solution_unacceptable_references** - Similar to global, it contains information about invalid references, but only at the level of a specific solution.

**projects** - Represents the set of all projects of a particular solution projects. It contains objects of the same template that represent specific projects and project-level constraints. Each object has its own name (for example: "Mir.Controller.Model", "Mir.Controller.ARP56").

**consider_global_and_solution_references** - An object that includes 2 parameters: "required "and "unacceptable". These parameters determine whether the current project has restrictions on Global and Solution level references or not. The first one is responsible for the rules for mandatory references, and the second one is responsible for the rules for invalid references. Possible values for these parameters are true or false.

_!Important: if this parameter is not detected by the extension, then, unlike the others, it will not cause **Null property error**! Instead, the object and its 2 parameters will be counted as false (the same behavior occurs with the parameters "report_on_transit_references" and "project_names_semantic_check")!_

**required_references** - Similar to global, it contains information about required references, but only at the project-specific level.

**unacceptable_references** - Similar to global, it contains information about invalid references, but only at the project-specific level.

### c) Different levels of rules and their prioritization
The parameters "framework_max_version", "required_references", and "unacceptable_references" are set at different rule levels.
There are a total of 3 rule levels in the extension:
- **Global** - global-level rules (apply to all projects in the current folder)
- **Solution** - rules of the current solution level (apply to all projects of the current Solution)
- **Project** - rules of the current project (refer to a specific project)

**Prioritizing rules**:
Project-level rule > Solution-level Rule > Global Level Rule

_! Exception: the parameters "report_on_transit_references" and "project_names_semantic_check" use the reverse prioritization logic (see point 3A)!_

The rule prioritization logic is triggered if a conflict between the rules is detected. This is how the extension decides which rule is more important and should be used. The user is also shown a corresponding warning about detecting a conflict: **framework_max_version conflict warning** or **Match warning**, respectively.

If **conflicting rules have the same level**, both rules are ignored and the corresponding warning/error is displayed: **framework_max_version conflict warning** or **Match error**, respectively.

**Behavior in the absence of a rule**:
Rules for each project start to be read by priority level. Thus, the Project-level rule is analyzed first Project, then the Solution-level rule, and finally the Global-level rule.

If framework_max_version missing rule (" - ") is detected for "framework_max_version" **at the first level**, the Solution level rule will be checked. If there is also no rule there, then the check will be performed at the Global level. If an error is found at any level during the rule search, **framework_max_version deviant value error** is output and the search stops.

However, **framework_max_version reference conflict warning** can also be output if the referring project does not have a rule, when the new famework_max_version constraint of the project it refers to can potentially lead to a "smaller to larger" link (see below), or it may have a "netstandard" version that is incompatible for the referring netstandard-project.

For **"required_references"** and **"unacceptable_references"**, the logic is somewhat different: rules by priority level are "layered" on top of each other and, accordingly, rules of all levels are added in turn, regardless of whether the absence of rules was detected above. If there is a duplication or conflict of rules, the lower-level rule is ignored and the corresponding error/warning is displayed.

### d) How the reference between projects works
The easiest way to show how it works is to use an example of a fragment of a graphical report (see point 8 for a graphical report):</br>
<img width="405" height="121" alt="image" src="https://github.com/user-attachments/assets/8823cba0-7c98-457b-98ec-1140c6beb4a0" />

As you can see, the figure shows the reference "WinFormsApp1" - > "WinFormApp".
If this reference is mandatory, then within the settings of configuration files, this reference is written as follows:
```JSON
"WinFormApp":{
      "framework_max_version": "-",
      "report_on_transit_references": false,
      "consider_global_and_solution_references": {
          "required": true,
          "unacceptable": true
      }
      "required_references":[
         "WinFormApp1"
      ],
      "unacceptable_references":[
      ]
    },
```
Thus, **in the parameters** "required_references"," unacceptable_references", etc., **the name of the project that the** reference refers to (required or invalid) is written. And, in the case of a Project-level constraint, this constraint is specified in the project whose name matches the name of the project **from which * * the reference refers.
The project that the reference refers to is considered outgoing, and the one that it refers to is considered incoming.

**Ban on references "from less to more"**:
VS22 implements a ban on references between projects, which means that the targetFramework of the outgoing project **cannot be less than the** targetFramework of the incoming project.
This is because newer versions of projects have support for older versions, but older versions don't know anything about the functionality of the newer ones. Since the reference between projects assumes that the outgoing project gets access to the functionality of the incoming one, it must support all the functionality of this project and, accordingly, have a version of targetFramework higher than the incoming one or equal to it.
Since this check is implemented using VS22 tools, it is not implemented in the extension.

However, since the extension contains a limit on max. If the value is targetFramework, then it makes sense to check for **potential version conflicts in targetFramework**. 
So the limit on the max version of targetFramework parameter of the outgoing project **must be greater than or equal to** the same limit for the incoming project. If this condition is not met, a corresponding warning is generated (**framework_max_version reference conflict warning**).

_!In the current version of the extension, a restriction is considered when a reference is found between projects, among which the referring project has a lower "framework_max_version" value than the one it refers to, and when the current version of the "netstandard" ref is incompatible with the current version of the project (according to its TFM)!_

### e) Behavior of the extension when one or two configuration files are not detected
If the extension couldn't find the global configuration file and/or the solution-specific configuration file, it displays a warning about it and generates the missing file in the root folder itself.</br>

<img width="572" height="202" alt="image" src="https://github.com/user-attachments/assets/8ffea3d2-7db5-4acb-ab9b-2a71b20e2495" />
</br>
<img width="573" height="213" alt="image" src="https://github.com/user-attachments/assets/b6807eea-4eb3-41fa-8ee5-f040736d5d24" />

The file after generation will look something like this:</br>
<img width="502" height="492" alt="image" src="https://github.com/user-attachments/assets/fbb405dd-e3b9-4cff-a302-a1f501f1e5be" />


### f) Behavior of the extension when invalid values are detected in the file
The extension handles errors made in the configuration file in several different ways (depending on the type of error):
- _Notification of an invalid value and suggestion to solve the problem by transferring the content to a Rollback-file and generating a file_

Occurs when a syntax error occurs that causes the JSON parser to be unable to read the contents of the file.
We are talking about an extra or missing comma, colon, etc.
In this case, the extension notifies the user that the file cannot be read and suggests that the entire content will be transferred to the {value_file_name_rollback.rdg} file (if necessary, creates it in the root folder) and write to the existing file the correct configuration file stored in the cache, or a standard empty template relevant to this file, if the cache is not detected or its parsing failed with an error:

<img width="506" height="235" alt="image" src="https://github.com/user-attachments/assets/0ff23f37-c670-4cd1-9383-fa43c1cee612" />

<img width="510" height="186" alt="image" src="https://github.com/user-attachments/assets/0809db7b-4284-4561-918d-ee9690a2bc35" />

If the user agrees, the actions described above will be performed, and if they don't, a corresponding warning will be displayed about problems with the configuration file:

<img width="945" height="92" alt="image" src="https://github.com/user-attachments/assets/efd004e4-ade8-4f8e-a9d9-3c5c88d911de" />

Root folder after generating rollback-files:

<img width="906" height="452" alt="image" src="https://github.com/user-attachments/assets/9d401ec5-6c63-4ef8-8503-b95f33fbd51c" />


- _The output of the corresponding error about the inability to read a particular parameter in the configuration file_

Occurs when an error is made in the parameter value itself, or when one of the parameters is not found at all.
These are errors such as **Null property error** and **framework_max_version deviant value error**. These errors are displayed in the VS 22 "error panel" along with the rest:</br>

<img width="1280" height="208" alt="image" src="https://github.com/user-attachments/assets/c5d2517e-0406-4cd3-98c3-cfff31fe1d12" />

### g) Behavior of the extension when a mismatch is detected in projects between the configuration file and the actual state of the solution
If the configuration file contains one or more projects that are not actually present in the solution, or if the file does not contain one or more projects that are present in the solution, the user will be prompted to add / remove these project (s) from the configuration file of a particular solution when fixing the state (forced or background):</br>

<img width="508" height="181" alt="image" src="https://github.com/user-attachments/assets/4b57af7c-a165-43fb-aa9d-b227dc9d2c21" /></br>

<img width="505" height="188" alt="image" src="https://github.com/user-attachments/assets/047b072d-4324-4ce3-acfa-385b39e481fc" />

If the user agrees, these actions will be performed. In case of disagreement, a corresponding warning will be displayed:</br>

<img width="1023" height="107" alt="image" src="https://github.com/user-attachments/assets/dd56290d-cfad-4e06-8bff-7c6d9faa112d" />

If exactly 1 project has been added or deleted, the user will be prompted to rename the project while maintaining all existing rules.</br>

<img width="438" height="173" alt="image" src="https://github.com/user-attachments/assets/03b2480f-bd25-437c-8d19-47fb673967f7" />

In case of refusal, you will be prompted to add / remove these projects from the configuration file of a particular solution, and in case of repeated disagreement, a corresponding warning will be displayed (see above).</br>

<img width="440" height="165" alt="image" src="https://github.com/user-attachments/assets/8584596f-e362-4457-ab50-e74a2410c8ce" /> </br>

<img width="436" height="147" alt="image" src="https://github.com/user-attachments/assets/7598fad2-da03-452a-90e6-8e02f97995db" />


### h) Behavior of the extension when undetected project name in the solution declared inside its reference
If a value specified in required_references and / or unacceptable_reference at any level is found in the solution and does not correspond to any of the existing projects in the solution, the corresponding warning will be displayed:</br>

<img width="968" height="122" alt="image" src="https://github.com/user-attachments/assets/b823d915-60c2-42f9-9b4d-789841b4888e" />

### i) Behavior of the extension when a non-existent TFM is detected in "framework_max_version" TFM 
If a framework_max_version TFM is found in the "framework_max_version" parameter when using a template of various constraints TFM that does not match any of the existing ones according to [official documentation](https://learn.microsoft.com/en-us/dotnet/standard/frameworks://learn.microsoft.com/en-us/dotnet/standard/frameworks), then the following warning will be displayed:</br>

<img width="1014" height="155" alt="image" src="https://github.com/user-attachments/assets/e9fb54d1-01ad-4e23-a9a4-ec229dd627d9" />

### l) Ways to create configuration files
You can create configuration files in one of the following ways:
- Independently follow the instructions and templates from this User Guide
- Using the auto-generation of the template extension when a file is not detected in the root directory of Solution (recommended)

## 4. Output of all current project references
### a) Output of straight references
When you click on the "Show all straight references" button in the menu or press the Alt + R key combination, a message is displayed containing information about all direct references between projects. That is, references that the project refers to without the mediation of any third-party projects. If one of the projects does not have references to other projects, then puts a dash ("-"). </br>

<img width="355" height="809" alt="image" src="https://github.com/user-attachments/assets/6bc9c2bd-b64c-4ffc-b25f-f9d42741eac7" />

If no references are found in Solution for any project, a message about this is displayed. In this case, the corresponding warning shown in step 2 will also be observed.</br>

<img width="414" height="149" alt="image" src="https://github.com/user-attachments/assets/04026563-0df1-411c-895a-76083afd6bbf" />

### b) Output of transitive references

When you click on the "Show all transitive references" button in the menu or press the Alt + T key combination, a message is displayed containing information about all transitive references between projects. That is references that the current project refers to indirectly (through other projects). If any of the projects do not have such references, then puts a dash ("-"). </br>

<img width="305" height="646" alt="image" src="https://github.com/user-attachments/assets/1e633fc7-182f-46fa-848a-3395a56c9ee5" />

If no references are found in Solution for any project, a message is displayed (see point 3A).

## 5. Displaying changes to references since they were last committed
When you click on the "Show changes in refs" button in the menu or press the Alt + E key combination, a message is displayed containing information about all changes in references since the last commit (loading a solution, building a project, or forcing a commit):</br>

<img width="436" height="286" alt="image" src="https://github.com/user-attachments/assets/e45382d0-6e25-493d-a1fb-f2de818debe8" />

If there are no changes, a message is displayed with the corresponding content:</br>

<img width="310" height="166" alt="image" src="https://github.com/user-attachments/assets/2ba0c1cf-ef85-4442-9dd9-b8861eb94403" />

## 6. Forced commit of references
When you click on the "Fix current version of the solution" button in the menu or press the Alt + C key combination, the current references between projects, the current targetFramework versions of the project are committed and these parameters are checked for compliance with the rules loaded from the configuration files (at the time of this commit).
When the check is completed, it displays either a list of problems found by the extension, a warning that there are no references between projects, or a message that no problems were found in the solution (see point 2).
A message (MessageBox) with the result of successful commit execution is also displayed:</br>

<img width="321" height="172" alt="image" src="https://github.com/user-attachments/assets/74144246-02f3-42b4-88c9-15cd5399dae5" /></br>

<img width="327" height="175" alt="image" src="https://github.com/user-attachments/assets/240dd505-fc10-4a73-9d43-7a2de51efcc1" /></br>

<img width="450" height="173" alt="image" src="https://github.com/user-attachments/assets/6ea56a9e-ded4-4c5f-b2f3-d018eca2fc38" />

## 7. Tabular export of project state
When you click on the "Export to XLSX" button in the menu or press the Alt + X key combination, a tabular report is generated in the format of .xlsx file.

Based on the export results, the user will be prompted to open the folder with the saved export. When you click on the "Yes" button, the action described earlier will be performed. This message is also an export success message.</br>

<img width="434" height="160" alt="image" src="https://github.com/user-attachments/assets/2904ac6b-c00d-40c6-9407-22b1761d3562" />

The file will be saved at the following path and will have the following name:
````JSON
{solution¬_name}/reports/table_type/{DD.MM.YYYY-HH.MM.SS}/{ solution_name}_references_report.xlsx
````
, where D is the day digit, M is the month digit in the first case, minutes in the second, Y is the year digit, H is the hour digit, and S is the second digit. Together, these numbers form the exact date and time of report generation, which is the unique report ID.

The tabular report consists of three pages:
- Project selection page
- Selections based on references
- Current RefDepGuard errors
- Current RefDepGuard warnings

### a) Project selection page
It is a representation of all the projects included in the solution separately.
Next to each project are displayed:
- Its name
- Target work environment (targetFramework)
- Max allowed version of targetFramework
- Current recorded references between projects (Number and their names)
- Current unfixed mandatory references between projects (Number and their names)
- Current fixed invalid references between projects (Number and their names)
The name of the Solution and the date and time of report generation are displayed as the table header.</br>

<img width="975" height="263" alt="image" src="https://github.com/user-attachments/assets/60cfba88-3f83-4c15-a6b9-1cbb6bcac6b2" />

The maximum allowed version of targetFramework column uses the following conventions:
- S-Solution (Solution level restriction)
- G-Global (Global level limit)
- []- Square brackets (pointer to a constraint level other than Project)
- ? - Question mark (the user specified the restriction in the configuration file in an incorrect format, followed by the corresponding **framework_max_version deviant value error** in the error list)
- Red constraint color - Detected a targetFramework mismatch with the specified rule in the config file
- Yellow color of restriction - The rule contradicts some other rule (of the same level or different)
- Dash - No rules were found that create a limit on max version of the targetFramework project

### b) Reference selection page
It is a representation of all references included in the Solution separately.
How the reference name logic works, see point 2g.

Next to each reference are displayed:
- Its name
- Name of the project it belongs to
- Max allowed version of targetFramework
- Reference type (required, invalid, with a potential version conflict, simultaneously declared as required and invalid / "?" or without the special feature / "-")

The name of the Solution and the date and time of report generation are displayed as the table header.</br>

<img width="528" height="491" alt="image" src="https://github.com/user-attachments/assets/1c61168e-2c78-44ab-92e7-80429da551ec" />

Undetected mandatory references can be displayed in a separate field at the end of the table, if they are found: </br>

<img width="528" height="491" alt="image" src="https://github.com/user-attachments/assets/7fb9d7d2-eb0e-467d-8aa6-b4c071d84262" />


### c) Current RefDepGuard errors page
This is a representation of all errors detected by the extension at the time of generating the report.

Next to each error, the following is displayed:
- A project that is relevant to the error (for a number of errors, a dash is drawn, because they apply to all projects)
- Reference (If the error is related to some reference between projects, then it is displayed, otherwise-a dash)
- Error type (see possible error types in point 2A)
- Error level (see point 3B)
- Error description
- Necessary (recommended) action to solve the problem
- The file in which you need to perform the action (if it refers to .csproj, then you need to perform an action with the project located inside VS22; if it refers to.rdg, then you need to change the configuration file settings)
The name of the Solution and the date and time of report generation are displayed as the table header.</br>

<img width="999" height="214" alt="image" src="https://github.com/user-attachments/assets/dbe000c8-c15c-4ab3-a3a3-63848353fb1f" />

If no errors are detected at the time of export, the corresponding message will be displayed on the sheet:</br>

<img width="1028" height="94" alt="image" src="https://github.com/user-attachments/assets/4c7f7bae-e254-49fa-a370-16b5d05bfa40" />

### d) Current RefDepGuard Warnings page
It is a representation of all warnings detected by the extension at the time of generating the report.

The following messages are displayed next to each warning:
- Relevant project (for a number of warnings, a dash is drawn, because they apply to all projects)
- Reference (If the warning is related to some reference between projects, then it is displayed, otherwise-a dash)
- Warning type (see possible warning types in point 2A)
- Warning level (similar to the type of warnings, see point 3B)
- Description
- Necessary (recommended) action to solve the problem
- The file in which you need to perform the action (if it refers to .csproj, then you need to perform an action with the project located inside VS22; if it refers to.rdg, then you need to change the configuration file settings)
The name of the Solution and the date and time of report generation are displayed as the table header.</br>

<img width="1004" height="263" alt="image" src="https://github.com/user-attachments/assets/490c4976-b34a-4c96-8588-1b887846399a" />

If no warnings are detected at the time of export, the corresponding message will be displayed on the sheet:</br>

<img width="1011" height="81" alt="image" src="https://github.com/user-attachments/assets/77e08d58-6dbd-41d9-8ef6-02320be51322" />

## 8. Graphical export of project state
When you click on the "Export to HTML" button in the menu or press the Alt + H key combination, a graphical report is generated in the format .html file with [mermaid](https://docs.mermaidchart.com/mermaid-oss/intro/getting-started.html#native-mermaid-support://docs.mermaidchart.com/mermaid-oss/intro/getting-started.html#native-mermaid-support). 

Based on the export results, the user will be prompted to open the folder with the saved export. When you click on the "Yes" button, the action described earlier will be performed. This message is also an export success message.</br>

<img width="435" height="154" alt="image" src="https://github.com/user-attachments/assets/30b08718-beef-4186-95a5-b0ffb104e4fd" />

The file will be saved at the following path and will have the following name:
````JSON
{solution_name}/reports/graph_type/{DD.MM.YYYY-HH.MM.SS}/{solution_name}_references_report.html
````
, where D is the day digit, M is the month digit in the first case, minutes in the second, Y is the year digit, H is the hour digit, and S is the second digit. Together, these numbers form the exact date and time of report generation, which is the unique report ID.

The appearance of the graphical report is shown below:< /br>
<img width="1003" height="672" alt="image" src="https://github.com/user-attachments/assets/3c29aaf2-ede6-4c32-88ae-8835e54e9cd0" />
</br>
<img width="942" height="573" alt="image" src="https://github.com/user-attachments/assets/956a0bdb-cd19-47e2-8151-8c3cf2cee5ce" />

The report uses the following symbols:
- Bold link highlighting - mandatory reference between projects
- Red selection of the project - a project that violates its limit on max the version of targetFramework specified in the configuration file
- Yellow project selection - a project that has a conflict of max versions of targetFramework with the project it is linked to
- Red link selection - Either a mandatory reference that is missing or an invalid one that is present. They differ in the name of the link
- Yellow link selection - Potential conflict when upgrading the targetFramework version for an outgoing project, as a "smaller to larger" link was detected (see point 2g)

Projects are rectangles that have 2 - 3 lines. The first one shows the project name, the second one shows its targetFramework, and the third one shows the limit for the maximum possible version of targetFramework. **If there are no limits for max, the third line is not displayed**

### a) Prioritizing special symbols when drawing links
Since several different special symbols can be found for a single relationship, it makes sense to deduce the most important one. The prioritization of special symbols is as follows:

Red Link Selection > Yellow Link selection > Bold Link selection
