namespace problem_2;

public class ElectroExploration
{
    public class ElectroExplorationBuilder
    {
        private readonly ElectroExploration _electroExploration = new();

        public static implicit operator ElectroExploration(ElectroExplorationBuilder builder)
            => builder._electroExploration;
    }

    public static ElectroExplorationBuilder CreateBuilder() => new();
}