# Relatório Técnico — Implementação de Máquinas Abstratas

**Disciplina:** Fundamentos Teóricos da Computação
**Instituição:** Faculdade Cotemig
**Professor:** Júlio César da Silva
**Equipe:** Anna Julya Fernandes (72401192) _(demais integrantes a preencher)_
**Data:** junho de 2026

> **Como gerar o PDF exigido (`docs/relatorio.pdf`):** copie este conteúdo para o
> Word ou Google Docs, aplique a formatação ABNT (Times New Roman ou Arial,
> tamanho 12, espaçamento 1,5; margens superior/esquerda 3 cm e inferior/direita
> 2 cm) e exporte como PDF. Alternativamente, use `pandoc relatorio.md -o relatorio.pdf`.

---

## 1. Introdução

Este trabalho implementa, na linguagem C# (.NET 6), três máquinas abstratas
fundamentais da Teoria da Computação, organizadas em ordem crescente de poder
computacional: o Autômato Finito Determinístico (AFD), o Autômato de Pilha (AP)
com reconhecimento por pilha vazia e a Máquina de Turing (MT). O objetivo é
traduzir fielmente as definições matemáticas formais em estruturas de dados e
algoritmos, evidenciando, na prática, a hierarquia de Chomsky: linguagens
regulares, livres de contexto e recursivamente enumeráveis. Cada máquina foi
construída para reconhecer (ou, no caso da MT, também computar) uma linguagem
específica, com exibição passo a passo das configurações para fins de
verificação e depuração.

## 2. Fundamentação Teórica

### 2.1 Autômato Finito Determinístico (AFD)

Um AFD é uma 5-tupla **M = (Q, Σ, δ, q₀, F)**, onde Q é o conjunto finito de
estados, Σ o alfabeto de entrada, δ: Q × Σ → Q a função de transição (total e
determinística), q₀ ∈ Q o estado inicial e F ⊆ Q o conjunto de estados de
aceitação. Uma cadeia w é aceita se a computação a partir de q₀, consumindo w,
termina em um estado de F. AFDs reconhecem exatamente as **linguagens
regulares**.

**Linguagem-alvo:** `L1 = { w ∈ {a,b}* | w termina com "ab" }`.

**AFD construído:** Q = {q0, q1, q2}, Σ = {a, b}, q0 inicial, F = {q2}.

| δ | a | b |
|---|---|---|
| → q0 | q1 | q0 |
| q1 | q1 | q2 |
| * q2 | q1 | q0 |

Semântica dos estados: **q0** = ainda não há sufixo `ab` em formação; **q1** =
o último símbolo lido foi `a` (candidato a iniciar `ab`); **q2** = os dois
últimos símbolos lidos foram `ab` (aceitação).

**Cálculo manual (2 cadeias):**

- `bab`: q0 --b--> q0 --a--> q1 --b--> q2. Termina em q2 ∈ F → **ACEITA**.
- `ba`: q0 --b--> q0 --a--> q1. Termina em q1 ∉ F → **REJEITA**.

### 2.2 Autômato de Pilha com aceitação por pilha vazia

Um AP é uma 7-tupla **M = (Q, Σ, Γ, δ, q₀, Z₀, ∅)**, onde Γ é o alfabeto da
pilha, Z₀ ∈ Γ o símbolo inicial da pilha e δ: Q × (Σ ∪ {ε}) × Γ → P(Q × Γ*) a
função de transição (possivelmente não-determinística). Na **aceitação por
pilha vazia**, o conjunto de estados finais é vazio: w é aceita se existe uma
computação que consome toda a entrada e esvazia a pilha. APs reconhecem as
**linguagens livres de contexto**.

**Linguagem-alvo:** `L2 = { aⁿbⁿ | n ≥ 1 }`. AP determinístico que empilha um
`A` para cada `a` e desempilha um `A` para cada `b`; um λ-movimento remove `Z₀`
ao final.

**Cálculo manual (2 cadeias):**

