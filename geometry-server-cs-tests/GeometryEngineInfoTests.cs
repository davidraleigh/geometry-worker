using System;
using System.Collections.Generic;
using com.esri.core.geometry;
using Newtonsoft.Json.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using geometry_server;

namespace geometry_server_cs_tests {
    [TestClass]
    public class GeometryEngineInfoTests {
        [TestMethod]
        public void EngineMethodsTest() {
            List<String> enumStrings = GeometryEngineInfo.GeometryEngineMethods();
            Assert.IsNotNull(enumStrings);
            Assert.IsFalse(enumStrings.Count == 0);
            Assert.IsTrue(enumStrings.Contains("Buffer"));
            Assert.IsFalse(enumStrings.Contains("Project"));
        }

        [TestMethod]
        public void MethodDetailsTest() {
            JObject methodDetails = GeometryEngineInfo.OperatorInfo("Buffer");
            Assert.IsNotNull(methodDetails);
            Assert.IsTrue(methodDetails.HasValues);
            Assert.AreEqual(methodDetails.First.Path, "opertorType");
            Assert.AreEqual(methodDetails.First.Next.Path, "executeMethods");
            Assert.AreEqual(methodDetails.First.First.Value<String>(), "Buffer");
        }

        [TestMethod]
        public void NotImplementedTest() {
            List<String> nonSupported = GeometryEngineInfo.NotImplemented();
            Assert.IsNotNull(nonSupported);
            Assert.IsFalse(nonSupported.Count == 0);
            Assert.IsTrue(nonSupported.Contains("ExportToJson"));
            Assert.IsTrue(nonSupported.Contains("Project"));
        }
    }
}
