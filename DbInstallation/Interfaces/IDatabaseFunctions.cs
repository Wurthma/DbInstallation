using DbInstallation.Enums;

namespace DbInstallation.Interfaces
{
    public interface IDatabaseFunctions
    {
        bool TestConnection();

        bool Install();

        bool Update(int version);

        bool ValidateUpdateVersion();

        bool CheckEmptyDatabase();
    }
}