- `ab`: (q0, ab, Z) ⊢ (q0, b, AZ) ⊢ (q1, ε, Z) ⊢ (q1, ε, ε). Pilha vazia e
  entrada consumida → **ACEITA**.
- `aab`: (q0, aab, Z) ⊢ (q0, ab, AZ) ⊢ (q0, b, AAZ) ⊢ (q1, ε, AZ). Entrada
  consumida, mas a pilha contém `AZ` ≠ vazia → **REJEITA**.

**Desafio `L3` (palíndromos):** não-determinístico — empilha a primeira metade,
"adivinha" o meio (λ-movimento para comprimento par; consumo do símbolo central
para comprimento ímpar) e desempilha casando a segunda metade.

### 2.3 Máquina de Turing (MT)

Uma MT é uma 7-tupla **M = (Q, Σ, Γ, δ, q₀, q_accept, q_reject)**, com Γ ⊇ Σ
contendo o símbolo branco ⊔ ∉ Σ, e δ: Q × Γ → Q × Γ × {L, R}. A fita é
infinita, o cabeçote lê/escreve e move-se para a esquerda (L) ou direita (R).
MTs reconhecem as **linguagens recursivamente enumeráveis** e formalizam a
noção de algoritmo.

**Linguagem-alvo:** `L4 = { aⁿbⁿcⁿ | n ≥ 1 }`, linguagem **sensível ao
contexto** (não livre de contexto). Estratégia de marcação: a cada passagem,
marca um `a`→`X`, um `b`→`Y`, um `c`→`Z`, e retorna ao início; ao fim verifica
que sobraram apenas `Y`s e `Z`s na ordem correta.

**Cálculo manual (2 cadeias):**

- `abc`: marca a→X, b→Y, c→Z; volta; em q0 lê Y (sem mais `a`), verifica
  `YZ⊔` → **ACEITA**.
- `aabbc`: marca um par (a,b,c) e, na segunda passagem, marca o segundo `a` e
  o segundo `b`, mas não há segundo `c`; o cabeçote alcança o branco em q2 sem
  transição definida → **REJEITA**.

**Desafio `f(n) = n+1` (unário):** a MT percorre o bloco de `1`s até o primeiro
branco e o substitui por `1`, acrescentando exatamente um símbolo.

## 3. Descrição da Implementação

Todas as máquinas foram implementadas sem bibliotecas externas de autômatos,
usando apenas coleções nativas do C#, e cada estrutura de dados foi nomeada de
forma a refletir diretamente a tupla formal.

- **AFD (`Afd.cs`):** os cinco componentes da 5-tupla são propriedades
  explícitas (`Estados`, `Alfabeto`, `Transicoes`, `EstadoInicial`,
  `EstadosAceitacao`). A função δ é um `Dictionary<(string, char), string>`. O
  método `Simular` percorre a cadeia símbolo a símbolo, registrando o rastro; o
  desafio de carga dinâmica usa `System.Text.Json` para ler `afd.json`.

- **AP (`AutomatoPilha.cs`):** a pilha é uma `Stack<char>`, conforme exigido, e
  os λ-movimentos são representados por `'\0'`. Como `L3` é não-determinística,
  a simulação faz uma **busca em profundidade** sobre as configurações
  instantâneas, clonando a pilha a cada ramo e usando um conjunto de
  configurações visitadas (mais um limite de passos) para evitar laços infinitos
  de λ-movimentos. A aceitação é verificada exclusivamente por pilha vazia +
  entrada consumida — nenhum estado final é usado.

- **MT (`MaquinaTuring.cs`):** a fita é um `Dictionary<int,char>` dinâmico
  (branco = `'_'`), e δ é um
  `Dictionary<(string,char),(string,char,char)>`. A execução exibe a cada passo
  o estado, a fita com `[ ]` ao redor do símbolo sob o cabeçote e a posição do
  cabeçote; há `qaccept`/`qreject` explícitos, contador de passos e limite
  configurável. Transições não definidas levam à rejeição automática.

## 4. Resultados e Testes

### 4.1 Parte 1 — AFD (L1)

