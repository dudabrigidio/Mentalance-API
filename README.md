# Mentalance API

API REST desenvolvida em ASP.NET Core 8.0 para gerenciamento de saúde mental, permitindo que usuários registrem check-ins emocionais e recebam análises semanais personalizadas com recomendações geradas por Machine Learning.
## 📱 Sobre o Projeto

O **Mentalance** é um aplicativo mobile desenvolvido em React Native para ajudar usuários a monitorar e entender melhor suas emoções ao longo do tempo. Através de check-ins diários e análises semanais com inteligência artificial, o aplicativo oferece insights valiosos sobre padrões emocionais e recomendações personalizadas para o bem-estar mental.

## 👥 Integrantes do Grupo

- **André Luís Mesquita de Abreu** - RM558159
- **Maria Eduarda Brigidio** - RM558575
- **Rafael Bompadre Lima** - RM556459

## 🎥 Vídeo de Demonstração

[🔗 Link para o vídeo no YouTube](https://youtu.be/Vpxcz1JedAE?si=b9iV8jS9OYC-GHqs)


## 🎯 Sobre o Projeto

O **Mentalance** é uma plataforma de saúde mental que permite aos usuários:

- Registrar check-ins emocionais diários
- Receber análises semanais automáticas sobre seu estado emocional
- Obter recomendações personalizadas baseadas em Machine Learning
- Acompanhar sua jornada de bem-estar ao longo do tempo

A API foi desenvolvida seguindo boas práticas de desenvolvimento, incluindo arquitetura em camadas, versionamento de API, logging estruturado e tracing.

## ✨ Funcionalidades

### 👤 Gerenciamento de Usuários
- Cadastro de novos usuários
- Autenticação via email e senha (login)
- Atualização de dados do usuário
- Listagem e consulta de usuários

### 📝 Check-ins Emocionais
- Registro de check-ins com emoção e texto descritivo
- Análise automática de sentimento (positivo, neutro, negativo)
- Geração automática de respostas personalizadas
- Histórico completo de check-ins

### 📊 Análises Semanais
- Geração automática de análises semanais baseadas nos últimos 7 dias
- Identificação da emoção predominante
- Resumo textual gerado por ML
- Recomendações personalizadas baseadas nos padrões identificados

### 🤖 Machine Learning
- Modelos treinados para geração de resumos e recomendações
- Análise de padrões emocionais
- Personalização baseada no histórico do usuário

## 🛠 Tecnologias Utilizadas

### Framework e Linguagem
- **.NET 8.0** - Framework principal
- **C#** - Linguagem de programação
- **ASP.NET Core** - Framework web

### Banco de Dados
- **Oracle Database** - Banco de dados relacional
- **Entity Framework Core 9.0** 

### Machine Learning
- **ML.NET 4.0** - Framework de Machine Learning para .NET

### Observabilidade e Logging
- **Serilog** - Logging estruturado
- **OpenTelemetry** - Rastreamento distribuído
- **Health Checks** - Monitoramento de saúde da aplicação

### Documentação e API
- **Swagger/OpenAPI** - Documentação interativa da API
- **API Versioning** - Versionamento de endpoints

### Outras Bibliotecas
- **CORS** - Configuração de políticas de origem cruzada

## 📦 Pré-requisitos

Antes de começar, certifique-se de ter instalado:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Oracle Database](https://www.oracle.com/database/) ou acesso a uma instância Oracle
- [Visual Studio 2022](https://visualstudio.microsoft.com/) 
- [Git] (para clonar o repositório)

## 🚀 Instalação

1. Clone o repositório:
```bash
git clone https://github.com/seu-usuario/Mentalance.git
cd Mentalance
```

2. Restaure as dependências do projeto:
```bash
cd Mentalance
dotnet restore
```

3. Configure a string de conexão no arquivo `appsettings.json` (veja seção [Configuração](#configuração))

4. Execute as migrações do Entity Framework:
```bash
dotnet ef database update
```

## ⚙️ Configuração

### String de Conexão

Edite o arquivo `appsettings.json` ou `appsettings.Development.json` e configure a string de conexão do Oracle:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "User Id=seu_usuario;Password=sua_senha;Data Source=servidor:porta/orcl"
  }
}
```

### Configuração de Logging


Ajuste os níveis de log no `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Mentalance": "Debug"
      }
    }
  }
}
```

### CORS

A aplicação está configurada com duas políticas CORS:
- `AllowLocalhost`: Permite apenas requisições de `http://localhost:8081`
- `AllowCors`: Permite requisições de qualquer origem (usado atualmente)

Ajuste conforme necessário no `Program.cs`.

## ▶️ Executando a Aplicação

### Modo Desenvolvimento

```bash
cd Mentalance
dotnet run
```

A aplicação estará disponível em:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

### Swagger UI

Quando executada em modo de desenvolvimento, acesse a documentação interativa da API:

```
https://localhost:5001/swagger
```

### Health Check

Verifique o status da aplicação:

```
https://localhost:5001/health
```

## 📚 Documentação da API

A documentação completa da API está disponível através do Swagger UI quando a aplicação está em execução. A API suporta versionamento e está atualmente na versão **v1.0**.

