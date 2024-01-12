using System.Diagnostics.CodeAnalysis;

namespace Benny;

public readonly struct Literal(int lit)
{
    private readonly int _lit = lit;

    public readonly int Variable => _lit < 0 ? _lit * -1 : _lit;

    public readonly bool Negation => _lit < 0;

    public override readonly string ToString() => $"{(Negation ? "¬" : "")}{Variable}";

    public override readonly int GetHashCode() => _lit;

    public readonly Literal Not() => new(-_lit);

    public bool Equals(Literal other) => _lit == other._lit;

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Literal literal && Equals(literal);

    public static bool operator ==(Literal l1, Literal l2) => l1.Equals(l2);

    public static bool operator !=(Literal l1, Literal l2) => !l1.Equals(l2);
}