| Cadeia | Esperado (enunciado) | Obtido | Observação |
|--------|------|--------|------------|
| ab | ACEITA | ACEITA | caso mínimo |
| aab | REJEITA* | **ACEITA** | *o enunciado está incorreto: `aab` termina em `ab` |
| bab | ACEITA | ACEITA | prefixo irrelevante |
| ababab | ACEITA | ACEITA | múltiplas ocorrências |
| ba | REJEITA | REJEITA | termina com `a` |
| ε | REJEITA | REJEITA | sem caracteres |
| b | REJEITA | REJEITA | um símbolo |

### 4.2 Parte 2 — AP (L2 = aⁿbⁿ)

| Cadeia | Esperado | Obtido |
|--------|----------|--------|
| ab | ACEITA | ACEITA |
| aabb | ACEITA | ACEITA |
| aaabbb | ACEITA | ACEITA |
| aab | REJEITA | REJEITA |
| abb | REJEITA | REJEITA |
| ba | REJEITA | REJEITA |
| ε | REJEITA | REJEITA |
| abab | REJEITA | REJEITA |

### 4.3 Parte 2 — AP (L3 = palíndromos)

| Cadeia | Esperado | Obtido |
|--------|----------|--------|
| a | ACEITA | ACEITA |
| aba | ACEITA | ACEITA |
| abba | ACEITA | ACEITA |
| ab | REJEITA | REJEITA |
| aab | REJEITA | REJEITA |

### 4.4 Parte 3 — MT (L4 = aⁿbⁿcⁿ)

| Cadeia | Esperado | Obtido |
|--------|----------|--------|
| abc | ACEITA | ACEITA |
| aabbcc | ACEITA | ACEITA |
| aaabbbccc | ACEITA | ACEITA |
| aabbc | REJEITA | REJEITA |
| ab | REJEITA | REJEITA |
| abcabc | REJEITA | REJEITA |
| ε | REJEITA | REJEITA |

### 4.5 Parte 3 — MT computadora f(n) = n+1

| Fita de entrada | Saída esperada | Saída obtida |
|-----------------|----------------|--------------|
| 1 | 11 | 11 |
| 111 | 1111 | 1111 |
| 11111 | 111111 | 111111 |

## 5. Análise Comparativa

As três máquinas formam uma hierarquia estrita de poder computacional. O **AFD**
tem memória finita (apenas o estado corrente) e reconhece somente linguagens
regulares; é incapaz de "contar" arbitrariamente, por isso não reconhece
`aⁿbⁿ`. O **AP** acrescenta uma pilha (memória ilimitada, mas com acesso
restrito ao topo, LIFO), o que permite reconhecer linguagens livres de contexto
como `aⁿbⁿ` e palíndromos; ainda assim, uma única pilha não basta para `aⁿbⁿcⁿ`,
pois ao verificar os `b` a contagem dos `a` é destruída. A **MT**, com fita
infinita de leitura e escrita bidirecional, supera ambos: reconhece `aⁿbⁿcⁿ` e,
mais do que reconhecer, **computa funções** (como o incremento unário). Em
resumo: Regulares ⊊ Livres de Contexto ⊊ Sensíveis ao Contexto ⊆ Recursivamente
Enumeráveis, com AFD, AP e MT como reconhecedores canônicos das três primeiras
(a MT cobrindo as duas últimas).

## 6. Conclusão

A implementação evidenciou, de forma concreta, por que cada modelo possui o
poder que a teoria lhe atribui: a diferença entre eles está essencialmente na
**memória disponível** e em **como ela pode ser acessada**. Traduzir as tuplas
formais diretamente em estruturas de dados (dicionários para δ, `Stack<char>`
para a pilha, `Dictionary<int,char>` para a fita) tornou a correspondência entre
teoria e código transparente e facilitou a depuração via configurações
instantâneas. O exercício reforçou a compreensão da hierarquia de Chomsky e da
Tese de Church-Turing.

## 7. Referências

