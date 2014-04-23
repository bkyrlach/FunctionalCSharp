namespace Functional.Value
{
    public sealed class Unit
    {
        public static readonly Unit Only = new Unit();

        private Unit()
        {
        }
    }
}
