using Parte3;

// =====================================================================
//  PARTE 3 - Simulador de Maquina de Turing
//  L4 = { a^n b^n c^n | n >= 1 }
//  Desafio: MT que COMPUTA f(n) = n + 1 em unario.
// =====================================================================

Console.OutputEncoding = System.Text.Encoding.UTF8;

// ---------------------------------------------------------------------
//  MT RECONHECEDORA de L4 = a^n b^n c^n.
//
//  Estrategia de MARCACAO: a cada passagem marca um 'a' (->X), o proximo
//  'b' (->Y) e o proximo 'c' (->Z), e volta ao inicio. Repete ate nao
//  haver mais 'a'. Por fim, verifica que sobraram apenas Y's e Z's na
//  ordem correta (ou seja, as quantidades batem).
//
//  Estados: q0 (procura a), q1 (procura b), q2 (procura c),
//           q3 (volta ao inicio), q4/q5 (verificacao final),
//           qaccept, qreject.
// ---------------------------------------------------------------------
var deltaL4 = new Dictionary<(string, char), (string, char, char)>
{
    // q0: marca um 'a' como X e vai procurar um 'b'. Se ja for 'Y',
    // nao ha mais 'a' -> inicia a verificacao final.
    { ("q0", 'a'), ("q1", 'X', 'R') },
    { ("q0", 'Y'), ("q4", 'Y', 'R') },

    // q1: anda a direita sobre a's e Y's ate achar um 'b'; marca-o (->Y).
    { ("q1", 'a'), ("q1", 'a', 'R') },
    { ("q1", 'Y'), ("q1", 'Y', 'R') },
    { ("q1", 'b'), ("q2", 'Y', 'R') },

    // q2: anda sobre b's e Z's ate achar um 'c'; marca-o (->Z) e volta.
    { ("q2", 'b'), ("q2", 'b', 'R') },
    { ("q2", 'Z'), ("q2", 'Z', 'R') },
    { ("q2", 'c'), ("q3", 'Z', 'L') },

    // q3: volta a esquerda ate o primeiro X; entao avanca e reinicia em q0.
    { ("q3", 'a'), ("q3", 'a', 'L') },
    { ("q3", 'b'), ("q3", 'b', 'L') },
    { ("q3", 'Y'), ("q3", 'Y', 'L') },
    { ("q3", 'Z'), ("q3", 'Z', 'L') },
    { ("q3", 'X'), ("q0", 'X', 'R') },

    // q4/q5: verificacao final. So pode haver Y's seguidos de Z's e branco.
    { ("q4", 'Y'), ("q4", 'Y', 'R') },
    { ("q4", 'Z'), ("q5", 'Z', 'R') },
    { ("q5", 'Z'), ("q5", 'Z', 'R') },
    { ("q5", '_'), ("qaccept", '_', 'R') },
    // Qualquer transicao nao definida (ex.: sobrou um 'b' ou 'c', ou a
    // entrada esta vazia em q0) leva a rejeicao automatica.
};

var mtL4 = new MaquinaTuring(
    estados: new() { "q0", "q1", "q2", "q3", "q4", "q5", "qaccept", "qreject" },
    alfabetoEntrada: new() { 'a', 'b', 'c' },
    alfabetoFita: new() { 'a', 'b', 'c', 'X', 'Y', 'Z', MaquinaTuring.Branco },
    transicoes: deltaL4,
    estadoInicial: "q0",
    estadoAceitacao: "qaccept",
    estadoRejeicao: "qreject");

Console.WriteLine("############################################################");
Console.WriteLine("#  MT reconhecedora de L4 = { a^n b^n c^n | n >= 1 }");
Console.WriteLine("############################################################\n");

string arqMt = Path.Combine(AppContext.BaseDirectory, "entradas_mt.txt");
string[] casosL4 = File.Exists(arqMt)
    ? File.ReadAllLines(arqMt)
        .Where(l => !l.TrimStart().StartsWith("#"))
        .Select(l => l == "ε" ? "" : l)
        .ToArray()
    : new[] { "abc", "aabbcc", "aaabbbccc", "aabbc", "ab", "abcabc", "" };

foreach (var c in casosL4)
{
    string exibicao = c.Length == 0 ? "ε (vazia)" : c;
    Console.WriteLine($">>> Entrada: {exibicao}");
    var (veredito, passos, _) = mtL4.Executar(c, limitePassos: 1000, exibirPassos: true);
    Console.WriteLine($"=== Resultado: {(veredito == MaquinaTuring.Veredito.Aceita ? "ACEITA" : "REJEITA")} ({passos} passos) ===\n");
}

// ---------------------------------------------------------------------
//  DESAFIO: MT que COMPUTA f(n) = n + 1 em unario.
//  Representacao: n ocorrencias do simbolo '1'. Saida: n+1 ocorrencias.
//
//  Estrategia: caminha ate o primeiro branco a direita do bloco de '1's
//  e o substitui por '1', acrescentando exatamente um simbolo. Encerra.
//
//    (qVai, '1') -> (qVai, '1', 'R')   percorre os 1's
//    (qVai, '_') -> (qaccept, '1', 'R') grava o novo 1 e aceita
// ---------------------------------------------------------------------
var deltaInc = new Dictionary<(string, char), (string, char, char)>
{
    { ("qVai", '1'), ("qVai", '1', 'R') },
    { ("qVai", '_'), ("qaccept", '1', 'R') },
};

var mtIncremento = new MaquinaTuring(
    estados: new() { "qVai", "qaccept", "qreject" },
    alfabetoEntrada: new() { '1' },
    alfabetoFita: new() { '1', MaquinaTuring.Branco },
    transicoes: deltaInc,
    estadoInicial: "qVai",
    estadoAceitacao: "qaccept",
    estadoRejeicao: "qreject");

Console.WriteLine("\n############################################################");
Console.WriteLine("#  DESAFIO - MT computadora de f(n) = n + 1 (unario)");
Console.WriteLine("############################################################\n");

string[] entradasUnario = { "1", "111", "11111" };
foreach (var entrada in entradasUnario)
{
    Console.WriteLine($">>> Entrada: {entrada}  (n = {entrada.Length})");
    var (_, passos, fitaFinal) = mtIncremento.Executar(entrada, limitePassos: 1000, exibirPassos: true);
    Console.WriteLine($"=== Saida na fita: {fitaFinal}  (n+1 = {fitaFinal.Length}) | {passos} passos ===\n");
}
