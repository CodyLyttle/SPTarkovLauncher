using System;
using Microsoft.Win32;

namespace SPTarkovLauncher.Config;

internal class RegistryConfigManager : IConfigManager
{
    private readonly string _registryKeyPath;

    public RegistryConfigManager(string registryKeyPath)
    {
        _registryKeyPath = registryKeyPath;
    }

    public void Save(string key, string value)
    {
        using RegistryKey subKey = CreateSubKey();
        subKey.SetValue(key, value);
    }

    public bool Load(string key, out string value)
    {
        using RegistryKey subKey = CreateSubKey();
        value = (string) subKey.GetValue(key);
        return value is not null;
    }

    private RegistryKey CreateSubKey()
    {
        RegistryKey key = Registry.CurrentUser.CreateSubKey(_registryKeyPath);

        if (key is null)
        {
            throw new InvalidOperationException("Failed to create registry sub key");
        }

        return key;
    }
}