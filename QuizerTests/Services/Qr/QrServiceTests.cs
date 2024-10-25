using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quizer.Services.Qr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quizer.Services.Qr.Tests
{
    [TestClass()]
    public class QrServiceTests
    {
        [TestMethod()]
        public void GenerateQrCodeTest()
        {
            QrService qrService = new QrService();
            qrService.GenerateQrCode("test", "https://quizzypizzy.ru/Lobby/Join/7f946f47-ebc0-4784-8425-0cd469d29880");
            Assert.IsTrue(true);
        }
    }
}