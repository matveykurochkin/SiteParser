using NLog;
using SiteParser.Sites;

namespace SiteParser;

static class Program
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static async Task Main(string[] args)
    {
        var wienerBoerseParse = new WienerBoerseParser();
        var url = "https://www.wienerborse.at/en/stocks-prime-market/";
        var csvFile = wienerBoerseParse.LoadQuotesToCsv(url);
        Logger.Info($"Quotes from Wiener Boerse successfuly loaded to [{csvFile}]");

        var shanghaiFuturesExchangeParser = new ShanghaiFuturesExchangeParser();
        csvFile = await shanghaiFuturesExchangeParser.LoadQuotesToCsv("https://www.shfe.com.cn/data/delaymarket_ag.dat",
            CancellationToken.None);
        Logger.Info($"Quotes from Shanghai Futures Exchange successfuly loaded to [{csvFile}]");

        Logger.Info("All done");
    }
}