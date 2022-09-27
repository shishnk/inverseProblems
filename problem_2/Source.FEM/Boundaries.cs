namespace problem_2.Source;

public readonly record struct DirichletBoundary(int Node, double Value);

public readonly record struct NeumannBoundary(int Elem, int Local1, int Local2, double Theta);