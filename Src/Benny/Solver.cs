namespace Benny;

public class Solver
{
    public Assignments? Cdcl(Formula formula, Assignments? assignments = null)
    {
        var maxVariable = formula.Variables.Max();
        assignments ??= new Assignments(maxVariable);

        var literalToClauses = IndexClausesByWatchedLiterals(formula);

        var toPropagate = new Queue<Literal>();
        InitialUnitClauses(formula, assignments, toPropagate);

        var (reason, clause) = UnitPropagation(assignments, toPropagate, literalToClauses);
        if (reason == Reason.Conflict) return null;

        while (!AllVariablesAssigned(formula, assignments))
        {
            var (variable, value) = formula.PickBranchingVariable(assignments);
            assignments.DecisionLevel++;
            assignments.Assign(variable, value, null);
            toPropagate.Clear();
            toPropagate.Enqueue(new Literal(value ? variable : -variable));

            while (true)
            {
                (reason, clause) = UnitPropagation(assignments, toPropagate, literalToClauses);
                if (reason != Reason.Conflict) break;

                var (backtrackLevel, learntClause) = ConflictAnalysis(clause, assignments, literalToClauses);
                if (backtrackLevel < 0) return null;

                formula.AddLearntClause(formula, learntClause);
                Backtrack(assignments, backtrackLevel, maxVariable);
                assignments.DecisionLevel = backtrackLevel;

                var lit = learntClause.Literals[0];
                assignments.Assign(lit.Variable, !lit.Negation, learntClause);
                toPropagate.Clear();
                toPropagate.Enqueue(lit);
            }
        }

        return assignments;
    }

    private static Dictionary<Literal, HashSet<Clause>> IndexClausesByWatchedLiterals(Formula formula)
    {
        var literalToClauses = new Dictionary<Literal, HashSet<Clause>>();
        foreach (var clause in formula.Clauses)
            clause.AddToLiteralToClausesIndex(literalToClauses);

        return literalToClauses;
    }

    private void InitialUnitClauses(Formula formula, Assignments assignments, Queue<Literal> toPropagate)
    {
        foreach (var clause in formula.Clauses)
            if (clause.Literals.Length == 1)
            {
                var lit = clause.Literals[0];
                assignments.Assign(lit.Variable, !lit.Negation, clause);
                toPropagate.Enqueue(lit);
            }
    }

    private (Reason, Clause) UnitPropagation(Assignments assignments, Queue<Literal> toPropagate, Dictionary<Literal, HashSet<Clause>> literalToClauses)
    {
        while (toPropagate.Count > 0)
        {
            var watchingLiteral = toPropagate.Dequeue().Not();
            if (literalToClauses.ContainsKey(watchingLiteral))
            {
                var watchingClauses = literalToClauses[watchingLiteral];
                foreach (var watchingClause in watchingClauses)
                {
                    for (int i = 2; i < watchingClause.Literals.Length; i++)
                    {
                        var literal = watchingClause.Literals[i];
                        if (assignments.Value(literal) != false)
                        {
                            watchingClause.SwitchWatchedLiteral(watchingLiteral, i, literalToClauses);
                            goto end_of_clause_loop;
                        }
                    }

                    if (watchingClause.FirstWatch == watchingClause.SecondWatch)
                        return (Reason.Conflict, watchingClause);

                    var otherWatchedLiteral = watchingLiteral == watchingClause.FirstWatch ? watchingClause.SecondWatch : watchingClause.FirstWatch;
                    if (!assignments.IsAssigned(otherWatchedLiteral.Variable))
                    {
                        assignments.Assign(otherWatchedLiteral.Variable, !otherWatchedLiteral.Negation, watchingClause);
                        toPropagate.Enqueue(otherWatchedLiteral);
                    }
                    else if (assignments.Value(otherWatchedLiteral) == false)
                        return (Reason.Conflict, watchingClause);

                    end_of_clause_loop:;
                }
            }
        }

        return (Reason.Unresolved, default);
    }

    private (int, Clause) ConflictAnalysis(Clause clause, Assignments assignments, Dictionary<Literal, HashSet<Clause>> literalToClauses)
    {
        if (assignments.DecisionLevel == 0) 
            return (-1, default);

        int CountAssignedLiteralAt(Literal[] literals, int dl) => literals.Count(l => assignments.GetAssignedDecisionLevel(l.Variable) == dl);
        bool HasImpliedLiteralsAt(Literal[] literals, int dl) => literals.Any(l => assignments.GetAssignedDecisionLevel(l.Variable) == dl && assignments.GetAntecedent(l.Variable) != null);

        do
        {
            var literal = clause.Literals.First(l => assignments.GetAssignedDecisionLevel(l.Variable) == assignments.DecisionLevel && assignments.GetAntecedent(l.Variable) != null);
            var antecedent = assignments.GetAntecedent(literal.Variable);
            clause = Clause.Resolve(clause, antecedent!.Value, literal.Variable);

            if (CountAssignedLiteralAt(clause.Literals, assignments.DecisionLevel) == 1) break;

        }
        while (HasImpliedLiteralsAt(clause.Literals, assignments.DecisionLevel));

        var decisionLevelAndLiteral = clause.Literals.ToLookup(l => assignments.GetAssignedDecisionLevel(l.Variable));
        var decisionLevels = decisionLevelAndLiteral.Select(l => l.Key).Distinct().OrderBy(dl => dl).ToList();

        clause.SetWatchedLiterals(decisionLevelAndLiteral, decisionLevels[decisionLevels.Count - 1], literalToClauses);

        if (decisionLevels.Count <= 1)
            return (0, clause);
        return (decisionLevels[^2], clause);
    }

    private void Backtrack(Assignments assignments, int backtrackLevel, int maxVariable)
    {
        var toRemove = new List<int>();
        for (var variable = 1; variable <= maxVariable; variable++)
            if (assignments.GetAssignedDecisionLevel(variable) > backtrackLevel) toRemove.Add(variable);

        foreach (var variable in toRemove) assignments.Unassign(variable);
    }

    private bool AllVariablesAssigned(Formula formula, Assignments assignments) => formula.Variables.Count == assignments.Count;


    public enum Reason
    {
        Conflict,
        Unresolved
    }    
}