1. SIPSER, Michael. *Introduction to the Theory of Computation*. 3. ed. Boston: Cengage, 2013.
2. HOPCROFT, J. E.; MOTWANI, R.; ULLMAN, J. D. *Introdução à Teoria de Autômatos, Linguagens e Computação*. 2. ed. Rio de Janeiro: Campus/Elsevier, 2003.
3. MENEZES, Paulo Blauth. *Linguagens Formais e Autômatos*. 6. ed. Porto Alegre: Bookman, 2010.

---

## Apêndice — Questões Reflexivas

### Parte 1 — AFD

**1. Por que L1 é regular? Expressão regular equivalente.**
`L1` é regular porque pode ser descrita por uma expressão regular e reconhecida
por um AFD com número finito de estados — exatamente a caracterização das
linguagens regulares (Teorema de Kleene). Para reconhecer "termina com `ab`",
basta lembrar, a qualquer momento, apenas se o sufixo lido até agora é vazio,
é `a` ou é `ab`; isso são três situações finitas, sem necessidade de contagem.
A expressão regular equivalente é **(a|b)\*ab**: qualquer prefixo sobre {a,b}
seguido obrigatoriamente do bloco `ab` no final. Como a quantidade de
informação a memorizar é limitada (constante), um autômato de memória finita é
suficiente, o que confirma a regularidade.

**2. 5-tupla formal e justificativa de cada estado.**
M = ({q0,q1,q2}, {a,b}, δ, q0, {q2}). **q0** representa "nenhum progresso rumo
ao sufixo ab" — todo `b` mantém em q0 e todo `a` inicia um possível sufixo.
**q1** representa "acabei de ler um `a`": dele, um `b` completa `ab` (vai a q2)
e outro `a` mantém o candidato (permanece em q1). **q2** representa "os dois
últimos símbolos foram `ab`" e é o único estado de aceitação; um `a` seguinte
recomeça um candidato (q1) e um `b` destrói o sufixo (volta a q0). A escolha de
três estados é mínima: precisamos distinguir exatamente esses três sufixos
possíveis (ε, a, ab).

**3. E se δ não fosse total? Tratamento de símbolos inválidos.**
Formalmente, um AFD exige δ total; quando ela é parcial, o comportamento
convencional é assumir um "estado morto" (poço) implícito do qual não se sai e
que não aceita — equivalente a rejeitar a cadeia. Na implementação, δ é um
dicionário que pode não conter uma chave: quando `(estado, símbolo)` não existe,
o método `Simular` rejeita a cadeia imediatamente. Para símbolos **fora do
alfabeto** Σ (por exemplo `c`), o programa também rejeita e emite uma mensagem
explicando que o símbolo não pertence a Σ. Assim, entradas inválidas não quebram
a execução: são tratadas como rejeição segura, preservando o determinismo.

### Parte 2 — Autômato de Pilha

**1. Por que L2 = aⁿbⁿ não é regular? (Lema do Bombeamento).**
Suponha, por absurdo, que `L2` seja regular. Pelo Lema do Bombeamento, existe um
comprimento de bombeamento `p` tal que toda cadeia `w ∈ L2` com |w| ≥ p pode ser
escrita como `w = xyz`, com |xy| ≤ p, |y| ≥ 1, e `xyⁱz ∈ L2` para todo i ≥ 0.
Tome `w = aᵖbᵖ ∈ L2`. Como |xy| ≤ p, o trecho `xy` está inteiramente na região
de `a`s, logo `y = aᵏ` com k ≥ 1. Bombeando i = 2, obtemos `xy²z = aᵖ⁺ᵏbᵖ`, que
tem mais `a`s do que `b`s e portanto **não pertence a L2**. Isso contradiz o
lema; logo `L2` não é regular. Intuitivamente, um AFD teria de "contar" os `a`s
para compará-los aos `b`s, o que exige memória ilimitada, indisponível em um
autômato de estados finitos.

