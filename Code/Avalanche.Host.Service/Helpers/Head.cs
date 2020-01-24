using Avalanche.Host.Service.Enumerations;
using Avalanche.Host.Service.Services.Logging;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avalanche.Host.Service.Helpers
{
    public static class Head
    {
        static readonly SimpleInjector.Container __container;

        static Head()
        {
            __container = new Container();

            Register<IAppLoggerService, AppLoggerService>();
        }


        #region public helpers

        public static void Register<TService, TImpl>(IoCLifestyle lifestyle = IoCLifestyle.Singleton) where TService : class where TImpl : class, TService
        {
            __container.Register<TService, TImpl>(GetLifestyleFromIoCLifestyle(lifestyle));
        }

        public static TService GetImplementation<TService>() where TService : class
        {
            return __container.GetInstance<TService>();
        }

        #endregion


        #region private helpers

        static Lifestyle GetLifestyleFromIoCLifestyle(IoCLifestyle ls)
        {
            switch (ls)
            {
                case IoCLifestyle.Singleton:
                    return Lifestyle.Singleton;
                case IoCLifestyle.Transient:
                    return Lifestyle.Transient;
                default:
                    return Lifestyle.Scoped;
            }
        }

        #endregion
    }
}
