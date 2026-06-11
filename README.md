# FTC 2026/1 — Trabalho Final: Máquinas Abstratas (AFD, AP e MT)

Implementação em **C# / .NET 6** de três máquinas abstratas da Teoria da
Computação: Autômato Finito Determinístico (AFD), Autômato de Pilha (AP) com
reconhecimento por pilha vazia e Máquina de Turing (MT).

> Disciplina: Fundamentos Teóricos da Computação — Faculdade Cotemig
> Professor: Júlio César da Silva

## Integrantes da equipe

| Nome completo | Matrícula |
|---------------|-----------|
| Anna Julya Fernandes | 72401192 |
| Gabriel Raposo | 72401249 | 

> ⚠️ **Preencham os nomes e matrículas acima** antes de entregar.

## Vídeo de defesa

🔗 https://youtu.be/FS93ILZ6ANM

## Estrutura do repositório

```
ftc-20261-final/
├── Parte1/                 # AFD — reconhecedor de L1 = { w | w termina com "ab" }
│   ├── Afd.cs              # classe AFD (5-tupla formal)
│   ├── Program.cs          # simulação + carga dinâmica via afd.json
│   ├── afd.json            # configuração de AFD genérico (desafio)
│   └── entradas.txt        # casos de teste
├── Parte2/                 # AP — L2 = a^n b^n e desafio L3 = palíndromos
│   ├── AutomatoPilha.cs    # classe AP (7-tupla, aceitação por pilha vazia)
│   ├── Program.cs
│   └── entradas_ap.txt
├── Parte3/                 # MT — L4 = a^n b^n c^n e desafio f(n) = n+1
│   ├── MaquinaTuring.cs    # classe MT (7-tupla, fita dinâmica)
│   ├── Program.cs
│   └── entradas_mt.txt
├── docs/
│   └── relatorio.pdf       # relatório técnico (exportar de relatorio.md)
└── README.md
```

## Pré-requisitos

- [.NET SDK 6.0 ou superior](https://dotnet.microsoft.com/download)

Verifique a instalação com:

```bash
dotnet --version
```

## Como compilar e executar

Cada parte é um projeto independente. A partir da raiz do repositório:

```bash
# Parte 1 — AFD
dotnet run --project Parte1

# Parte 2 — Autômato de Pilha
dotnet run --project Parte2

# Parte 3 — Máquina de Turing
dotnet run --project Parte3
```

Os arquivos de entrada (`entradas.txt`, `entradas_ap.txt`, `entradas_mt.txt` e
`afd.json`) são copiados automaticamente para a pasta de execução.

## Descrição de cada parte

### Parte 1 — AFD (`L1 = { w ∈ {a,b}* | w termina com "ab" }`)
AFD de 3 estados modelado como a 5-tupla `(Q, Σ, δ, q0, F)`. A função de
transição `δ` é um `Dictionary<(string, char), string>`. O programa lê cadeias
de `entradas.txt`, exibe o rastro de estados e o veredito, imprime a tabela de
transições (`ExibirDiagrama`) e carrega um AFD genérico de `afd.json`.

### Parte 2 — Autômato de Pilha (`L2 = a^n b^n`; desafio `L3` = palíndromos)
AP de 7-tupla com **aceitação exclusivamente por pilha vazia**. A pilha é uma
`Stack<char>`, com λ-movimentos representados por `'\0'`. Cada passo exibe a
configuração instantânea (estado, entrada restante, pilha). O AP de `L3`
(palíndromos) é **não-determinístico** e resolvido por busca em profundidade
sobre as configurações.

### Parte 3 — Máquina de Turing (`L4 = a^n b^n c^n`; desafio `f(n) = n+1`)
MT de 7-tupla com fita dinâmica (`Dictionary<int,char>`, branco = `'_'`) e
estratégia de marcação (`X`, `Y`, `Z`). Exibe a configuração a cada passo (com
`[ ]` ao redor do símbolo sob o cabeçote), tem `qaccept`/`qreject` explícitos e
um limite configurável de passos. A segunda MT **computa** o incremento unário.

## Observação importante sobre os casos de teste da Parte 1

A tabela do enunciado lista `aab → REJEITA` ("não termina com ab"). Esse caso
está **incorreto**: a cadeia `aab` termina com o sufixo `ab`, portanto um AFD
fiel à definição formal de `L1` **aceita** `aab`. A implementação segue a
definição formal (que prevalece). Recomenda-se confirmar com o professor.

## Histórico de commits

O desenvolvimento foi versionado de forma incremental (um commit por etapa),
conforme exigido.
