using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using NLog;

namespace SiteParser.Sites;

public class ShanghaiFuturesExchangeParser
{
    // класс предоставляющий JSON-объект
    public class DelayMarketDataItem
    {
        public string? ContractName { get; set; }
        public string? LastPrice { get; set; }
        public string? UpperDown { get; set; }
        public string? OpenInterest { get; set; }
        public string? Volume { get; set; }
        public string? Turnover { get; set; }
        public string? BidPrice { get; set; }
        public string? AskPrice { get; set; }
        public string? PreSettlementPrice { get; set; }
        public string? OpenPrice { get; set; }
        public string? HighPrice { get; set; }
        public string? LowerPrice { get; set; }
    }

    public class DelayMarketInfo
    {
        public List<DelayMarketDataItem>? DelayMarket { get; set; }
    }

    /// <summary>
    /// Создан чтобы обойти ошибку описанную вот тут
    /// https://github.com/JoshClose/CsvHelper/issues/2069
    /// В ней автоматический маппер не работает
    /// поэтому пришлось реализовать маппер вручную
    /// </summary>
    private sealed class DelayMarketMapper : ClassMap<DelayMarketDataItem>
    {
        public DelayMarketMapper()
        {
            //маппер содержит информацию о структуре CSV строки для конкретного класса (порядковый номер колонки и её наименование)
            Map(m => m.ContractName).Index(0).Name(nameof(DelayMarketDataItem.ContractName));
            Map(m => m.LastPrice).Index(1).Name(nameof(DelayMarketDataItem.LastPrice));
            Map(m => m.UpperDown).Index(2).Name(nameof(DelayMarketDataItem.UpperDown));
            Map(m => m.OpenInterest).Index(3).Name(nameof(DelayMarketDataItem.OpenInterest));
            Map(m => m.Volume).Index(4).Name(nameof(DelayMarketDataItem.Volume));
            Map(m => m.Turnover).Index(5).Name(nameof(DelayMarketDataItem.Turnover));
            Map(m => m.BidPrice).Index(6).Name(nameof(DelayMarketDataItem.BidPrice));
            Map(m => m.AskPrice).Index(7).Name(nameof(DelayMarketDataItem.AskPrice));
            Map(m => m.PreSettlementPrice).Index(8).Name(nameof(DelayMarketDataItem.PreSettlementPrice));
            Map(m => m.OpenPrice).Index(9).Name(nameof(DelayMarketDataItem.OpenPrice));
            Map(m => m.HighPrice).Index(10).Name(nameof(DelayMarketDataItem.HighPrice));
            Map(m => m.LowerPrice).Index(11).Name(nameof(DelayMarketDataItem.LowerPrice));
        }
    }

    // это нужно для того, чтобы игнорировать регистр
    private static readonly JsonSerializerOptions Jso = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly string Path = Directory.GetCurrentDirectory();

    private static readonly string FormattedDate = DateTime.Today.ToString("yyyyMMdd");

    /// <summary>
    /// метод для парсинга сайта
    /// </summary>
    /// <param name="url"></param>
    /// <param name="ct"></param>
    /// <returns>пусть к файлу который получился в результате выполнения метода</returns>
    public async Task<string?> LoadQuotesToCsv(string url, CancellationToken ct)
    {
        try
        {
            var newPathForResult = System.IO.Path.Combine(Path, "Result");

            if (!Directory.Exists(newPathForResult))
            {
                Logger.Trace("Create directory: {direcory}", newPathForResult);
                Directory.CreateDirectory(newPathForResult);
            }

            using var httpClient = new HttpClient();
            var marketData = await httpClient.GetFromJsonAsync<DelayMarketInfo>(url, Jso, ct);
            Logger.Debug("Downloaded market data: {@marketData}", marketData);
            
            if (marketData != null)
            {
                var csvFile = System.IO.Path.Combine(newPathForResult, $"ShanghaiFuturesExchange_{FormattedDate}.csv");
                Logger.Info($"Start write to csv file: {csvFile}");
                await using StreamWriter writer = new StreamWriter(csvFile);
                
                // устанавливаем разделитель \t
                await using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = "\t"
                });

                csv.Context.RegisterClassMap<DelayMarketMapper>();
                csv.WriteHeader<DelayMarketDataItem>();
                await csv.NextRecordAsync();
                await csv.WriteRecordsAsync(marketData.DelayMarket, ct);
                return csvFile;
            }

            Logger.Info("Market data empty. Nothing to save to csv");
            return null;
        }
        catch (Exception exception)
        {
            Logger.Error("Error in: {0}, error message: {1}", typeof(WienerBoerseParser), exception.Message);
            throw;
        }
    }
}