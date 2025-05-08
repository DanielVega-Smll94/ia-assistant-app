# 🧠 IA Assistant App

Aplicación completa que integra un asistente de inteligencia artificial con generación de texto e imágenes, utilizando:

- 🎨 **Angular 17** (Frontend)
- 🔧 **ASP.NET Core 9.0** (Backend)
- 📦 **Docker & Docker Compose**
- 📜 **Swagger** para documentación de API

---

## 📁 Estructura del Proyecto

IA-ASSISTANT-APP/
├── ia-assistant-ui/ # Angular 17 frontend
└── OpenAiApiDemo/ # ASP.NET Core 9 backend


---

## 🚀 Cómo ejecutar localmente

### Requisitos

- Node.js 20+
- .NET SDK 9.0+
- Docker

### 1. Clonar el repositorio

```bash
git clone https://github.com/DanielVega-SmII94/ia-assistant-app.git
cd ia-assistant-app

BACKEND
cd OpenAiApiDemo
dotnet run
Swagger disponible


FRONTEND
cd ia-assistant-ui
npm install
ng serve

docker-compose up --build


✨ Funcionalidades
- Soporte para modelos OpenAI y Gemini
- Generación de texto e imágenes
- Visualización de historial
- Copiar HTML y Markdown
- Previsualización y descarga de imágenes
- Interfaz con scroll automático, checkbox y selectores de idioma/modelo

![Test](https://upload.wikimedia.org/wikipedia/commons/thumb/4/48/Markdown-mark.svg/1280px-Markdown-mark.svg.png)

## 📸 Capturas del sistema
![interfaz-chat](https://github.com/user-attachments/assets/bd555675-b2bb-46a9-b6f5-8a7bf3a36f91)

### 💬 Interfaz de chatbot:
![Interfaz del chat](https://raw.githubusercontent.com/DanielVega-Smll94/ia-assistant-app/master/readme-images/interfaz-chat.png)

### 🛠️ Herramientas de Generación
![Herramientas de generación](https://raw.githubusercontent.com/DanielVega-Smll94/ia-assistant-app/master/readme-images/herramientas-generacion.png)

### 🤖🧠 Generación de imágenes por medio de OPENAI:
![OpenAI](https://raw.githubusercontent.com/DanielVega-Smll94/ia-assistant-app/master/readme-images/openai-generacion.png)

### 🤖🪐 Generación de imágenes por medio de GEMINI:
![Gemini](https://raw.githubusercontent.com/DanielVega-Smll94/ia-assistant-app/master/readme-images/gemini-generacion.png)


