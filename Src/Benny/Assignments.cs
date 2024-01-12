namespace Benny;

public class Assignments
{
    private readonly Assignment[] _assignments;

    public Assignments(int maxVariable)
    {
        _assignments = new Assignment[maxVariable];
        foreach (ref var assignement in _assignments.AsSpan())
            assignement.DecisionLevel = -1;
    }

    public int DecisionLevel { get; set; }

    public int Count { get; private set; }

    public IEnumerable<int> AssignedVariables
    {
        get
        {
            for (var i = 0; i < _assignments.Length; i++)
                if (_assignments[i].DecisionLevel != -1) yield return i + 1;
        }
    }

    public bool? Value(Literal literal)
    {
        ref readonly var assignment = ref _assignments[literal.Variable - 1];
        if (assignment.DecisionLevel == -1) return null;

        return literal.Negation ? !assignment.Value : assignment.Value;
    }

    public bool IsAssigned(int variable) => _assignments[variable - 1].DecisionLevel != -1;

    public void Assign(int variable, bool value, Clause? antecedent)
    {
        ref var assignment = ref _assignments[variable - 1];
        assignment.Value = value;
        assignment.Antecedent = antecedent;
        assignment.DecisionLevel = DecisionLevel;
        Count++;
    }

    public void Unassign(int variable)
    {
        ref var assignment = ref _assignments[variable - 1];
        assignment.DecisionLevel = -1;
        Count--;
    }

    public bool Satisfy(Formula formula) => !formula.Clauses.Any(c => !c.Literals.Any(l => true == Value(l)));

    public int GetAssignedDecisionLevel(int variable) => _assignments[variable - 1].DecisionLevel;

    public Clause? GetAntecedent(int variable) => _assignments[variable - 1].Antecedent;
}
