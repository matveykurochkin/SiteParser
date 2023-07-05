using SiteParser.Sites;

namespace SiteParser;

static class Program
{
    public static async Task Main(string[] args)
    {
        WienerBoerseParse.ParseWienerBoerse();
        
        await ShanghaiFuturesExchangeParse.ParseShanghaiFuturesExchange();
    }
}