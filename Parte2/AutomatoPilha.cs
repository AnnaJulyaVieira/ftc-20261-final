using System.Text;

namespace Parte2;

/// <summary>
/// Simulador de Automato de Pilha (AP) com reconhecimento por PILHA VAZIA.
///
/// Modela a 7-tupla formal M = (Q, Sigma, Gamma, delta, q0, Z0, vazio):
///   - Q     : conjunto de estados                         -> <see cref="Estados"/>
///   - Sigma : alfabeto de entrada                         -> <see cref="Alfabeto"/>
///   - Gamma : alfabeto da pilha                           -> <see cref="AlfabetoPilha"/>
///   - delta : Q x (Sigma U {lambda}) x Gamma -> P(Q x Gamma*) -> <see cref="Transicoes"/>
///   - q0    : estado inicial                              -> <see cref="EstadoInicial"/>
///   - Z0    : simbolo inicial da pilha                    -> <see cref="SimboloInicialPilha"/>
///   - F = conjunto vazio: a ACEITACAO E POR PILHA VAZIA.
///
/// A pilha e implementada manualmente como Stack&lt;char&gt; (exigencia do
/// enunciado). Como L3 (palindromos) exige nao-determinismo, a simulacao faz
/// uma busca em profundidade sobre as configuracoes possiveis, clonando a
/// pilha a cada ramo.
/// </summary>
public class AutomatoPilha
{
    /// <summary>Representa o lambda-movimento (transicao vazia), '\0'.</summary>
    public const char Lambda = '\0';

    public HashSet<string> Estados { get; }
    public HashSet<char> Alfabeto { get; }
    public HashSet<char> AlfabetoPilha { get; }

    // delta: a chave e (estado, simboloEntrada|lambda, topoPilha) e o valor e
    // um CONJUNTO de pares (novoEstado, cadeiaParaEmpilhar) -> nao-determinismo.
    // Convencao da "cadeiaParaEmpilhar": o caractere de indice 0 fica no TOPO.
    // Cadeia "" significa apenas desempilhar o topo.
    public Dictionary<(string estado, char entrada, char topo), List<(string novoEstado, string empilhar)>> Transicoes { get; }

    public string EstadoInicial { get; }
    public char SimboloInicialPilha { get; }

    // Limite de passos para evitar laco infinito de lambda-movimentos.
    public int LimitePassos { get; set; } = 20000;

    public AutomatoPilha(
        HashSet<string> estados,
        HashSet<char> alfabeto,
        HashSet<char> alfabetoPilha,
        Dictionary<(string, char, char), List<(string, string)>> transicoes,
        string estadoInicial,
        char simboloInicialPilha)
    {
        Estados = estados;
        Alfabeto = alfabeto;
        AlfabetoPilha = alfabetoPilha;
        Transicoes = transicoes;
        EstadoInicial = estadoInicial;
        SimboloInicialPilha = simboloInicialPilha;
    }

    // Clona uma Stack<char> preservando a ordem (topo continua sendo o topo).
    // new Stack<char>(s) inverte a ordem; aplicar duas vezes restaura.
    private static Stack<char> Clonar(Stack<char> origem) =>
        new Stack<char>(new Stack<char>(origem));

    // Converte a pilha em texto com o topo a esquerda (para exibicao e para a
    // chave do conjunto de visitados).
    private static string PilhaTexto(Stack<char> pilha) =>
        pilha.Count == 0 ? "(vazia)" : new string(pilha.ToArray());

    /// <summary>
    /// Tenta encontrar UMA computacao que aceite a cadeia por pilha vazia
    /// (pilha vazia E entrada totalmente consumida). Retorna o rastro de
    /// configuracoes instantaneas do caminho aceitador, ou null se rejeitar.
    /// </summary>
    public List<string>? Simular(string cadeia)
    {
        var pilhaInicial = new Stack<char>();
        pilhaInicial.Push(SimboloInicialPilha); // Z0 no fundo (e topo inicial)

        var visitados = new HashSet<string>();
        var rastro = new List<string>();
        int passos = 0;

        bool aceita = Busca(EstadoInicial, 0, pilhaInicial, cadeia, visitados, rastro, ref passos);
        return aceita ? rastro : null;
    }

