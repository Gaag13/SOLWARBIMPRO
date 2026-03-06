# WARBIMPRO

![Revit](https://img.shields.io/badge/Revit-2023--2026-blue)
![Lenguaje](https://img.shields.io/badge/Lenguaje-C%23-green)
![Framework](https://img.shields.io/badge/.NET-Framework%204.8%20%7C%20.NET%208-purple)
![Estado](https://img.shields.io/badge/Estado-En%20desarrollo-success)

**WARBIMPRO** es un **complemento (Add-in) profesional para Autodesk Revit** desarrollado en **C# utilizando la Revit API**, diseñado para mejorar la **productividad BIM, automatizar tareas repetitivas y optimizar la gestión de modelos dentro de Revit**.

El complemento incorpora herramientas para **gestión de familias, control de vistas, cuantificación de elementos y automatización de flujos de trabajo**, permitiendo a modeladores BIM, ingenieros y arquitectos trabajar de forma más eficiente.

---

# Vista General

WARBIMPRO integra un conjunto de herramientas directamente en el **Ribbon de Revit**, facilitando tareas comunes dentro del modelado BIM como:

- Gestión de familias
- Organización de vistas
- Automatización de modelado
- Extracción de cantidades
- Exportación de datos a Excel

El objetivo del proyecto es **mejorar la eficiencia en proyectos BIM reduciendo tareas manuales y repetitivas**.

---

# Funcionalidades Principales

## Gestión de Sesión

### Inicio de Sesión

El plugin incluye un sistema de **inicio de sesión y registro de usuarios**, permitiendo autenticarse dentro de la plataforma WARBIMPRO y habilitando futuras integraciones con servicios en la nube.

---

# Herramientas de Gestión de Familias

## Importar Familias

Permite **cargar múltiples familias de Revit (.rfa)** desde una ruta específica de manera rápida y organizada.

Beneficios:

- Importación masiva de familias
- Gestión eficiente de bibliotecas BIM
- Ahorro de tiempo en proyectos grandes

---

## Explorador de Familias (Family Browser)

Incluye un **Dockable Panel** que permite explorar las familias cargadas en el proyecto.

El explorador organiza las familias por:

- **Familia**
- **Tipos de familia**
- **Family Symbols**

Los elementos pueden **arrastrarse directamente al modelo**, facilitando la inserción de componentes BIM.

---

## Exportar Familias

Permite **exportar familias del proyecto como archivos individuales (.rfa)**.

Esto permite:

- Crear bibliotecas reutilizables
- Compartir contenido BIM entre proyectos
- Organizar recursos de modelado

---

# Herramientas de Gestión de Vistas

## Buscar en Vistas

Permite **buscar y gestionar vistas dentro del proyecto**, facilitando la organización en modelos complejos.

---

## Duplicar Elementos Estructurales

Herramienta para **duplicar elementos estructurales rápidamente**, optimizando la creación de elementos repetitivos en el modelo.

---

## Transferir Plantillas de Vista

Permite **transferir View Templates entre proyectos de Revit**, ayudando a mantener consistencia visual y estándares BIM.

---

# Cuantificación del Modelo

## Cuantificación de Elementos

El módulo de cuantificación permite generar informes de:

- Áreas
- Volúmenes
- Cantidades de materiales

Los datos pueden **exportarse a Excel** para análisis adicional o generación de reportes.

---

# Interfaz de Usuario

WARBIMPRO detecta automáticamente el **tema de Revit** y adapta los iconos según el entorno:

- Soporte para **tema claro**
- Soporte para **tema oscuro**

Esto mejora la visibilidad y la experiencia de usuario.

---

# Arquitectura del Proyecto

El proyecto sigue una arquitectura basada en **MVVM (Model-View-ViewModel)**, permitiendo una mejor organización del código y facilitando el mantenimiento y escalabilidad.

Estructura principal:





---

# Tecnologías Utilizadas

- **C#**
- **Revit API**
- **.NET Framework 4.8**
- **.NET 8**
- **Arquitectura MVVM**

---

# Compatibilidad

Actualmente el plugin es compatible con:

- Revit 2023
- Revit 2024
- Revit 2025
- Revit 2026

---

# Instalación

Una vez compilado o instalado el paquete del plugin, aparecerá automáticamente una nueva pestaña en el **Ribbon de Revit llamada WARBIMPRO**.

---

# Autor

**Giancarlo Arciniegas**  
BIM Developer

GitHub:  
https://github.com/Gaag13
Instagram
[Instagram WARBIMPRO](https://www.instagram.com/warbimpro/)
---

# Contribuciones

Las contribuciones, sugerencias y reportes de errores son bienvenidos a través de **GitHub Issues o Pull Requests**.

---

# Roadmap Futuro

Entre las mejoras planificadas se encuentran:

- Integración con servicios en la nube
- Análisis avanzado de cantidades
- Herramientas de estimación de costos
- Automatización de reportes BIM
- Paneles de análisis de datos

---

# Capturas del Plugin

Puedes agregar imágenes del plugin así:

```markdown

