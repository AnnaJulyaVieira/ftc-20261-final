using Parte1;

// =====================================================================
//  PARTE 1 - Simulador Generico de AFD
//  Linguagem-alvo L1 = { w em {a,b}* | w termina com "ab" }
// =====================================================================

Console.OutputEncoding = System.Text.Encoding.UTF8;

// ---------------------------------------------------------------------
// Construcao do AFD para L1 como a 5-tupla formal (Q, Sigma, delta, q0, F).
//
// Estrategia (3 estados):
//   q0 -> ainda nao vi um "a" que possa iniciar o sufixo "ab"
//   q1 -> acabei de ler um "a" (candidato a iniciar "ab")
//   q2 -> acabei de ler "ab" (estado de ACEITACAO)
//
// delta:
//   (q0,a)=q1   (q0,b)=q0
//   (q1,a)=q1   (q1,b)=q2
//   (q2,a)=q1   (q2,b)=q0
// ---------------------------------------------------------------------
var estados = new HashSet<string> { "q0", "q1", "q2" };
var alfabeto = new HashSet<char> { 'a', 'b' };
var transicoes = new Dictionary<(string, char), string>
{
    { ("q0", 'a'), "q1" }, { ("q0", 'b'), "q0" },
    { ("q1", 'a'), "q1" }, { ("q1", 'b'), "q2" },
    { ("q2", 'a'), "q1" }, { ("q2", 'b'), "q0" },
};
var estadoInicial = "q0";
var estadosAceitacao = new HashSet<string> { "q2" };

var afdL1 = new Afd(estados, alfabeto, transicoes, estadoInicial, estadosAceitacao);

Console.WriteLine("############################################################");
Console.WriteLine("#  AFD para L1 = { w em {a,b}* | w termina com \"ab\" }      #");
Console.WriteLine("############################################################\n");

afdL1.ExibirDiagrama();

// ---------------------------------------------------------------------
// Leitura das cadeias de entradas.txt (uma por linha) e simulacao.
// A linha "ε" (ou vazia) representa a cadeia vazia.
// ---------------------------------------------------------------------
string arquivoEntradas = Path.Combine(AppContext.BaseDirectory, "entradas.txt");
if (File.Exists(arquivoEntradas))
{
    Console.WriteLine($"=== Simulacao das cadeias de '{arquivoEntradas}' ===\n");
    foreach (var linhaBruta in File.ReadAllLines(arquivoEntradas))
    {
        // Ignora linhas de comentario que comecam com '#'.
        if (linhaBruta.TrimStart().StartsWith("#")) continue;

        // "ε" e usado no arquivo para indicar a cadeia vazia.
        string cadeia = linhaBruta == "ε" ? "" : linhaBruta;

        var r = afdL1.Simular(cadeia);
        string exibicao = cadeia.Length == 0 ? "ε (vazia)" : cadeia;
        string resultado = r.Aceita ? "ACEITA" : "REJEITA";

        Console.WriteLine($"Cadeia : {exibicao}");
        Console.WriteLine($"Rastro : {string.Join(" -> ", r.Rastro)}");
        if (r.Mensagem is not null)
            Console.WriteLine($"Obs    : {r.Mensagem}");
        Console.WriteLine($"Result : {resultado}");
        Console.WriteLine(new string('-', 50));
    }
}
else
{
    Console.WriteLine($"[Aviso] Arquivo '{arquivoEntradas}' nao encontrado.");
}

// ---------------------------------------------------------------------
// Desafio obrigatorio: carga dinamica de um AFD qualquer a partir de
// afd.json. Aqui carregamos o mesmo L1 a partir do arquivo para mostrar
// que o simulador e generico.
// ---------------------------------------------------------------------
Console.WriteLine("\n############################################################");
Console.WriteLine("#  Desafio: AFD carregado dinamicamente de 'afd.json'      #");
Console.WriteLine("############################################################\n");

string arquivoConfig = Path.Combine(AppContext.BaseDirectory, "afd.json");
try
{
    var afdJson = Afd.CarregarDeJson(arquivoConfig);
    afdJson.ExibirDiagrama();

    // Testa algumas cadeias no AFD carregado do arquivo.
    string[] testes = { "ab", "aab", "bab", "ababab", "ba", "", "b" };
    Console.WriteLine("=== Teste do AFD carregado de afd.json ===\n");
    foreach (var t in testes)
    {
        var r = afdJson.Simular(t);
        string exibicao = t.Length == 0 ? "ε (vazia)" : t;
        Console.WriteLine($"{exibicao,-12} -> {(r.Aceita ? "ACEITA" : "REJEITA"),-8} | rastro: {string.Join(" -> ", r.Rastro)}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[Erro ao carregar afd.json] {ex.Message}");
}