    /// <summary>Atalho booleano: a cadeia pertence a linguagem reconhecida?</summary>
    public bool Aceitar(string cadeia) => Simular(cadeia) is not null;

    // Busca em profundidade pelas configuracoes. Acrescenta a descricao de cada
    // configuracao visitada no caminho corrente a 'rastro'; em caso de falha do
    // ramo, remove (backtracking).
    private bool Busca(string estado, int pos, Stack<char> pilha, string w,
                       HashSet<string> visitados, List<string> rastro, ref int passos)
    {
        if (passos++ > LimitePassos) return false;

        // Registra a configuracao instantanea atual no rastro.
        rastro.Add(Descrever(estado, pos, pilha, w));

        // Condicao de aceitacao: PILHA VAZIA e toda a entrada consumida.
        if (pilha.Count == 0 && pos == w.Length)
            return true;

        // Memoriza configuracoes ja exploradas (estado|pos|pilha) para evitar
        // ciclos. Como a configuracao determina todo o futuro, isso e seguro.
        string chave = $"{estado}|{pos}|{PilhaTexto(pilha)}";
        if (!visitados.Add(chave))
        {
            rastro.RemoveAt(rastro.Count - 1);
            return false;
        }

        // Sem topo nao ha como transitar (e a pilha vazia ja foi tratada acima).
        if (pilha.Count == 0)
        {
            rastro.RemoveAt(rastro.Count - 1);
            return false;
        }

        char topo = pilha.Peek();

        // 1) Transicoes que consomem um simbolo da entrada.
        if (pos < w.Length)
        {
            char simbolo = w[pos];
            if (Transicoes.TryGetValue((estado, simbolo, topo), out var alvos))
            {
                foreach (var (novoEstado, empilhar) in alvos)
                {
                    var novaPilha = AplicarTransicao(pilha, empilhar);
                    if (Busca(novoEstado, pos + 1, novaPilha, w, visitados, rastro, ref passos))
                        return true;
                }
            }
        }

        // 2) Lambda-movimentos (nao consomem entrada).
        if (Transicoes.TryGetValue((estado, Lambda, topo), out var alvosLambda))
        {
            foreach (var (novoEstado, empilhar) in alvosLambda)
            {
                var novaPilha = AplicarTransicao(pilha, empilhar);
                if (Busca(novoEstado, pos, novaPilha, w, visitados, rastro, ref passos))
                    return true;
            }
        }

        rastro.RemoveAt(rastro.Count - 1);
        return false;
    }

    // Desempilha o topo e empilha 'empilhar' (empilhar[0] fica no topo).
    private static Stack<char> AplicarTransicao(Stack<char> pilha, string empilhar)
    {
        var nova = Clonar(pilha);
        nova.Pop(); // remove o topo consumido pela transicao
        for (int i = empilhar.Length - 1; i >= 0; i--)
            nova.Push(empilhar[i]);
        return nova;
    }

    private static string Descrever(string estado, int pos, Stack<char> pilha, string w)
    {
        string restante = pos >= w.Length ? "ε" : w.Substring(pos);
        return $"  estado={estado,-3} | entrada restante='{restante}' | pilha(topo→base)={PilhaTexto(pilha)}";
    }

    /// <summary>
    /// Executa a simulacao e imprime no console a configuracao instantanea de
    /// cada passo, alem do veredito final.
    /// </summary>
    public void ExibirSimulacao(string cadeia, string? rotulo = null)
    {
        string exibicao = cadeia.Length == 0 ? "ε (vazia)" : cadeia;
        Console.WriteLine($"Cadeia: {exibicao}{(rotulo is null ? "" : $"   [{rotulo}]")}");

        var rastro = Simular(cadeia);
        if (rastro is not null)
        {
            foreach (var passo in rastro)
                Console.WriteLine(passo);
            Console.WriteLine("Resultado: ACEITA (pilha esvaziada e entrada consumida)");
        }
        else
        {
            Console.WriteLine("  (nenhuma computacao esvazia a pilha consumindo toda a entrada)");
            Console.WriteLine("Resultado: REJEITA");
        }
        Console.WriteLine(new string('-', 60));
    }
}
