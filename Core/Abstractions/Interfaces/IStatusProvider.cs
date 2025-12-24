namespace Lyuze.Core.Abstractions.Interfaces {
    public interface IStatusProvider {

        IReadOnlyList<string> GetStatuses();
        int GetRandomStatusIndex(IReadOnlyList<string> list);

    }
}
