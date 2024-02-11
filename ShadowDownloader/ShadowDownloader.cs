﻿using System.Diagnostics;
using System.Net.Http.Headers;
using Serilog;
using ShadowDownloader.Adapter;
using ShadowDownloader.Arg;
using ShadowDownloader.Enum;
using ShadowDownloader.Model;

namespace ShadowDownloader;

public class ShadowDownloader
{
    public Configuration Config { get; }
    public List<IAdapter> Adapters { get; } = new();
    public ShadowDownloader(Configuration configuration)
    {
        Config = configuration;
    }

    public ShadowDownloader()
    {
        Config = Configuration.Default();
    }

    public bool AddAdapter(IAdapter adapter)
    {
        if (Adapters.Any(x => x.GetId() == adapter.GetId())) return false;
        Adapters.Add(adapter);
        return true;
    }

    public IAdapter GetAdapter(string id)
    {
        var adapter = Adapters.FirstOrDefault(x => x.GetId() == id);
        if (adapter is null)
        {
            throw new Exception("未找到适配器");
        }

        return adapter;
    }
    public CheckUrlResult CheckUrl(string id,string url)
    {
        var adapter = GetAdapter(id);
        return adapter.CheckUrl(url);
    }

    public async Task<List<CheckFileResult>> CheckFile(string id,CheckUrlResult result)
    {
        var adapter = GetAdapter(id);
        return await adapter.CheckFile(result,Config.SavePath);
    }

    public async Task<int> Download(string id, CheckFileResult result)
    {
        var adapter = GetAdapter(id);
        return await adapter.Download(result,Config);
    }
}