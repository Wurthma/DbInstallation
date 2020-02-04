using System;
using System.Collections.Generic;
using System.Text;

namespace DbInstallation.Interfaces
{
    public interface IDatabaseProperties
    {
        string DataBaseUser { get; }

        string DatabasePassword { get; }

        string ServerOrTns { get; }
    }
}
