# Sistema Patentes CIF - ALPR Control Center

Sistema de reconocimiento vehicular desarrollado en WPF (.NET) orientado al análisis de dominios automotores, consulta registral y gestión histórica de detecciones.

La aplicación permite procesar imágenes, detectar patentes mediante OCR, consultar información vehicular asociada y almacenar los resultados para análisis posterior.

---

## Características principales

### Detección OCR

- Procesamiento de imágenes locales.
- Reconocimiento automático de patentes.
- Detección de múltiples vehículos por imagen.
- Visualización de regiones detectadas.
- Historial de lecturas en tiempo real.

### Consulta Vehicular

- Consulta manual por patente.
- Obtención de marca, modelo y año.
- Información geográfica asociada.
- Ficha detallada del vehículo.

### Historial

- Persistencia mediante SQLite.
- Filtrado instantáneo por dominio.
- Consulta de registros históricos.
- Visualización de fichas asociadas.

### Inteligencia

- Análisis estadístico de detecciones.
- Métricas operativas.
- Vehículos más observados.
- Indicadores y tendencias.

### Configuración

- Sistema de temas visuales.
- Configuración OCR.
- Configuración de API.
- Preferencias persistentes mediante JSON.

---

## Temas visuales

ALPR Control Center incorpora un sistema de temas dinámicos que pueden cambiarse en tiempo real sin reiniciar la aplicación.

### Batman
Tema oficial del proyecto.

- Fondo oscuro
- Acentos ámbar
- Estética forense

### Nightwing
Tema tecnológico moderno.

- Fondo azul profundo
- Acentos cian
- Inspirado en centros de operaciones y ciberseguridad

### Superman
Tema institucional.

- Azul oscuro
- Acentos carmesí
- Orientado a entornos gubernamentales

### Joker
Tema experimental.

- Púrpura profundo
- Verde neón
- Estética de laboratorio de inteligencia digital

---

## Arquitectura

```text
ALPR Control Center
│
├── Dashboard
├── Detección OCR
├── Consulta Vehicular
├── Historial
├── Inteligencia
├── Configuración
│
├── SQLite
├── Settings.json
├── ThemeManager
└── DatabaseManager
```

---

## Tecnologías utilizadas

- C#
- .NET
- WPF
- SQLite
- JSON
- GMap.NET
- OCR Engine

---

## Almacenamiento

### Configuración

Las preferencias de usuario se almacenan en:

```text
%LocalAppData%\SistemaPatentesCIF\
```

mediante archivos JSON.

### Historial

Las detecciones son almacenadas de forma persistente en SQLite para permitir auditoría y análisis posterior.

---

## Estado actual

### Implementado

- Sistema de temas dinámicos
- Persistencia SQLite
- Historial de detecciones
- Consulta vehicular
- Fichas detalladas
- Dashboard operativo
- Configuración persistente

### En desarrollo

- Métricas avanzadas
- Inteligencia vehicular
- Reportes
- Estadísticas de actividad

---

## Autor
Mgter. Marcos A. Monti
Desarrollado como plataforma de investigación y análisis vehicular para entornos forenses y de inteligencia.

Poder Judicial de Formosa
Laboratorio de Informática Forense
