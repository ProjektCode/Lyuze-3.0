namespace Lyuze.Core.Services.Interfaces {
    public interface IStatusProvider {

        IReadOnlyList<string> GetStatuses();
        int GetRandomStatusIndex(IReadOnlyList<string> list);

    }
}
