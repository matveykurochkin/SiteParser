using Newtonsoft.Json;
using NLog;

namespace SiteParser.Sites;

public class ContractData
{
    public string? ContractName { get; set; }
    public string? LastPrice { get; set; }
    public string? UpperDown { get; set; }
    public string? OpenInterest { get; set; }
    public string? Volume { get; set; }
    public string? Turnover { get; set; }
    public string? BidPrice { get; set; }
    public string? AskPrice { get; set; }
    public string? PresettlementPrice { get; set; }
    public string? OpenPrice { get; set; }
    public string? HighPrice { get; set; }
    public string? LowerPrice { get; set; }
}

public class DelayMarket
{
    public List<ContractData>? delaymarket { get; set; }
}

public static class ShanghaiFuturesExchangeParse
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static readonly HttpClient _httpClient = new();
    private static string _url = "https://www.shfe.com.cn/data/delaymarket_ag.dat";

    private static DateTime _today = DateTime.Today;
    private static string _formattedDate = _today.ToString("yyyyMMdd");

    private static string _csvFile = $"ShanghaiFuturesExchange_{_formattedDate}.csv";

    private static string? _contractName;
    private static string? _lastPrice;
    private static string? _upperDown;
    private static string? _openInterest;
    private static string? _volume;
    private static string? _turnover;
    private static string? _bidPrice;
    private static string? _askPrice;
    private static string? _preSettlementPrice;
    private static string? _openPrice;
    private static string? _highPrice;
    private static string? _lowerPrice;

    public static async Task ParseShanghaiFuturesExchange()
    {
        try
        {
            string data = await _httpClient.GetStringAsync(_url);

            DelayMarket? marketData = JsonConvert.DeserializeObject<DelayMarket>(data);

            if (marketData is not null)
            {
                await using StreamWriter writer = new StreamWriter(_csvFile);
                // Записываем заголовок CSV
                await writer.WriteLineAsync(
                    "Contract Name\tLast Price\tUpper Down\tOpen Interest\tVolume\tTurnover\tBid-Ask\tAsk Price\tPre-Settlement Price\tOpen Price\tHigh Price\tLower Price");

                foreach (var contract in marketData.delaymarket!)
                {
                    // Получение значений из объектов ContractData
                    _contractName = contract.ContractName;
                    _lastPrice = contract.LastPrice;
                    _upperDown = contract.UpperDown;
                    _openInterest = contract.OpenInterest;
                    _volume = contract.Volume;
                    _turnover = contract.Turnover;
                    _bidPrice = contract.BidPrice;
                    _askPrice = contract.AskPrice;
                    _preSettlementPrice = contract.PresettlementPrice;
                    _openPrice = contract.OpenPrice;
                    _highPrice = contract.HighPrice;
                    _lowerPrice = contract.LowerPrice;

                    _logger.Info($"\nContract Name: {_contractName}\n" +
                                 $"Last Price: {_lastPrice}\n" +
                                 $"Upper Down: {_upperDown}\n" +
                                 $"Open Interest: {_openInterest}\n" +
                                 $"Volume: {_volume}\n" +
                                 $"Turnover: {_turnover}\n" +
                                 $"Bid Price: {_bidPrice}\n" +
                                 $"Ask Price: {_askPrice}\n" +
                                 $"Pre-Settlement Price: {_preSettlementPrice}\n" +
                                 $"Open Price: {_openPrice}\n" +
                                 $"High Price: {_highPrice}\n" +
                                 $"Lower Price: {_lowerPrice}\n");

                    await writer.WriteLineAsync(
                        $"{_contractName}\t{_lastPrice}\t{_upperDown}\t{_openInterest}\t{_volume}\t{_turnover}\t{_bidPrice}\t{_askPrice}\t{_preSettlementPrice}\t{_openPrice}\t{_highPrice}\t{_lowerPrice}");
                }
            }
        }
        catch (Exception exception)
        {
            _logger.Error("Error in: {0}, error message: {1}", typeof(WienerBoerseParse), exception.Message);
        }
    }
}