using System.Text;
using System.Text.Json;

namespace Parte1;

/// <summary>
/// Simulador generico de um Automato Finito Deterministico (AFD).
///
/// Modela diretamente a 5-tupla formal M = (Q, Sigma, delta, q0, F):
///   - Q      : conjunto finito de estados            -> <see cref="Estados"/>
///   - Sigma  : alfabeto finito de entrada            -> <see cref="Alfabeto"/>
///   - delta  : funcao de transicao Q x Sigma -> Q    -> <see cref="Transicoes"/>
///   - q0     : estado inicial                        -> <see cref="EstadoInicial"/>
///   - F      : conjunto de estados de aceitacao      -> <see cref="EstadosAceitacao"/>
///
/// Nenhuma biblioteca externa de automatos e utilizada: a estrutura e
/// construida apenas com colecoes nativas do C#.
/// </summary>
public class Afd
{
    // Q - conjunto finito de estados.
    public HashSet<string> Estados { get; }

    // Sigma - alfabeto finito de entrada.
    public HashSet<char> Alfabeto { get; }

    // delta - funcao de transicao. A chave (estado, simbolo) mapeia para o
    // proximo estado, exatamente como exige o enunciado:
    // Dictionary<(string estado, char simbolo), string>.
    public Dictionary<(string estado, char simbolo), string> Transicoes { get; }

    // q0 - estado inicial.
    public string EstadoInicial { get; }

    // F - conjunto de estados de aceitacao.
    public HashSet<string> EstadosAceitacao { get; }

    public Afd(
        HashSet<string> estados,
        HashSet<char> alfabeto,
        Dictionary<(string, char), string> transicoes,
        string estadoInicial,
        HashSet<string> estadosAceitacao)
    {
        Estados = estados;
        Alfabeto = alfabeto;
        Transicoes = transicoes;
        EstadoInicial = estadoInicial;
        EstadosAceitacao = estadosAceitacao;

        // Validacoes basicas de coerencia da 5-tupla.
        if (!Estados.Contains(EstadoInicial))
            throw new ArgumentException($"Estado inicial '{EstadoInicial}' nao pertence a Q.");
        foreach (var f in EstadosAceitacao)
            if (!Estados.Contains(f))
                throw new ArgumentException($"Estado de aceitacao '{f}' nao pertence a Q.");
    }

    /// <summary>
    /// Resultado detalhado de uma simulacao: aceitacao, rastro de estados
    /// percorridos e uma eventual mensagem (ex.: simbolo invalido).
    /// </summary>
    public record Resultado(bool Aceita, List<string> Rastro, string? Mensagem);

    /// <summary>
    /// Simula a leitura da cadeia simbolo a simbolo a partir de q0,
    /// aplicando delta a cada passo. Aceita se, ao consumir toda a cadeia,
    /// o estado corrente pertence a F.
    /// </summary>
    public Resultado Simular(string cadeia)
    {
        var rastro = new List<string> { EstadoInicial };
        string estadoAtual = EstadoInicial;

        foreach (char simbolo in cadeia)
        {
            // Tratamento de simbolo fora do alfabeto: como delta nao esta
            // definida para ele, a cadeia e rejeitada imediatamente.
            if (!Alfabeto.Contains(simbolo))
            {
                return new Resultado(false, rastro,
                    $"Simbolo '{simbolo}' fora do alfabeto Sigma. Cadeia rejeitada.");
            }

            // delta pode ser parcial: se nao houver transicao definida para
            // (estadoAtual, simbolo), a cadeia e rejeitada (transicao para um
            // "estado morto" implicito).
            if (!Transicoes.TryGetValue((estadoAtual, simbolo), out var proximo))
            {
                return new Resultado(false, rastro,
                    $"Sem transicao definida para ({estadoAtual}, '{simbolo}'). Cadeia rejeitada.");
            }

            estadoAtual = proximo;
            rastro.Add(estadoAtual);
        }

        bool aceita = EstadosAceitacao.Contains(estadoAtual);
        return new Resultado(aceita, rastro, null);
    }

