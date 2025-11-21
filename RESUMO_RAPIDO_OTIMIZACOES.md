# ⚡ Resumo Rápido - Otimizações da Análise Semanal

## 🎯 4 Passos para Otimizar

### 1️⃣ Singleton no MLService (MAIOR IMPACTO)
**Arquivo:** `Program.cs` linha 118
```csharp
// MUDAR DE:
builder.Services.AddScoped<IMLService, MLService>();
// PARA:
builder.Services.AddSingleton<IMLService, MLService>();
```
**Resultado:** Modelos treinam apenas 1 vez (na inicialização)

---

### 2️⃣ Reutilizar PredictionEngine
**Arquivo:** `MLService.cs`

**Adicionar campos (após linha 21):**
```csharp
private PredictionEngine<SemanaAnalise, AnaliseML>? _engineResumo;
private PredictionEngine<SemanaAnalise, RecomendacaoML>? _engineRecomendacao;
private readonly object _lockObject = new object();
```

**No construtor (após linha 85):**
```csharp
if (_model != null)
    _engineResumo = _mlContext.Model.CreatePredictionEngine<SemanaAnalise, AnaliseML>(_model);
if (_modelRecomendacao != null)
    _engineRecomendacao = _mlContext.Model.CreatePredictionEngine<SemanaAnalise, RecomendacaoML>(_modelRecomendacao);
```

**No método GerarResumoAsync (linha 159):**
```csharp
// SUBSTITUIR:
var engineResumo = _mlContext.Model.CreatePredictionEngine<SemanaAnalise, AnaliseML>(_model);
var resultadoResumo = engineResumo.Predict(input);

// POR:
if (_engineResumo != null)
{
    lock (_lockObject)
    {
        var resultadoResumo = _engineResumo.Predict(input);
        resultado.Resumo = resultadoResumo.Resumo;
    }
}
```
**Fazer o mesmo para `_engineRecomendacao` na linha 183**

---

### 3️⃣ Eliminar Busca Duplicada
**Arquivo:** `IMLService.cs` - Adicionar método:
```csharp
Task<AnaliseML> GerarResumoAsync(int usuarioId, IEnumerable<Checkin> checkins);
```

**Arquivo:** `MLService.cs` - Modificar método existente:
```csharp
public async Task<AnaliseML> GerarResumoAsync(int usuarioId)
{
    var checkins = await _checkinRepository.GetByUsuarioEPeriodoAsync(usuarioId);
    return await GerarResumoAsync(usuarioId, checkins.ToList());
}

public async Task<AnaliseML> GerarResumoAsync(int usuarioId, IEnumerable<Checkin> checkins)
{
    var checkinsList = checkins.ToList();
    // ... resto do código SEM buscar check-ins novamente
}
```

**Arquivo:** `AnaliseSemanalService.cs` linha 150:
```csharp
// MUDAR DE:
var analiseML = await _mlService.GerarResumoAsync(idUsuario);
// PARA:
var analiseML = await _mlService.GerarResumoAsync(idUsuario, checkinsList);
```

---

### 4️⃣ Adicionar Índices no Banco
**Arquivo:** `AppDbContext.cs` - No método `OnModelCreating`:
```csharp
modelBuilder.Entity<Checkin>()
    .HasIndex(c => c.IdUsuario)
    .HasDatabaseName("IX_Checkin_IdUsuario");

modelBuilder.Entity<Checkin>()
    .HasIndex(c => new { c.IdUsuario, c.DataCheckin })
    .HasDatabaseName("IX_Checkin_IdUsuario_DataCheckin");
```

**Criar Migration:**
```bash
dotnet ef migrations add AdicionarIndicesCheckin
dotnet ef database update
```

---

## 📊 Impacto Esperado

| Passo | Redução de Tempo | Dificuldade |
|-------|------------------|-------------|
| 1️⃣ Singleton | 80-90% | ⭐ Fácil |
| 4️⃣ Índices | 20-30% | ⭐ Fácil |
| 2️⃣ PredictionEngine | 10-20% | ⭐⭐ Médio |
| 3️⃣ Eliminar Duplicação | 5-10% | ⭐⭐⭐ Complexo |

**Total esperado:** De ~5-10 segundos para ~0.5-1 segundo

---

## ✅ Checklist de Implementação

- [ ] Passo 1: Singleton implementado
- [ ] Passo 2: PredictionEngine reutilizado
- [ ] Passo 3: Busca duplicada eliminada
- [ ] Passo 4: Índices criados no banco
- [ ] Aplicação testada e funcionando
- [ ] Tempo de resposta melhorou

---

**📖 Para detalhes completos, veja:** `GUIA_OTIMIZACAO_ANALISE_SEMANAL.md`

