using System;
using System.Collections.Generic;
using System.Text;

namespace DbInstallation.Interfaces
{
    public interface IDatabaseFunctions
    {
        bool TestConnection();

        bool Install();

        bool Update();
    }
}
