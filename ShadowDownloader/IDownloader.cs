using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowDownloader;

public interface IDownloader
{
    void SetProxy(Uri uri);
    void ClearProxy();
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage req);
    Task<long> Head(string url, Uri referer);
}