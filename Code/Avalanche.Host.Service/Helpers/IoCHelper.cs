using Avalanche.Host.Service.Enumerations;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avalanche.Host.Service.Helpers
{
    public static class IoCHelper
    {
        static readonly SimpleInjector.Container __container;

        static IoCHelper()
        {
            __container = new Container();
        }

        #region public helpers

        public static void Register<TService, TImpl>(IoCLifestyle lifestyle = IoCLifestyle.Singleton) where TService : class where TImpl : class, TService
        {
            __container.Register<TService, TImpl>(GetLifestyleFromIoCLifestyle(lifestyle));
        }

        public static void Register<TService>(Func<TService> instanceCreator, IoCLifestyle lifestyle = IoCLifestyle.Singleton) where TService : class
        {
            __container.Register<TService>(instanceCreator, GetLifestyleFromIoCLifestyle(lifestyle));
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
