using Chromely;
using Chromely.Core;
using Chromely.Core.Configuration;
using Chromely.Core.Network;

namespace Poc1
{
    class Program
    {
        static void Main(string[] args)
        {
            AppBuilder
            .Create()
            .UseApp<ChromelyBasicApp>()
            .Build()
            .Run(args);
        }
    }
}
