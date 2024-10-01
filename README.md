# Visual Ternera - W.I.P

Visual Ternera es un entorno de desarrollo integrado para etiquetas con el lenguaje ZPL.

# ¿Como funciona?
Sus funcionalidades están separadas en distintos módulos independientes de dos tipos, pantallas y servicios.
Cada modulo esta separado como un proyecto diferente dentro de la solución. Entre ellos se encuentran los siguientes módulos:

## Main:
Es el único modulo integrado en el ejecutable. Su principal función es cargar y mostrarle al usuario los módulos disponibles en el sistema.
Ademas, es el responsable de representar el estado de los puestos operacionales. Sistema relacionado con el servicio [VSTS](https://github.com/runfo-sa/visual-ternera-servicio).

## Editor:
Es el modulo encargado en crear y modificar las etiquetas disponibles en el sistema. Ademas de ofrecer herramientas de ayuda para facilitar su edición.

## Comparador:
Su función consiste en permitir la comparación de dos etiquetas, ya sean dos etiquetas totalmente diferentes o dos versiones distintas de la misma.

## Verificador:
Permite a traves de un set de reglas por etiqueta, validar que todos los productos asignados a esa etiqueta estén correctamente parametrizados.

## Publicar:
Es el encargado en habilitar las etiquetas que van a estar disponibles para la producción.

# Servicios
El sistema ademas incluye, servicios modulares y fácilmente reemplazables, que son ampliamente utilizados por los módulos previamente mencionados.

## Backend:
Servicio encargado de proporcionar los datos necesarios para completar las variables encontradas en las etiquetas con datos reales.

Visual Ternera incluye una implementación con el sistema PiQuatro de Twins Informática.

## Preview:
Servicio responsable de generar una previsualización digital de la etiqueta seleccionada.

Visual Ternera incluye una implementación con la API de Labelary.

## Version:
Servicio que administra el sistema de control de versionado a utilizar. Ademas de permitir modificar la forma en la que se almacenan las etiquetas con total transparencia.

Visual Ternera incluye dos implementaciones, una utilizando Git como el control de versionado y un tipo de almacenamiento por archivos tradicional. Y una utilizando una base de datos como el control de versionado y sistema de almacenamiento.