### Versionamento

A API suporta versionamento através de:
- Query String: `?api-version=1.0`
- Header: `x-api-version: 1.0`
- URL: `/api/v1/controller`

## 📁 Estrutura do Projeto

```
Mentalance/
├── Controllers/          # Controladores da API
│   ├── AnaliseSemanalController.cs
│   ├── CheckinController.cs
│   ├── UsuarioController.cs
│   └── HealthController.cs
├── Models/               # Modelos de domínio
│   ├── AnaliseSemanal.cs
│   ├── Checkin.cs
│   ├── Usuario.cs
│   └── EmocaoEnum.cs
├── Dto/                  # Data Transfer Objects
│   ├── CheckinDto.cs
│   ├── LoginDto.cs
│   └── UsuarioDto.cs
├── Service/              # Camada de serviços
│   ├── AnaliseSemanalService.cs
│   ├── CheckinService.cs
│   └── UsuarioService.cs
├── Repository/           # Camada de repositório
│   ├── AnaliseSemanalRepository.cs
│   ├── CheckinRepository.cs
│   └── UsuarioRepository.cs
├── ML/                   # Serviços de Machine Learning
│   ├── Service/
│   │   └── MLService.cs
│   └── Data/
│       ├── AnaliseML.cs
│       ├── RecomendacaoML.cs
│       └── dadosTreino.json
├── Connection/           # Contexto do Entity Framework
│   └── AppDbContext.cs
├── Converters/           # Conversores JSON
│   └── EmocaoEnumJsonConverter.cs
├── Migrations/           # Migrações do banco de dados
└── Program.cs            # Ponto de entrada da aplicação
```

## 🔌 Endpoints Principais

### Usuários

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/v1/Usuarios` | Lista todos os usuários |
| GET | `/api/v1/Usuarios/{id}` | Busca usuário por ID |
| POST | `/api/v1/Usuarios` | Cria novo usuário |
| PUT | `/api/v1/Usuarios/{id}` | Atualiza usuário |
| DELETE | `/api/v1/Usuarios/{id}` | Remove usuário |
| POST | `/api/v1/Usuarios/Login` | Realiza login |

### Check-ins

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/v1/Checkins` | Lista todos os check-ins |
| GET | `/api/v1/Checkins/{id}` | Busca check-in por ID |
| POST | `/api/v1/Checkins` | Cria novo check-in |
| PUT | `/api/v1/Checkins/{id}` | Atualiza check-in |
| DELETE | `/api/v1/Checkins/{id}` | Remove check-in |

### Análises Semanais

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/v1/AnaliseSemanal` | Lista todas as análises |
| GET | `/api/v1/AnaliseSemanal/{id}` | Busca análise por ID |
| POST | `/api/v1/AnaliseSemanal?idUsuario={id}` | Gera análise semanal |
| DELETE | `/api/v1/AnaliseSemanal/{id}` | Remove análise |

### Exemplo de Requisição - Criar Check-in

```json
POST /api/v1/Checkins
Content-Type: application/json

{
  "idUsuario": 1,
  "emocao": "Feliz",
  "texto": "Hoje estou feliz porque consegui resolver o problema do cliente"
}
```

**Emoções disponíveis:** `Feliz`, `Cansado`, `Ansioso`, `Calmo`, `Estressado`

## 🤖 Machine Learning

O sistema utiliza ML.NET para gerar análises e recomendações personalizadas:

### Modelos Implementados

1. **Modelo de Resumo**: Gera resumos textuais sobre o estado emocional da semana
2. **Modelo de Recomendação**: Gera recomendações personalizadas baseadas nos padrões identificados

### Treinamento

Os modelos são treinados automaticamente na inicialização da aplicação usando dados de treinamento localizados em `ML/Data/dadosTreino.json`.

### Funcionalidades ML

- Análise de sentimento automática nos check-ins
- Identificação de emoção predominante na semana
- Geração de resumos contextuais
- Recomendações personalizadas baseadas em padrões

## 📊 Logging e Observabilidade

### Serilog

A aplicação utiliza Serilog para logging estruturado com:
- Enriquecimento de contexto (ambiente, máquina, thread)
- Níveis de log configuráveis

### OpenTelemetry

Rastreamento distribuído configurado para:
- Requisições HTTP
- Chamadas de banco de dados (Entity Framework)
- Operações de Machine Learning
- Exportação para console 

### Health Checks

Endpoint `/health` monitora:
- Status do banco de dados Oracle
- Saúde geral da aplicação

## 🧪 Testes

O projeto inclui testes unitários e de integração no projeto `MentalanceTests`:

```bash
cd MentalanceTests
dotnet test
```

### Estrutura de Testes

- **Unit Tests**: Testes de serviços e lógica de negócio

## 🤝 Contribuindo

Contribuições são bem-vindas! Para contribuir:

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📝 Licença

Este projeto está sob licença [especificar licença]. Veja o arquivo `LICENSE` para mais detalhes.

## 👥 Integrantes do projeto

- Maria Eduarda Brigidio - RM558575
- André Luís Mesquita de Abreu- RM558159
- Rafael Bompadre Lima - RM556459

---

**Desenvolvido para promover o bem-estar mental**

