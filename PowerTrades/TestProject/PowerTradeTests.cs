using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using PowerTrades;
using Services;

namespace TestProject
{
    public class PowerTradeTests
    {
        [Test]
        public void GetReportTest()
        {
            // arrange
            var mockPowerService = new Mock<IPowerService>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<PowerTradeExportService>>();

            PowerTradeExportService powerTradeExportService = new(mockPowerService.Object, mockConfiguration.Object, mockLogger.Object);
            var trades = GetTrades();

            // act
            Type serviceType = powerTradeExportService.GetType();
            MethodInfo method = serviceType.GetMethod("GetReport", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = { trades };

            var report = ((IEnumerable<IntraDayReport>)method.Invoke(powerTradeExportService, parameters)).ToArray();

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

            PowerTradeExportService powerTradeExportService = new(mockPowerService.Object, mockConfiguration.Object, mockLogger.Object);
            var filePath = @"C:/Projects/test.csv";

            File.Delete(filePath);

            // act
            Type serviceType = powerTradeExportService.GetType();
            MethodInfo method = serviceType.GetMethod("SaveCsv", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = { filePath, GetReport().ToList() };
            method.Invoke(powerTradeExportService, parameters);

            // assert
            Assert.IsTrue(File.Exists(filePath));
            File.Delete(filePath);
        }

        private IEnumerable<IntraDayReport> GetReport()
        {
            IntraDayReport[] array = (from period in Enumerable.Range(1, 24)
                                      select new IntraDayReport
                                      {
                                          LocalTime = DateTime.Now.Date.AddHours(period).ToShortTimeString(),
                                          Volume = period
                                      }).ToArray();
            return array;
        }

        private IEnumerable<PowerTrade> GetTrades()
        {
            int num = 0;
            PowerTrade[] array = (from _ in Enumerable.Range(0, 3)
                                  select PowerTrade.Create(System.DateTime.Now, 24)).ToArray();

            foreach (PowerTrade obj in array)
            {
                foreach (PowerPeriod powerPeriod in obj.Periods)
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