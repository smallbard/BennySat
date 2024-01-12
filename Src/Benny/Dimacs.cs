namespace Benny;

public class Dimacs
{
    public static Formula Parse(TextReader reader)
    {
        var clauses = new List<Clause>();
        for (var line  = reader.ReadLine(); line != null; line = reader.ReadLine()) 
        {
            var tokens = line.Split(null);
            if (tokens.Length > 1 && tokens[0] != "p" && tokens[0] != "c")
            {
                var literals = new List<Literal>(tokens.Length - 1);
                foreach (var token in tokens)
                {
                    if (int.TryParse(token, out int literal))
                    {
                        if (literal == 0 && literals.Count > 0)
                            clauses.Add(new Clause(literals.ToArray()));
                        else
                            literals.Add(new Literal(literal));
                    }
                }
            }
        }

        return new Formula(clauses);
    }
}
