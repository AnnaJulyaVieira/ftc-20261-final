using System.Text;

namespace Parte3;

/// <summary>
/// Simulador de Maquina de Turing (MT) deterministica de fita unica.
///
/// Modela a 7-tupla formal M = (Q, Sigma, Gamma, delta, q0, qaccept, qreject):
///   - Q       : conjunto de estados                      -> <see cref="Estados"/>
///   - Sigma   : alfabeto de entrada (sem o branco)        -> <see cref="AlfabetoEntrada"/>
///   - Gamma   : alfabeto da fita (contem o branco '_')    -> <see cref="AlfabetoFita"/>
///   - delta   : Q x Gamma -> Q x Gamma x {L,R}            -> <see cref="Transicoes"/>
///   - q0      : estado inicial                            -> <see cref="EstadoInicial"/>
///   - qaccept : estado de aceitacao                       -> <see cref="EstadoAceitacao"/>
///   - qreject : estado de rejeicao                        -> <see cref="EstadoRejeicao"/>
///
/// A fita e uma estrutura DINAMICA: Dictionary&lt;int,char&gt; onde a chave e a
/// posicao. Posicoes nao gravadas valem o branco '_'.
/// </summary>
public class MaquinaTuring
{
    /// <summary>Simbolo branco da fita.</summary>
    public const char Branco = '_';

    public HashSet<string> Estados { get; }
    public HashSet<char> AlfabetoEntrada { get; }
    public HashSet<char> AlfabetoFita { get; }

    // delta: (estado, simboloLido) -> (novoEstado, simboloEscrito, direcao 'L'/'R').
    public Dictionary<(string estado, char simbolo), (string novoEstado, char novoSimbolo, char direcao)> Transicoes { get; }

    public string EstadoInicial { get; }
    public string EstadoAceitacao { get; }
    public string EstadoRejeicao { get; }

    public MaquinaTuring(
        HashSet<string> estados,
        HashSet<char> alfabetoEntrada,
        HashSet<char> alfabetoFita,
        Dictionary<(string, char), (string, char, char)> transicoes,
        string estadoInicial,
        string estadoAceitacao,
        string estadoRejeicao)
    {
        Estados = estados;
        AlfabetoEntrada = alfabetoEntrada;
        AlfabetoFita = alfabetoFita;
        Transicoes = transicoes;
        EstadoInicial = estadoInicial;
        EstadoAceitacao = estadoAceitacao;
        EstadoRejeicao = estadoRejeicao;
    }

    public enum Veredito { Aceita, Rejeita, LimiteAtingido }

    /// <summary>
    /// Executa a MT sobre a cadeia de entrada. A cada passo, exibe a
    /// configuracao instantanea (estado, fita com o cabecote entre colchetes e
    /// posicao do cabecote). Para ao atingir qaccept/qreject, ao nao haver
    /// transicao definida (rejeita) ou ao exceder o limite de passos.
    /// </summary>
    /// <param name="entrada">cadeia de entrada</param>
    /// <param name="limitePassos">limite configuravel para evitar laco infinito</param>
    /// <param name="exibirPassos">se true, imprime cada configuracao</param>
    public (Veredito veredito, int passos, string fitaFinal) Executar(
        string entrada, int limitePassos = 1000, bool exibirPassos = true)
    {
        // Inicializa a fita dinamica com a cadeia de entrada.
        var fita = new Dictionary<int, char>();
        for (int i = 0; i < entrada.Length; i++)
            fita[i] = entrada[i];

        int cabecote = 0;
        string estado = EstadoInicial;
        int passos = 0;

        while (true)
        {
            if (exibirPassos)
                ExibirConfiguracao(estado, fita, cabecote, passos);

            // Estados de parada.
            if (estado == EstadoAceitacao)
                return (Veredito.Aceita, passos, LerFita(fita));
            if (estado == EstadoRejeicao)
                return (Veredito.Rejeita, passos, LerFita(fita));

            if (passos >= limitePassos)
            {
                if (exibirPassos)
                    Console.WriteLine($"  [Limite de {limitePassos} passos atingido - execucao interrompida]");
                return (Veredito.LimiteAtingido, passos, LerFita(fita));
            }

            char lido = fita.GetValueOrDefault(cabecote, Branco);

            // delta parcial: sem transicao definida => rejeita explicitamente.
            if (!Transicoes.TryGetValue((estado, lido), out var acao))
            {
                estado = EstadoRejeicao;
                continue; // o laco exibe a config final e retorna Rejeita.
            }

            // Aplica a transicao: escreve, muda de estado e move o cabecote.
            fita[cabecote] = acao.novoSimbolo;
            estado = acao.novoEstado;
            cabecote += acao.direcao == 'R' ? 1 : -1;
            passos++;
        }
    }

    // Imprime: estado, fita com [ ] ao redor do simbolo sob o cabecote, posicao.
    private void ExibirConfiguracao(string estado, Dictionary<int, char> fita, int cabecote, int passos)
    {
        int min = fita.Count > 0 ? Math.Min(fita.Keys.Min(), cabecote) : cabecote;
        int max = fita.Count > 0 ? Math.Max(fita.Keys.Max(), cabecote) : cabecote;

        var sb = new StringBuilder();
        for (int i = min; i <= max; i++)
        {
            char c = fita.GetValueOrDefault(i, Branco);
            sb.Append(i == cabecote ? $"[{c}]" : $" {c} ");
        }

        Console.WriteLine($"Passo {passos,3} | estado={estado,-8} | pos={cabecote,2} | fita: {sb}");
    }

    // Le o conteudo nao-branco da fita como string (para mostrar a saida final).
    private static string LerFita(Dictionary<int, char> fita)
    {
        if (fita.Count == 0) return "(vazia)";
        int min = fita.Keys.Min(), max = fita.Keys.Max();
        var sb = new StringBuilder();
        for (int i = min; i <= max; i++)
            sb.Append(fita.GetValueOrDefault(i, Branco));
        // Remove brancos das extremidades para exibir apenas o conteudo util.
        return sb.ToString().Trim(Branco) is { Length: > 0 } s ? s : "(vazia)";
    }
}
