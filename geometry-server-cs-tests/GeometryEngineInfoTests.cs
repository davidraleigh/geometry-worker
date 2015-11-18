using System;
using System.Collections.Generic;
using com.esri.core.geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using geometry_server;

namespace geometry_server_cs_tests {
    [TestClass]
    public class GeometryEngineInfoTests {
        [TestMethod]
        public void EngineMethods() {
            List<String> enumStrings = GeometryEngineInfo.GeometryEngineMethods();
            Assert.IsNotNull(enumStrings);
            Assert.IsFalse(enumStrings.Count == 0);
            Assert.IsTrue(enumStrings.Contains("Buffer"));
        }

        [TestMethod]
        public void MethodDetails() {
            List<String> methodDetails = GeometryEngineInfo.OperatorInfo("Buffer");
            Assert.IsNotNull(methodDetails);
            Assert.IsFalse(methodDetails.Count == 0);

        }
    }
}
