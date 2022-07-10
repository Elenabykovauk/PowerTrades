using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using PowerTrades;
using Services;

namespace TestProject
{
    public class Tests
    {
        [Test]
        public void GetReportTest()
        {
            // arrange
            var mockPowerService = new Mock<IPowerService>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<PowerTradeExportService>>();

            PowerTradeExportService powerTradeExportService = new PowerTradeExportService(mockPowerService.Object, mockConfiguration.Object, mockLogger.Object);
            var trades = GetTrades();

            // act
            var report = powerTradeExportService.GetReport(trades).ToArray();

            // assert
            Assert.IsNotNull(report);
            Assert.AreEqual(1 + 25 + 49, report[0].Volume);
        }

        [Test]
        public void SaveCsvTest()
        {
            // arrange
            var mockPowerService = new Mock<IPowerService>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<PowerTradeExportService>>();

            PowerTradeExportService powerTradeExportService = new PowerTradeExportService(mockPowerService.Object, mockConfiguration.Object, mockLogger.Object);
            var filePath = @"C:/Projects/test.csv";

            // act
            powerTradeExportService.SaveCsv(filePath, GetTrades());

            // assert
            Assert.IsTrue(File.Exists(filePath));
        }

        private IEnumerable<PowerTrade> GetTrades()
        {
            int num = 0;
            PowerTrade[] array = (from _ in Enumerable.Range(0, 3)
                                  select PowerTrade.Create(System.DateTime.Now, 24)).ToArray(); 
            
            foreach (PowerTrade obj in array)
            {
                foreach(PowerPeriod powerPeriod in obj.Periods)
                {
                    double volume = ((double)(num + 1));
                    powerPeriod.Volume = volume;
                    num++;
                }
            }

            return array;
        }
    }
}