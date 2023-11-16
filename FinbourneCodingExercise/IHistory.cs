namespace FinbourneCodingExercise
{
    public interface IHistory<T> where T : class
    {
        int Count { get; }

        T? LeastUsedItem { get; }
        T? TheMostRecentlyUsedItem { get; }

        void Record(T item);
        void Remove(T item);
    }
}