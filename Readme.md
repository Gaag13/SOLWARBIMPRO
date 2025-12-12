# SOLCANTIDADES 🚀  

**SOLCANTIDADES** es un complemento (**addin**) para **Autodesk Revit**, desarrollado en **C#**, que permite calcular en **tiempo real** las áreas y volúmenes de los elementos mientras se modela en **BIM**.  

Su objetivo principal es proporcionar a los modeladores una herramienta que facilite la **extracción de WARBIMPRO de obra en tiempo real**, mejorando la precisión y eficiencia en la planificación y ejecución de proyectos de construcción.  

## 🔹 **Características Principales**  
✔ **Cálculo en tiempo real** de **áreas y volúmenes** durante la modelación BIM.  
✔ **Optimización del proceso de diseño**, permitiendo decisiones más rápidas y precisas.  
✔ **Integración fluida con Revit**, mejorando el flujo de trabajo del modelador.  
✔ **Datos instantáneos sin necesidad de procesos manuales adicionales**.  
## 🔹 **Tecnologías Utilizadas**  
- 💻 **C# y Revit API** – Desarrollo del add-in y extracción de datos de modelos BIM.  
- 🏗 **Autodesk Revit** – Software donde se ejecuta el complemento.  

## 🔹 **Uso y Aplicaciones**  
✅ Modeladores BIM que necesiten **calcular WARBIMPRO de obra de manera instantánea**.  
✅ Ingenieros y arquitectos que quieran **optimizar el proceso de estimación de costos**.  
✅ Empresas de construcción que busquen **automatizar cálculos y mejorar su planificación**.  
✅ **Estimación de materiales** basada en los volúmenes extraídos y utilizando **dosificaciones predefinidas**.  
✅ **Cálculo de WARBIMPRO de materiales** como **arena, agua, grava y cemento** según la obra modelada.  
✅ **Generación de un presupuesto preliminar**, calculando el costo total estimado del proyecto.  
## 📩 **Contribuciones y Contacto**  
Si deseas contribuir o necesitas soporte, visita el repositorio en [GitHub](https://github.com/Gaag13/SOLCANTIDADES).  

**¡Optimiza la estimación de WARBIMPRO y costos en BIM con SOLCANTIDADES!** 🚀🏗  


Autodesk Revit plugin project organised into multiple solution files that target versions 2020 - 2025.

### Technologies Used

* C# 12
* .NET Framework 4.8
* .NET 8

### Getting Started

Before you can build this project, you will need to install .NET, depending upon the solution file you are building. If you haven't already installed these
frameworks, you can do so by visiting the following:

* [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48)
* [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet)

After installing the necessary frameworks, clone this repository to your local machine and navigate to the project directory.

### Building

We recommend JetBrains Rider as preferred IDE, since it has outstanding .NET support. If you don't have Rider installed, you can download it
from [here](https://www.jetbrains.com/rider/).

1. Open JetBrains Rider
2. Click on `File -> Open` and choose the RevitLookup.sln file to open.
3. In the `Solutions Configuration` drop-down menu, select `Release R25` or `Debug R25`. Suffix `R25` means compiling for the Revit 2025.
4. After the solution loads, you can build it by clicking on `Build -> Build Solution`.

Also, you can use Visual Studio. If you don't have Visual Studio installed, download it from [here](https://visualstudio.microsoft.com/downloads/).

1. Open Visual Studio
2. Click on `File -> Open -> Project/Solution` and locate your solution file to open.
3. In the `Solutions Configuration` drop-down menu, select `Release R25` or `Debug R25`. Suffix `R25` means compiling for the Revit 2025.
4. After the solution loads, you can build it by clicking on `Build -> Build Solution`.

### MSI installer and bundle build on the local machine

To build the project for all versions, create the installer and bundle, the project uses [NUKE](https://github.com/nuke-build/nuke)

To execute your NUKE build locally, you can follow these steps:

1. **Install NUKE as a global tool**. First, make sure you have NUKE installed as a global tool. You can install it using dotnet CLI:

    ```powershell
    dotnet tool install Nuke.GlobalTool --global
    ```

   You only need to do this once on your machine.

2. **Navigate to your project directory**. Open a terminal / command prompt and navigate to your project's root directory.
3. **Run the build**. Once you have navigated to your project's root directory, you can run the NUKE build by calling:

   Compile:
   ```powershell
   nuke
   ```

   Create installer:
   ```powershell
   nuke createinstaller
   ```

   Create installer and bundle:
   ```powershell
   nuke createinstaller createbundle
   ```

   This command will execute the NUKE build defined in your project.

### Solution structure

| Folder  | Description                                                                |
|---------|----------------------------------------------------------------------------|
| build   | Nuke build system. Used to automate project builds                         |
| install | Add-in installer, called implicitly by the Nuke build                      |
| source  | Project source code folder. Contains all solution projects                 |
| output  | Folder of generated files by the build system, such as bundles, installers |

### Project structure

| Folder     | Description                                                                                                                                                                                          |
|------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Commands   | External commands invoked from the Revit ribbon. Registered in the `Application` class                                                                                                               |
| Models     | Classes that encapsulate the app's data, include data transfer objects (DTOs). More [details](https://learn.microsoft.com/en-us/dotnet/architecture/maui/mvvm).                                      |
| ViewModels | Classes that implement properties and commands to which the view can bind data. More [details](https://learn.microsoft.com/en-us/dotnet/architecture/maui/mvvm).                                     |
| Views      | Classes that are responsible for defining the structure, layout and appearance of what the user sees on the screen. More [details](https://learn.microsoft.com/en-us/dotnet/architecture/maui/mvvm). |
| Resources  | Images, sounds, localisation files, etc.                                                                                                                                                             |
| Utils      | Utilities, extensions, helpers used across the application                                                                                                                                           |

### Learn More

* You can explore more in the [RevitTemplates wiki](https://github.com/Nice3point/RevitTemplates/wiki) page.
