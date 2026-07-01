# General Information
**RefDepGuard.Console** is a "lightweight" version of the RefDepGuard extension and it designed to work in environments without an installed IDE. This app allows you to verify that the current state of the solution complies with the rules specified in the configuration files as part of an automated test (CI / CD pipeline)

# User_Guide-relevant version of the app
2.1.0

# Conditions for correct operation

In order for the console app to work correctly on the required device, you need to meet a number of conditions:
## 0. The device must have Windows 10+ OS
The program assumes use on Windows x64 OS

## 1. Install dotnet 8 or lower

First of all, we recommend checking the installed versions of dotnet using cmd. If at least 1 sdk version 8 or lower is displayed, the program will work correctly:< /br>
</br>
<img width="342" height="97" alt="image" src="https://github.com/user-attachments/assets/9e221c8c-4e60-4dc0-873a-42c3fbbf50e9" /></br>
</br>
If the sdk is missing, you can download it from the following [link](https://dotnet.microsoft.com/ru-ru/download/dotnet/8.0://dotnet.microsoft.com/ru-ru/download/dotnet)

## 2. Write a startup script

When running on a regular device (not a CI/CD pipeline), we recommend writing a simple one .bat script in order to the .exe file did not close immediately after execution:</br>

```
RefDepGuard.Console.exe en-US
pause
```
</br>
The resulting file can be named by any name, but we will continue to call it * *RefDepGuardStart.bat**.</br>

You can also replace the "en-US" argument with "ru-RU" or remove it altogether. This will allow the program to change the language of its messages from English to Russian

## 3. Place .exe and .bat files in root

You need to place the created one .bat file and console application file in the solution root folder: the folder where the .sln file is located:</br>

<img width="725" height="551" alt="Items (4)" src="https://github.com/user-attachments/assets/543fd078-7234-4c82-9d0f-14be14716752" /></br>
</br>
_!Important: The name of the .sln file must match the name of the root folder. Otherwise, the wrong solution will be checked, or it will not be checked at all!_

## 4. Launch .bat file

Finally, click LMB on .bat file and the console application starts correctly

# Features of the console app

RefDepGuard.Console checks whether the current state of the solution meets the specified rules in several stages:
## 0. Greeting
A welcome message is displayed along with the name of the solution to be checked </br>

<img width="601" height="63" alt="image" src="https://github.com/user-attachments/assets/439458f9-08fd-4383-8d87-4150d0dfb50c" />

## 1. Solution state parsing
During this stage, the application receives information about all projects included in the solution, as well as their TargetFramework versions and relationships. This data is also duplicated in the console:</br>

<img width="512" height="945" alt="image" src="https://github.com/user-attachments/assets/18cc8cb3-02e8-4ae9-8eba-734a3f4cd044" />


## 2. Parsing rules from configuration files
During this stage, the application receives data about the rules specified in the configuration files.</br>
</br>
<img width="354" height="44" alt="image" src="https://github.com/user-attachments/assets/a0774a75-639f-4f1f-ae3e-6967aa475679" />
</br>
_!Important: unlike the extension, the console application does not know how to generate configuration file templates, so any syntax errors will lead to unsuccessful parsing of the rules and require finding problems and editing them manually!_
</br>
<img width="601" height="159" alt="image" src="https://github.com/user-attachments/assets/c376e025-f815-42ef-92a7-e2740f8de18e" />


## 3. Checking whether the status complies with the declared rules and displaying detected problems
During this stage, the application checks whether the current state complies with the declared rules, which results in problems detected or a message about their absence </br>
</br>
<img width="1370" height="890" alt="image" src="https://github.com/user-attachments/assets/64a60049-8944-454a-bc6c-882ced3ed853" />
</br>
<img width="484" height="126" alt="image" src="https://github.com/user-attachments/assets/7a76818a-5e8f-4a13-b1fb-3cd18f103328" />
</br>
_!Important: if errors are detected as a result of the check, or the check fails for some reason (fail at stage 1-3), the program ends execution with the code "-1", which corresponds to completion with an error.  Otherwise - with the code "0"(successful execution)!_