    /// <summary>
    /// Atalho booleano exigido pelo enunciado: bool Aceitar(string cadeia).
    /// </summary>
    public bool Aceitar(string cadeia) => Simular(cadeia).Aceita;

    /// <summary>
    /// Imprime no console uma representacao textual da tabela de transicoes,
    /// marcando o estado inicial com "->" e os de aceitacao com "*".
    /// </summary>
    public void ExibirDiagrama()
    {
        Console.WriteLine("=== Diagrama / Tabela de Transicoes do AFD ===");
        Console.WriteLine($"Q  (estados)          : {{ {string.Join(", ", Estados.OrderBy(s => s))} }}");
        Console.WriteLine($"Sigma (alfabeto)      : {{ {string.Join(", ", Alfabeto.OrderBy(c => c))} }}");
        Console.WriteLine($"q0 (estado inicial)   : {EstadoInicial}");
        Console.WriteLine($"F  (aceitacao)        : {{ {string.Join(", ", EstadosAceitacao.OrderBy(s => s))} }}");
        Console.WriteLine();

        // Cabecalho da tabela: uma coluna por simbolo do alfabeto.
        var simbolos = Alfabeto.OrderBy(c => c).ToList();
        var sb = new StringBuilder();
        sb.Append("Estado".PadRight(12));
        foreach (var s in simbolos)
            sb.Append($"| {s}".PadRight(10));
        Console.WriteLine(sb.ToString());
        Console.WriteLine(new string('-', 12 + simbolos.Count * 10));

        foreach (var estado in Estados.OrderBy(s => s))
        {
            sb.Clear();
            // Prefixos: "->" indica inicial, "*" indica aceitacao.
            string prefixo = "";
            if (estado == EstadoInicial) prefixo += "->";
            if (EstadosAceitacao.Contains(estado)) prefixo += "*";
            sb.Append((prefixo + estado).PadRight(12));

            foreach (var s in simbolos)
            {
                string destino = Transicoes.TryGetValue((estado, s), out var d) ? d : "-";
                sb.Append($"| {destino}".PadRight(10));
            }
            Console.WriteLine(sb.ToString());
        }
        Console.WriteLine();
    }

    // ---------------------------------------------------------------------
    // Desafio obrigatorio: carga de um AFD qualquer a partir de afd.json.
    // ---------------------------------------------------------------------

    // Classes auxiliares que espelham o esquema do arquivo afd.json.
    private class AfdJson
    {
        public List<string> estados { get; set; } = new();
        public List<string> alfabeto { get; set; } = new();
        public string estadoInicial { get; set; } = "";
        public List<string> estadosAceitacao { get; set; } = new();
        public List<TransicaoJson> transicoes { get; set; } = new();
    }

    private class TransicaoJson
    {
        public string origem { get; set; } = "";
        public string simbolo { get; set; } = "";
        public string destino { get; set; } = "";
    }

    /// <summary>
    /// Carrega um AFD a partir de um arquivo de configuracao JSON com o
    /// esquema: estados, alfabeto, estadoInicial, estadosAceitacao,
    /// transicoes (lista de { origem, simbolo, destino }).
    /// </summary>
    public static Afd CarregarDeJson(string caminho)
    {
        if (!File.Exists(caminho))
            throw new FileNotFoundException($"Arquivo de configuracao nao encontrado: {caminho}");

        string conteudo = File.ReadAllText(caminho);
        var dados = JsonSerializer.Deserialize<AfdJson>(conteudo,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("afd.json invalido ou vazio.");

        var estados = new HashSet<string>(dados.estados);
        var alfabeto = new HashSet<char>(dados.alfabeto.Select(a => a[0]));
        var aceitacao = new HashSet<string>(dados.estadosAceitacao);

        var transicoes = new Dictionary<(string, char), string>();
        foreach (var t in dados.transicoes)
        {
            char simbolo = t.simbolo[0];
            transicoes[(t.origem, simbolo)] = t.destino;
        }

        return new Afd(estados, alfabeto, transicoes, dados.estadoInicial, aceitacao);
    }
}
