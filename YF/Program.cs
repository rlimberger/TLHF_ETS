using System.Diagnostics;
using System.Globalization;
using CsvHelper;
using NodaTime;
using YahooQuotesApi;

class Program
{
    public class ETSCandle
    {
        [CsvHelper.Configuration.Attributes.Name("<TICKER>")]
        public string Ticker { get; set; }
        [CsvHelper.Configuration.Attributes.Name("<PER>")]
        public int Per { get; set; }
        [CsvHelper.Configuration.Attributes.Name("<DATE>")]
        public string Date { get; set; }
        [CsvHelper.Configuration.Attributes.Name("<OPEN>")]
        public double Open { get; set; }
        [CsvHelper.Configuration.Attributes.Name("<HIGH>")]
        public double High { get; set; }
        [CsvHelper.Configuration.Attributes.Name("<LOW>")]
        public double Low { get; set; }
        [CsvHelper.Configuration.Attributes.Name("<CLOSE>")]
        public double Close { get; set; }
        [CsvHelper.Configuration.Attributes.Name("<VOL>")]
        public double Volume { get; set; }
        
        public override string ToString()
        {
            return $"{Ticker} {Per} {Date} {Open} {High} {Low} {Close} {Volume}";
        }
    }

    static async Task Main(string[] args)
    {
        YahooQuotes yahooQuotes = new YahooQuotesBuilder()
            .WithHistoryStartDate(Instant.FromUtc(2020, 1, 1, 0, 0))
            .Build();

        Security security = await yahooQuotes.GetAsync("NVDA", Histories.PriceHistory)
                            ?? throw new ArgumentException("Unknown symbol.");

        PriceTick[] priceHistory = security.PriceHistory.Value;

        List<ETSCandle> candles = new List<ETSCandle>();

        foreach (var day in priceHistory)
        {
            ETSCandle candle = new ETSCandle
            {
                Ticker = "NVDA",
                Per = 1,
                Date = day.Date.ToDateOnly().ToString("yyyyMMdd"),
                Open = day.Open,
                High = day.High,
                Low = day.Low,
                Close = day.Close,
                Volume = day.Volume
            };

            candles.Add(candle);

            Console.WriteLine(candle.ToString());
        }

        var writer = new StreamWriter("C:\\Users\\renel\\OneDrive\\Desktop\\data\\NVDA.csv");
        var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(candles);
        writer.Flush();
        writer.Close();
    }
}


