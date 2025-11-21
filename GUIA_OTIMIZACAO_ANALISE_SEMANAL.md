# 🚀 Guia Prático: Otimização da Análise Semanal

Este guia explica passo a passo como otimizar a performance da análise semanal, resolvendo os problemas de lentidão identificados.

---

## 📋 Problemas Identificados

1. **MLService treina modelos a cada requisição** (muito lento)
2. **Busca duplicada de check-ins** (desperdício de recursos)
3. **PredictionEngine criado a cada chamada** (custo alto)
4. **Falta de índices no banco de dados** (queries lentas)

---

## 🎯 Passo 1: Tornar MLService Singleton

### O que fazer:
O `MLService` está registrado como `Scoped` (linha 118 do `Program.cs`), o que significa que ele é criado a cada requisição HTTP. Isso faz com que os modelos ML sejam treinados toda vez!

### Como resolver:

**Arquivo:** `Mentalance/Program.cs`

**Localização:** Linha 118

**Mudança:**
```csharp
// ANTES (linha 118):
builder.Services.AddScoped<IMLService, MLService>();

// DEPOIS:
builder.Services.AddSingleton<IMLService, MLService>();
```

**Por quê?**
- `Singleton` cria apenas UMA instância do serviço quando a aplicação inicia
- Os modelos são treinados apenas uma vez (na inicialização)
- Todas as requisições compartilham a mesma instância (muito mais rápido)

**⚠️ Atenção:** 
- Certifique-se de que o `MLService` é thread-safe (geralmente é, pois ML.NET é thread-safe)
- Se houver problemas, pode usar `AddScoped` mas com lazy loading (mais complexo)

---

## 🎯 Passo 2: Reutilizar PredictionEngine

### O que fazer:
A cada chamada de `GerarResumoAsync`, novos `PredictionEngine` são criados. Isso é custoso e desnecessário.

### Como resolver:

**Arquivo:** `Mentalance/ML/Service/MLService.cs`

**Mudança 1:** Adicionar campos privados para armazenar os engines (após linha 21):
```csharp
private PredictionEngine<SemanaAnalise, AnaliseML>? _engineResumo;
private PredictionEngine<SemanaAnalise, RecomendacaoML>? _engineRecomendacao;
private readonly object _lockObject = new object(); // Para thread-safety
```

**Mudança 2:** Modificar o construtor para criar os engines após treinar os modelos (após linha 85):
```csharp
// Após treinar os modelos, criar os engines
if (_model != null)
{
    _engineResumo = _mlContext.Model.CreatePredictionEngine<SemanaAnalise, AnaliseML>(_model);
}
if (_modelRecomendacao != null)
{
    _engineRecomendacao = _mlContext.Model.CreatePredictionEngine<SemanaAnalise, RecomendacaoML>(_modelRecomendacao);
}
```

**Mudança 3:** Modificar o método `GerarResumoAsync` (linhas 154-199):

**Substituir:**
```csharp
var engineResumo = _mlContext.Model.CreatePredictionEngine<SemanaAnalise, AnaliseML>(_model);
var resultadoResumo = engineResumo.Predict(input);
```

**Por:**
```csharp
if (_engineResumo != null)
{
    lock (_lockObject) // Thread-safe
    {
        var resultadoResumo = _engineResumo.Predict(input);
        resultado.Resumo = resultadoResumo.Resumo;
    }
}
```

**Fazer o mesmo para `_engineRecomendacao`** (linhas 183-185).

**Por quê?**
- `PredictionEngine` é thread-safe e pode ser reutilizado
- Criar um novo engine a cada chamada é muito custoso
- Reutilizar economiza tempo e memória

---

## 🎯 Passo 3: Eliminar Busca Duplicada de Check-ins

### O que fazer:
Os check-ins são buscados duas vezes: uma vez no `AnaliseSemanalService` e outra no `MLService`.

### Como resolver:

**Opção A - Passar check-ins como parâmetro (Recomendado):**

**Arquivo:** `Mentalance/ML/Service/IMLService.cs`

**Adicionar novo método:**
```csharp
Task<AnaliseML> GerarResumoAsync(int usuarioId, IEnumerable<Checkin> checkins);
```

**Arquivo:** `Mentalance/ML/Service/MLService.cs`

**Modificar o método existente:**
```csharp
// Manter o método original para compatibilidade (opcional)
public async Task<AnaliseML> GerarResumoAsync(int usuarioId)
{
    // Busca checkins dos últimos 7 dias
    var checkins = await _checkinRepository.GetByUsuarioEPeriodoAsync(usuarioId);
    var checkinsList = checkins.ToList();
    
    return await GerarResumoAsync(usuarioId, checkinsList);
}

// Novo método que recebe check-ins
public async Task<AnaliseML> GerarResumoAsync(int usuarioId, IEnumerable<Checkin> checkins)
{
    var checkinsList = checkins.ToList();
    // ... resto do código sem buscar check-ins novamente
}
```

