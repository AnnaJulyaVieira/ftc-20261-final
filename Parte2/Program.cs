using Parte2;

// =====================================================================
//  PARTE 2 - Simulador de Automato de Pilha (aceitacao por PILHA VAZIA)
//  L2 = { a^n b^n | n >= 1 }
//  L3 = { w em {a,b}* | w = w^R, |w| >= 1 }   (palindromos) - desafio
// =====================================================================

Console.OutputEncoding = System.Text.Encoding.UTF8;
const char L = AutomatoPilha.Lambda; // lambda-movimento, '\0'

// ---------------------------------------------------------------------
//  AP para L2 = a^n b^n. Gamma = {Z, A}, Z0 = 'Z'.
//
//  Ideia: empilha um 'A' para cada 'a'; ao ver o primeiro 'b' muda para q1
//  e desempilha um 'A' para cada 'b'. Quando restar apenas Z0 na pilha,
//  um lambda-movimento o remove, esvaziando a pilha (aceitacao).
//
//    (q0, a, Z) -> (q0, "AZ")   empilha A sobre Z0
//    (q0, a, A) -> (q0, "AA")   empilha mais um A
//    (q0, b, A) -> (q1, "")     primeiro b: desempilha A e muda de fase
//    (q1, b, A) -> (q1, "")     demais b: desempilha A
//    (q1, λ, Z) -> (q1, "")     remove Z0 -> pilha vazia
// ---------------------------------------------------------------------
var transL2 = new Dictionary<(string, char, char), List<(string, string)>>
{
    { ("q0", 'a', 'Z'), new() { ("q0", "AZ") } },
    { ("q0", 'a', 'A'), new() { ("q0", "AA") } },
    { ("q0", 'b', 'A'), new() { ("q1", "") } },
    { ("q1", 'b', 'A'), new() { ("q1", "") } },
    { ("q1", L,  'Z'), new() { ("q1", "") } },
};

var apL2 = new AutomatoPilha(
    estados: new() { "q0", "q1" },
    alfabeto: new() { 'a', 'b' },
    alfabetoPilha: new() { 'Z', 'A' },
    transicoes: transL2,
    estadoInicial: "q0",
    simboloInicialPilha: 'Z');

Console.WriteLine("############################################################");
Console.WriteLine("#  AP para L2 = { a^n b^n | n >= 1 } (aceita por pilha vazia)");
Console.WriteLine("############################################################\n");

string arqL2 = Path.Combine(AppContext.BaseDirectory, "entradas_ap.txt");
string[] casosL2 = File.Exists(arqL2)
    ? File.ReadAllLines(arqL2)
        .Where(l => !l.TrimStart().StartsWith("#"))
        .Select(l => l == "ε" ? "" : l)
        .ToArray()
    : new[] { "ab", "aabb", "aaabbb", "aab", "abb", "ba", "", "abab" };

foreach (var c in casosL2)
    apL2.ExibirSimulacao(c);

// ---------------------------------------------------------------------
//  DESAFIO: AP para L3 = palindromos sobre {a,b}, |w| >= 1.
//  Gamma = {Z, a, b}, Z0 = 'Z'.
//
//  Diferenca essencial: L2 e DETERMINISTICA (sabemos onde os 'a' viram 'b').
//  L3 e NAO-DETERMINISTICA: e preciso "adivinhar" o meio da cadeia.
//
//  Fase 1 (q0) - empilha cada simbolo lido (mantendo o topo anterior).
//  Transicao de meio (q0 -> q1):
//    - palindromo PAR  : lambda-movimento, sem consumir simbolo;
//    - palindromo IMPAR: consome o simbolo central sem empilhar.
//  Fase 2 (q1) - para cada simbolo lido, desempilha se casar com o topo.
//  Fim: lambda-movimento remove Z0, esvaziando a pilha.
// ---------------------------------------------------------------------
var transL3 = new Dictionary<(string, char, char), List<(string, string)>>();

void Add(string est, char ent, char topo, string novoEst, string empilhar)
{
    var chave = (est, ent, topo);
    if (!transL3.TryGetValue(chave, out var lista))
        transL3[chave] = lista = new List<(string, string)>();
    lista.Add((novoEst, empilhar));
}

// Fase 1: empilhar o simbolo lido (mantem o topo atual abaixo).
Add("q0", 'a', 'Z', "q0", "aZ");
Add("q0", 'b', 'Z', "q0", "bZ");
foreach (char topo in new[] { 'a', 'b' })
{
    Add("q0", 'a', topo, "q0", "a" + topo);
    Add("q0", 'b', topo, "q0", "b" + topo);
}

// Meio PAR: lambda, muda para q1 sem mexer na pilha (mantem o topo).
Add("q0", L, 'a', "q1", "a");
Add("q0", L, 'b', "q1", "b");

// Meio IMPAR: consome o simbolo central e vai para q1 mantendo a pilha.
foreach (char topo in new[] { 'a', 'b', 'Z' })
{
    Add("q0", 'a', topo, "q1", topo.ToString());
    Add("q0", 'b', topo, "q1", topo.ToString());
}

// Fase 2: casar/desempilhar.
Add("q1", 'a', 'a', "q1", "");
Add("q1", 'b', 'b', "q1", "");

// Fim: remove Z0 -> pilha vazia.
Add("q1", L, 'Z', "q1", "");

var apL3 = new AutomatoPilha(
    estados: new() { "q0", "q1" },
    alfabeto: new() { 'a', 'b' },
    alfabetoPilha: new() { 'Z', 'a', 'b' },
    transicoes: transL3,
    estadoInicial: "q0",
    simboloInicialPilha: 'Z');

Console.WriteLine("\n############################################################");
Console.WriteLine("#  DESAFIO - AP para L3 = palindromos sobre {a,b} (nao-det.)");
Console.WriteLine("############################################################\n");

string[] casosL3 = { "a", "aba", "abba", "ab", "aab" };
foreach (var c in casosL3)
    apL3.ExibirSimulacao(c, rotulo: "L3");
