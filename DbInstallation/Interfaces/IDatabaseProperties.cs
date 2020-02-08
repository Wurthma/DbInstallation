namespace DbInstallation.Interfaces
{
    public interface IDatabaseProperties
    {
        string DataBaseUser { get; }

        string DatabasePassword { get; }

        string ServerOrTns { get; }

        string TablespaceData { get; }

        string TablespaceIndex { get; }

        string DatabaseName { get; }

        bool IsTrustedConnection { get; }
    }
}
