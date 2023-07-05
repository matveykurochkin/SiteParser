using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using HtmlAgilityPack;
using NLog;

namespace SiteParser.Sites;

public class WienerBoerseParser
{
    // класс предоставляющий JSON-объект
    public class WienerBoerseQuote
    {
        public WienerBoerseQuote(HtmlNodeCollection cells)
        {
            Name = cells[0].InnerText?.Trim();
            Last = cells[1].InnerText?.Trim();
            ChangeOneDayPercent = cells[2].SelectNodes("div")[0].InnerText?.Trim();
            ChangeOneDayDollar = cells[2].SelectNodes("div")[1].InnerText?.Trim();
            DateTime = cells[3].InnerText?.Trim();
            if (DateTime?.Length > 10)
                DateTime = cells[3].InnerText.Insert(10, " ").Trim();
            MarketCapitalization = cells[4].InnerText?.Trim();
            BidVolume = cells[5].InnerText?.Trim();
            AskVolume = cells[6].InnerText?.Trim();
            TotalVolume = cells[7].InnerText?.Trim();
            TotalValue = cells[8].InnerText?.Trim();
            Status = cells[9].InnerText?.Trim();
        }

        public string? Name { get; }
        public string? Last { get; }
        public string? ChangeOneDayPercent { get; }
        public string? ChangeOneDayDollar { get; }
        public string? DateTime { get; }
        public string? MarketCapitalization { get; }
        public string? BidVolume { get; }
        public string? AskVolume { get; }
        public string? TotalVolume { get; }
        public string? TotalValue { get; }
        public string? Status { get; }
    }

    /// <summary>
    /// Создан чтобы обойти ошибку описанную вот тут
    /// https://github.com/JoshClose/CsvHelper/issues/2069
    /// В ней автоматический маппер не работает
    /// поэтому пришлось реализовать маппер вручную
    /// </summary>
    private sealed class QuoteMapper : ClassMap<WienerBoerseQuote>
    {
        public QuoteMapper()
        {
            //маппер содержит информацию о структуре CSV строки для конкретного класса (порядковый номер колонки и её наименование)
            Map(m => m.Name).Index(0).Name(nameof(WienerBoerseQuote.Name));
            Map(m => m.Last).Index(1).Name(nameof(WienerBoerseQuote.Last));
            Map(m => m.ChangeOneDayPercent).Index(2).Name(nameof(WienerBoerseQuote.ChangeOneDayPercent));
            Map(m => m.ChangeOneDayDollar).Index(3).Name(nameof(WienerBoerseQuote.ChangeOneDayDollar));
            Map(m => m.DateTime).Index(4).Name(nameof(WienerBoerseQuote.DateTime));
            Map(m => m.MarketCapitalization).Index(5).Name(nameof(WienerBoerseQuote.MarketCapitalization));
            Map(m => m.BidVolume).Index(6).Name(nameof(WienerBoerseQuote.BidVolume));
            Map(m => m.AskVolume).Index(7).Name(nameof(WienerBoerseQuote.AskVolume));
            Map(m => m.TotalVolume).Index(8).Name(nameof(WienerBoerseQuote.TotalVolume));
            Map(m => m.TotalValue).Index(9).Name(nameof(WienerBoerseQuote.TotalValue));
            Map(m => m.Status).Index(10).Name(nameof(WienerBoerseQuote.Status));
        }
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static readonly string FormattedDate = DateTime.Today.ToString("yyyyMMdd");

    /// <summary>
    /// метод для парсинга сайта
    /// </summary>
    /// <param name="url"></param>
    /// <returns>пусть к файлу который получился в результате выполнения метода</returns>
    public string? LoadQuotesToCsv(string url)
    {
        try
        {
            HtmlWeb webClient = new();
            HtmlDocument document = webClient.Load(url);

            var path = Directory.GetCurrentDirectory();
            var newPathForResult = Path.Combine(path, "Result");

            if (!Directory.Exists(newPathForResult))
            {
                Logger.Debug("Create directory: {directory}", newPathForResult);
                Directory.CreateDirectory(newPathForResult);
            }

            var csvFile = Path.Combine(newPathForResult, $"WienerBoerse_{FormattedDate}.csv");
            var rows = document.DocumentNode.SelectNodes("//tbody/tr");
            
            if (rows != null)
            {
                Logger.Debug("Start writing to csv file: {file}", csvFile);
                using var writer = new StreamWriter(csvFile);
                // устанавливаем разделитель \t
                using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = "\t"
                });
                
                csv.Context.RegisterClassMap<QuoteMapper>();
                csv.WriteHeader<WienerBoerseQuote>();
                csv.NextRecord();
                // парсим и записываем каждую строку
                csv.WriteRecords(rows.Select(row =>
                {
                    var cells = row.SelectNodes("td");
                    var quote = new WienerBoerseQuote(cells);

                    // записываем полученные данные в log - файл
                    Logger.Info("Recived quote: {@quote}", quote);
                    return quote;
                }));

                return csvFile;
            }

            Logger.Warn("No data. Nothing to save to csv");
            return null;
        }
        catch (Exception exception)
        {
            Logger.Error("Error in: {0}, error message: {1}", typeof(WienerBoerseParser), exception.Message);
            throw;
        }
    }
}