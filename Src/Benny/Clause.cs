namespace Benny;

public readonly struct Clause(Literal[] literals)
{
    public Literal[] Literals { get; } = literals;

    public readonly Literal FirstWatch => Literals[0];

    public readonly Literal SecondWatch => Literals.Length == 1 ? Literals[0] : Literals[1];

    public readonly override string ToString() => string.Join("∨", Literals.Select(l => l.ToString()));

    public readonly ClauseStatus Status(Assignments assigments)
    {
        var values = new List<bool?>();
        foreach (var literal in Literals) values.Add(assigments.Value(literal));

        if (values.Contains(true)) return ClauseStatus.Satisfied;

        var falseCount = values.Count(v => v == false);
        if (falseCount == values.Count) return ClauseStatus.Unsatisfied;
        if (falseCount == values.Count - 1) return ClauseStatus.Unit;

        return ClauseStatus.Unresolved;
    }

    public readonly void AddToLiteralToClausesIndex(Dictionary<Literal, HashSet<Clause>> literalToClauses)
    {
        if (!literalToClauses.ContainsKey(FirstWatch)) literalToClauses[FirstWatch] = [];
        if (!literalToClauses.ContainsKey(SecondWatch)) literalToClauses[SecondWatch] = [];

        literalToClauses[FirstWatch].Add(this);
        literalToClauses[SecondWatch].Add(this);
    }

    public readonly void SetWatchedLiterals(ILookup<int, Literal> decisionLevelAndLiteral, int maxDecisionLevel, Dictionary<Literal, HashSet<Clause>> literalToClauses)
    {
        if (literalToClauses.TryGetValue(FirstWatch, out var indexedClauses)) indexedClauses.Remove(this);
        if (literalToClauses.TryGetValue(SecondWatch, out var indexedClauses2)) indexedClauses2.Remove(this);

        var toWatchedLiterals = decisionLevelAndLiteral[maxDecisionLevel].ToList();
        for (int i = 0; i < 2 && i < toWatchedLiterals.Count; i++)
        {
            var watchedLiteral = toWatchedLiterals[i];
            var oldIndex = Array.IndexOf(Literals, watchedLiteral);

            Literals[oldIndex] = Literals[i];
            Literals[i] = watchedLiteral;
        }

        AddToLiteralToClausesIndex(literalToClauses);
    }

    public readonly void SwitchWatchedLiteral(Literal oldWatchedLiteral, int indexNewWatchedLiteral, Dictionary<Literal, HashSet<Clause>> literalToClauses)
    {
        var newWatchedLiteral = Literals[indexNewWatchedLiteral];
        Literals[indexNewWatchedLiteral] = oldWatchedLiteral;

        if (FirstWatch == oldWatchedLiteral)
            Literals[0] = newWatchedLiteral;
        else if (SecondWatch == oldWatchedLiteral)
            Literals[1] = newWatchedLiteral;

        literalToClauses[oldWatchedLiteral].Remove(this);
        if (!literalToClauses.ContainsKey(newWatchedLiteral)) literalToClauses[newWatchedLiteral] = [];
        literalToClauses[newWatchedLiteral].Add(this);
    }

    public static Clause Resolve(Clause a, Clause b, int variable)
    {
        var literals = new HashSet<Literal>();
        for (int i = 0; i < a.Literals.Length; i++) 
        { 
            var lit = a.Literals[i];
            if (lit.Variable != variable)
                literals.Add(lit);
        }

        for (int i = 0; i < b.Literals.Length; i++)
        {
            var lit = b.Literals[i];
            if (lit.Variable != variable)
                literals.Add(lit);
        }

        return new(literals.ToArray());
    }
    //=> new(a.Literals.Union(b.Literals).Except([new Literal(variable), new Literal(-variable)]).ToArray());

    public readonly override int GetHashCode()
    {
        var hc = new HashCode();
        foreach (var literal in Literals) hc.Add(literal.GetHashCode());
        return hc.ToHashCode();
    }
}

public enum ClauseStatus
{
    Satisfied,
    Unsatisfied,
    Unit,
    Unresolved
}
