namespace SPTarkovLauncher.Config;

internal interface IConfigManager
{
    void Save(string key, string value);
    bool Load(string key, out string value);
}