**2. Aceitação por pilha vazia × por estado final são equivalentes?**
Sim, são equivalentes em poder de reconhecimento: a classe de linguagens aceitas
por APs por pilha vazia é exatamente a classe aceita por APs por estado final —
ambas são as linguagens livres de contexto. A demonstração é construtiva. Dado
um AP que aceita por estado final, constrói-se outro que aceita por pilha vazia
adicionando um novo símbolo de fundo `X₀` (abaixo de Z₀) e, dos estados finais,
λ-movimentos que esvaziam a pilha; um estado "dreno" garante que apenas as
cadeias antes aceitas esvaziem a pilha. Reciprocamente, dado um AP que aceita
por pilha vazia, adiciona-se `X₀` no fundo e um único estado final alcançado por
λ-movimento quando `X₀` aparece no topo (indicando pilha logicamente vazia). As
construções preservam a linguagem, provando a equivalência.

**3. Papel do símbolo Z₀.**
`Z₀` é o marcador de **fundo da pilha**, empilhado antes do início da
computação. Ele cumpre dois papéis na implementação: (i) permite detectar
quando todos os símbolos "úteis" já foram desempilhados — ao ver `Z₀` no topo,
sabe-se que a pilha está "logicamente vazia"; (ii) viabiliza a aceitação por
pilha vazia de forma controlada: um λ-movimento `(q1, λ, Z₀) → (q1, ε)` remove
`Z₀`, esvaziando de fato a pilha somente no momento correto (entrada consumida e
contagem casada). Sem `Z₀`, não haveria como distinguir "pilha vazia por
sucesso" de "pilha que esvaziou cedo demais", nem garantir que `n ≥ 1`.

### Parte 3 — Máquina de Turing

**1. Por que L4 = aⁿbⁿcⁿ não é reconhecida por um AP? Qual propriedade da MT permite?**
`L4` não é livre de contexto, logo nenhum AP a reconhece — o que se prova pelo
Lema do Bombeamento para linguagens livres de contexto. Tomando `z = aᵖbᵖcᵖ` e
qualquer decomposição `z = uvwxy` com |vwx| ≤ p e |vx| ≥ 1, o trecho `vwx`
abrange no máximo dois dos três blocos; bombeando, ao menos um símbolo deixa de
crescer na mesma proporção, quebrando a igualdade das três contagens. A causa
intuitiva é que a **pilha única** do AP, ao comparar `a`s com `b`s, é consumida
e perde a informação necessária para depois comparar com os `c`s. A MT supera
isso porque sua **fita é de leitura e escrita, com movimento bidirecional**:
ela pode marcar símbolos e revisitar qualquer posição quantas vezes for preciso,
mantendo e reutilizando as três contagens simultaneamente.

**2. Quantos estados a MT de L4 usou? Estratégia de marcação.**
A MT de `L4` usa 8 estados: q0 (busca o próximo `a`), q1 (busca o próximo `b`),
q2 (busca o próximo `c`), q3 (retorna ao início), q4 e q5 (verificação final) e
os estados de parada q_accept e q_reject. A **estratégia de marcação** funciona
em rodadas: em cada rodada marca-se um `a` como `X`, caminha-se à direita até o
primeiro `b` marcando-o como `Y`, continua-se até o primeiro `c` marcando-o como
`Z`, e então retorna-se à esquerda até o `X` mais recente para iniciar nova
rodada. Quando q0 encontra `Y` (em vez de `a`), não há mais `a`s; passa-se à
verificação, que aceita apenas se restarem `Y`s seguidos de `Z`s e depois o
branco — garantindo que as três quantidades são iguais.

**3. Um computador moderno é mais poderoso que uma MT? (Tese de Church-Turing).**
Não. Segundo a Tese de Church-Turing, qualquer função efetivamente calculável
pode ser computada por uma Máquina de Turing; nenhum modelo físico de computação
conhecido ultrapassa esse limite de **computabilidade**. Um computador moderno é
mais rápido e conveniente, mas é, na verdade, *menos* poderoso em um sentido
teórico: possui memória **finita**, ao passo que a MT dispõe de fita infinita.
Tudo o que um computador real calcula, uma MT também calcula (e há problemas,
como o da parada, que nenhum dos dois resolve). A diferença é de eficiência e
praticidade, não de poder computacional fundamental: ambos são limitados pela
mesma fronteira da computabilidade.
