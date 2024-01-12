namespace Benny;

public struct Assignment
{
    public bool Value { get; set; }

    public Clause? Antecedent { get; set; }

    public int DecisionLevel { get; set; }
}