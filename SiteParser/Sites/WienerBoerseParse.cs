using HtmlAgilityPack;
using NLog;

namespace SiteParser.Sites;

public static class WienerBoerseParse
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static readonly string _url = "https://www.wienerborse.at/en/stocks-prime-market/";
    private static HtmlWeb _webClient = new();
    private static HtmlDocument _document = _webClient.Load(_url);

    private static DateTime _today = DateTime.Today;
    private static string _formattedDate = _today.ToString("yyyyMMdd");

    private static string _csvFile = $"WienerBoerse_{_formattedDate}.csv";

    private static string? _name;
    private static string? _last;
    private static string? _changeOneDayPercent;
    private static string? _changeOneDayDollar;
    private static string? _dateTime;
    private static string? _marketCapitalization;
    private static string? _bidVolume;
    private static string? _askVolume;
    private static string? _totalVolume;
    private static string? _totalValue;
    private static string? _status;

    public static void ParseWienerBoerse()
    {
        try
        {
            var rows = _document.DocumentNode.SelectNodes("//tbody/tr");
            if (rows != null)
            {
                using StreamWriter writer = new StreamWriter(_csvFile);
                // Записываем заголовок CSV
                writer.WriteLine(
                    "Name\tLast\tChg. % 1D\tDate Time\tMarket Capitalization\tBid Volume\tAsk Volume\tTotal Volume\tTotal Value\tStatus");

                foreach (var row in rows)
                {
                    var cells = row.SelectNodes("td");

                    _name = cells[0].InnerText;
                    _last = cells[1].InnerText;
                    _changeOneDayPercent = cells[2].SelectNodes("div")[0].InnerText;
                    _changeOneDayDollar = cells[2].SelectNodes("div")[1].InnerText;
                    _dateTime = cells[3].InnerText;
                    if (_dateTime.Length > 10)
                        _dateTime = cells[3].InnerText.Insert(10, " ");
                    _marketCapitalization = cells[4].InnerText;
                    _bidVolume = cells[5].InnerText;
                    _askVolume = cells[6].InnerText;
                    _totalVolume = cells[7].InnerText;
                    _totalValue = cells[8].InnerText;
                    _status = cells[9].InnerText;

                    _logger.Info(
                        $"\nName: {_name}\n" +
                        $"Last: {_last}\n" +
                        $"Chg. %: {_changeOneDayPercent} {_changeOneDayDollar}\n" +
                        $"Date/Time: {_dateTime}\n" +
                        $"Market Capitalization: {_marketCapitalization}\n" +
                        $"Bid Volume: {_bidVolume}\n" +
                        $"Ask Volume: {_askVolume}\n" +
                        $"Total Volume: {_totalVolume}\n" +
                        $"Total Value: {_totalValue}\n" +
                        $"Status: {_status}\n");

                    writer.WriteLine(
                        $"{_name}\t{_last}\t{_changeOneDayPercent} {_changeOneDayPercent}\t{_dateTime}\t{_marketCapitalization}\t{_bidVolume}\t{_askVolume}\t{_totalVolume}\t{_totalValue}\t{_status}");
                }
            }
        }
        catch (Exception exception)
        {
            _logger.Error("Error in: {0}, error message: {1}", typeof(WienerBoerseParse), exception.Message);
        }
    }
}