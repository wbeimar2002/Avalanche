using IsmUtility.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MonsoonAPI
{
    public class EncryptedJsonConfigurationProvider : JsonConfigurationProvider
    {
        public EncryptedJsonConfigurationProvider(EncryptedJsonConfigurationSource source)
            : base(source)
        { }

        public override void Load(Stream stream)
        {
            Stream decrypted = CryptographyUtilities.ReadCryptoDataStream(stream);

            base.Load(decrypted);
        }
    }

    public class EncryptedJsonConfigurationSource : JsonConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            return new EncryptedJsonConfigurationProvider(this);
        }
    }

    public static class EncryptedJsonConfigurationExtensions
    {
        public static IConfigurationBuilder AddEncryptedJsonFile(this IConfigurationBuilder builder, string path)
        {
            return AddEncryptedJsonFile(builder, path, false);
        }

        public static IConfigurationBuilder AddEncryptedJsonFile(this IConfigurationBuilder builder, string path, bool optional)
        {
            return AddEncryptedJsonFile(builder, path, false, false);
        }

        public static IConfigurationBuilder AddEncryptedJsonFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            return AddEncryptedJsonFile(builder, null, path, false, false);
        }

        public static IConfigurationBuilder AddEncryptedJsonFile(this IConfigurationBuilder builder, IFileProvider provider, string path, bool optional, bool reloadOnChange)
        {
            return builder.AddEncryptedJsonFile(s =>
            {
                s.FileProvider = provider;
                s.Path = path;
                s.Optional = optional;
                s.ReloadOnChange = reloadOnChange;
                s.ResolveFileProvider();
            });
        }

        public static IConfigurationBuilder AddEncryptedJsonFile(this IConfigurationBuilder builder, Action<EncryptedJsonConfigurationSource> configureSource)
            => builder.Add(configureSource);
    }
}
