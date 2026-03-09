# tns-agents-orchestrator-be

Solución para orquestación de agentes genéricos usando **Arquitectura Hexagonal** (Puertos y Adaptadores).

## Arquitectura

Este proyecto implementa la **Arquitectura Hexagonal** (también conocida como Ports & Adapters o Clean Architecture) para mantener el código del dominio agnóstico de la infraestructura, permitiendo conectarse a diferentes proveedores de LLM y servicios en la nube.

```
src/
├── domain/                        # Núcleo del dominio (sin dependencias externas)
│   ├── agent/
│   │   ├── Agent.ts               # Entidad principal del agente
│   │   └── AgentExecution.ts      # Entidad de ejecución del agente
│   ├── llm/
│   │   └── LLMTypes.ts            # Tipos del dominio LLM
│   └── shared/
│       ├── DomainError.ts         # Errores del dominio
│       └── UniqueId.ts            # Value Object para IDs únicos
│
├── application/                   # Capa de aplicación (casos de uso + puertos)
│   ├── ports/
│   │   ├── in/                    # Puertos de entrada (driving) - interfaces de casos de uso
│   │   └── out/                   # Puertos de salida (driven) - interfaces de repositorios y servicios
│   └── use-cases/                 # Implementación de los casos de uso
│
└── infrastructure/                # Capa de infraestructura (adaptadores)
    ├── adapters/
    │   ├── in/                    # Adaptadores de entrada (driving)
    │   │   ├── http/              # API REST (Express.js)
    │   │   └── worker/            # Worker de procesamiento en background
    │   └── out/                   # Adaptadores de salida (driven)
    │       ├── llm/               # Proveedores LLM (OpenAI, Anthropic)
    │       ├── persistence/       # Repositorios en memoria
    │       └── messaging/         # Publicador de mensajes/eventos
    └── config/
        ├── container.ts           # Inyección de dependencias
        └── env.ts                 # Variables de entorno
```

## Características

- ✅ **Arquitectura Hexagonal** - Dominio completamente desacoplado de la infraestructura
- ✅ **Múltiples proveedores LLM** - Soporte para OpenAI (GPT-4o) y Anthropic (Claude)
- ✅ **API REST** - Endpoints para gestión de agentes y ejecuciones
- ✅ **Worker en background** - Procesamiento asíncrono de ejecuciones
- ✅ **Sistema de eventos** - Publicación de eventos para cada acción
- ✅ **TypeScript** - Tipado estático en todo el código
- ✅ **Tests** - Tests unitarios e integración

## Instalación

```bash
npm install
```

## Configuración

```bash
cp .env.example .env
# Editar .env con tus API keys
```

Variables de entorno:

| Variable | Descripción | Default |
|----------|-------------|---------|
| `PORT` | Puerto del servidor | `3000` |
| `LLM_PROVIDER` | Proveedor LLM activo (`openai` o `anthropic`) | `openai` |
| `OPENAI_API_KEY` | API Key de OpenAI | - |
| `OPENAI_MODEL` | Modelo de OpenAI a usar | `gpt-4o` |
| `ANTHROPIC_API_KEY` | API Key de Anthropic | - |
| `ANTHROPIC_MODEL` | Modelo de Anthropic a usar | `claude-3-5-sonnet-20241022` |
| `WORKER_CONCURRENCY` | Concurrencia del worker | `5` |
| `WORKER_POLL_INTERVAL_MS` | Intervalo de polling del worker (ms) | `1000` |

## Desarrollo

```bash
npm run dev        # Iniciar en modo desarrollo
npm run build      # Compilar TypeScript
npm start          # Iniciar versión compilada
npm test           # Ejecutar tests
npm run test:coverage  # Tests con cobertura
npm run lint       # Lint del código
```

## API Endpoints

### Salud
- `GET /health` — Health check

### Agentes
- `GET /api/v1/agents` — Listar todos los agentes
- `GET /api/v1/agents/:id` — Obtener un agente por ID
- `POST /api/v1/agents` — Crear un nuevo agente

### Ejecuciones
- `POST /api/v1/agents/:agentId/executions` — Ejecutar un agente
- `GET /api/v1/agents/:agentId/executions` — Listar ejecuciones de un agente
- `GET /api/v1/agents/:agentId/executions/:executionId` — Obtener una ejecución

### Ejemplo: Crear un agente

```bash
curl -X POST http://localhost:3000/api/v1/agents \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Asistente General",
    "description": "Agente para responder preguntas generales",
    "systemPrompt": "Eres un asistente útil y preciso.",
    "provider": "openai",
    "model": "gpt-4o",
    "capabilities": ["qa", "summarize"]
  }'
```

### Ejemplo: Ejecutar un agente

```bash
curl -X POST http://localhost:3000/api/v1/agents/{agentId}/executions \
  -H "Content-Type: application/json" \
  -d '{
    "input": "¿Cuál es la capital de Argentina?"
  }'
```

## Extensión

Para agregar un nuevo proveedor LLM:

1. Implementar la interfaz `ILLMProvider` en `src/infrastructure/adapters/out/llm/`
2. Registrar el proveedor en `LLMProviderFactory`
3. Agregar la configuración en `src/infrastructure/config/env.ts`

```typescript
// src/infrastructure/adapters/out/llm/MyNewProvider.ts
export class MyNewProvider implements ILLMProvider {
  readonly providerName = 'my-provider';

  async complete(request: LLMRequest): Promise<LLMResponse> {
    // Implementación
  }

  supportsModel(model: string): boolean {
    return model.startsWith('my-model-');
  }
}
```