**Arquivo:** `Mentalance/Service/AnaliseSemanalService.cs`

**Modificar linha 150:**
```csharp
// ANTES:
var analiseML = await _mlService.GerarResumoAsync(idUsuario);

// DEPOIS:
var analiseML = await _mlService.GerarResumoAsync(idUsuario, checkinsList);
```

**Por quê?**
- Elimina uma query desnecessária ao banco
- Reduz o tempo de resposta
- Menos carga no banco de dados

---

## 🎯 Passo 4: Adicionar Índices no Banco de Dados

### O que fazer:
A query de check-ins pode estar lenta se não houver índices nas colunas `IdUsuario` e `DataCheckin`.

### Como resolver:

**Arquivo:** `Mentalance/Connection/AppDbContext.cs`

**Modificar o método `OnModelCreating` (após linha 46):**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Checkin>()
        .Property(c => c.Emocao)
        .HasConversion<string>();

    // Adicionar índices para melhorar performance
    modelBuilder.Entity<Checkin>()
        .HasIndex(c => c.IdUsuario)
        .HasDatabaseName("IX_Checkin_IdUsuario");

    modelBuilder.Entity<Checkin>()
        .HasIndex(c => new { c.IdUsuario, c.DataCheckin })
        .HasDatabaseName("IX_Checkin_IdUsuario_DataCheckin");
}
```

**Criar uma nova Migration:**

No terminal, execute:
```bash
cd Mentalance
dotnet ef migrations add AdicionarIndicesCheckin
dotnet ef database update
```

**Por quê?**
- Índices aceleram queries que filtram por `IdUsuario` e `DataCheckin`
- O índice composto `(IdUsuario, DataCheckin)` é ideal para a query usada
- Reduz drasticamente o tempo de busca no banco

---

## 📊 Ordem de Implementação Recomendada

Implemente nesta ordem para ver melhorias progressivas:

1. **Passo 1** (Singleton) - Maior impacto, mais fácil
2. **Passo 4** (Índices) - Impacto médio, fácil
3. **Passo 2** (PredictionEngine) - Impacto médio, médio
4. **Passo 3** (Eliminar duplicação) - Impacto menor, mais complexo

---

## 🧪 Como Testar

### Antes das otimizações:
1. Anote o tempo de resposta da análise semanal
2. Use ferramentas como Postman ou Swagger para medir

### Depois de cada passo:
1. Teste novamente e compare os tempos
2. Verifique os logs para confirmar que não há erros

### Métricas esperadas:
- **Passo 1:** Redução de 80-90% no tempo (de ~5-10s para ~1-2s)
- **Passo 4:** Redução adicional de 20-30% nas queries
- **Passo 2:** Redução adicional de 10-20% nas predições
- **Passo 3:** Redução adicional de 5-10% no total

---

## ⚠️ Cuidados e Considerações

### Thread Safety
- `MLService` como Singleton precisa ser thread-safe
- ML.NET geralmente é thread-safe, mas `PredictionEngine` precisa de lock se usado em paralelo
- O código do Passo 2 já inclui locks para segurança

### Memória
- Modelos ML ficam em memória permanentemente (Singleton)
- Se os modelos forem muito grandes, considere lazy loading
- Monitore o uso de memória após implementar

### Migrations
- Sempre teste migrations em ambiente de desenvolvimento primeiro
- Faça backup do banco antes de aplicar migrations em produção
- Verifique se os índices foram criados corretamente

---

## 🔍 Verificação Final

Após implementar todos os passos, verifique:

- [ ] `MLService` está registrado como Singleton no `Program.cs`
- [ ] `PredictionEngine` são criados apenas uma vez no construtor
- [ ] Não há busca duplicada de check-ins
- [ ] Índices foram criados no banco de dados
- [ ] Aplicação inicia sem erros
- [ ] Análise semanal funciona corretamente
- [ ] Tempo de resposta melhorou significativamente

---

## 📝 Notas Adicionais

### Se ainda estiver lento:
1. Verifique o tamanho do arquivo `dadosTreino.json` - se for muito grande, pode demorar para carregar
2. Considere usar cache para análises já geradas
3. Verifique se há problemas de rede/conexão com o banco
4. Monitore o uso de CPU e memória durante a execução

### Próximas otimizações (opcional):
- Cache de análises geradas (evitar regenerar para o mesmo período)
- Processamento assíncrono em background
- Otimização dos modelos ML (reduzir complexidade)

---

## 🆘 Problemas Comuns

### Erro: "PredictionEngine não é thread-safe"
**Solução:** Use locks como mostrado no Passo 2, ou crie um pool de engines.

### Erro: "Migration falhou"
**Solução:** Verifique se o banco está acessível e se você tem permissões para criar índices.

### Erro: "MLService não inicializa"
**Solução:** Verifique se o arquivo `dadosTreino.json` existe e está no caminho correto.

---

**Boa sorte com as otimizações! 🚀**

