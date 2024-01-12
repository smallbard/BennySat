namespace Benny.Tests
{
    [TestClass]
    public class SolverTests
    {
        [TestMethod]
        public void Formula_2_clauses()
        {
            using var sr = new StringReader("""
p cnf 3 2
1 2 -3 0
-2 3 0
""");

            var formula = Dimacs.Parse(sr);

            var solver = new Solver();
            var assignments = solver.Cdcl(formula);

            Assert.IsNotNull(assignments);
            Assert.IsTrue(assignments.Satisfy(formula));
        }

        [TestMethod]
        public void Formula_3_clauses()
        {
            using var sr = new StringReader("""
p cnf 3 3
1 -2 0
-3 0
3 -1 0
""");

            var formula = Dimacs.Parse(sr);

            var solver = new Solver();
            var assignments = solver.Cdcl(formula);

            Assert.IsNotNull(assignments);
            Assert.IsTrue(assignments.Satisfy(formula));
        }

        [TestMethod]
        public void Formula_4_clauses()
        {
            using var sr = new StringReader("""
p cnf 3 4
1 2 3 0
1 -2 0
2 -3 0
-1 -2 -3 0
""");

            var formula = Dimacs.Parse(sr);

            var solver = new Solver();
            var assignments = solver.Cdcl(formula);

            Assert.IsNotNull(assignments);
            Assert.IsTrue(assignments.Satisfy(formula));
        }

        [TestMethod]
        public void UniformRandom3Sat_First_Example()
        {
            var resourceName = typeof(SolverTests).Assembly.GetManifestResourceNames().First(n => n.Contains("uf20-"));
            using var sr = new StreamReader(typeof(SolverTests).Assembly.GetManifestResourceStream(resourceName)!);
            var formula = Dimacs.Parse(sr);

            var solver = new Solver();
            var assignments = solver.Cdcl(formula);

            Assert.IsNotNull(assignments);
            Assert.IsTrue(assignments.Satisfy(formula));
        }
    }
}