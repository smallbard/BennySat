namespace Benny;

public class Formula
{
    private float _bump;
    private readonly Dictionary<int, float> _variablesActity;
    private Task<IEnumerable<int>> _orderedVariablesByActivity;
    private int _lastPickBranchingVariable;

    public HashSet<int> Variables { get; } = [];

    public IList<Clause> Clauses { get; } = [];

    public int Length => Clauses.Count;

    public Formula(IEnumerable<Clause> clauses)
    {
        _variablesActity = new Dictionary<int, float>();

        foreach (var clause in clauses)
        {
            Clauses.Add(clause);
            foreach (var variable in clause.Literals.Select(l => l.Variable))
            {
                if (Variables.Add((variable)))
                    _variablesActity[variable] = 0;
            }
        }

        _orderedVariablesByActivity = Task.FromResult((IEnumerable<int>)Variables);
    }

    public void AddLearntClause(Formula formula, Clause clause)
    {
        //Variable State Independent Decaying Sum
        _bump += 0.1f;
        foreach (var literal in clause.Literals) _variablesActity[literal.Variable] += _bump;
        _orderedVariablesByActivity = Task.Run(() => (IEnumerable<int>)_variablesActity.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).ToList());

        formula.Clauses.Add(clause);
    }

    public (int, bool) PickBranchingVariable(Assignments assignments)
    {
        var variable = _orderedVariablesByActivity.Result.First(v => !assignments.AssignedVariables.Contains(v));
        var value = _lastPickBranchingVariable == variable;
        _lastPickBranchingVariable = variable;

        return (variable, value);
    }

    public override string ToString() => string.Join(" ∧ ", Clauses);
}
